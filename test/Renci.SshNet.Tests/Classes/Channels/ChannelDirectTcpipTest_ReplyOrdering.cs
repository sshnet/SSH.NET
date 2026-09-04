using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Channels
{
    /// <summary>
    /// A forwarded port owns the client socket between <see cref="ChannelDirectTcpip.Open"/> and
    /// <see cref="ChannelDirectTcpip.Bind"/> so that it can write its own reply first —
    /// <see cref="ForwardedPortDynamic"/> writes the SOCKS reply there, and RFC 1928 §6 requires that
    /// reply to precede any byte from the target.
    ///
    /// <see cref="ChannelDirectTcpip.OnData"/> runs on the session's message-receive thread, so if it
    /// wrote to the socket during that window the two threads would race and a target that speaks
    /// first — SSH, SMTP, IMAP, POP3, FTP, MySQL, PostgreSQL, Redis — could land its greeting where
    /// the reply belonged. These tests pin the ordering, and the disposal of the data and of the
    /// socket itself when the target speaks and then immediately hangs up.
    /// </summary>
    [TestClass]
    public class ChannelDirectTcpipTest_ReplyOrdering : TestBase
    {
        private const string Reply = "<REPLY>";
        private const string TargetGreeting = "SSH-2.0-OpenSSH_10.2p1\r\n";

        private Mock<ISession> _sessionMock;
        private Mock<IForwardedPort> _forwardedPortMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private uint _localChannelNumber;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;

        /// <summary>
        /// The socket the channel owns and writes to, i.e. the one the forwarded port accepted.
        /// </summary>
        private Socket _serverSide;

        /// <summary>
        /// The socket the SOCKS client would be holding.
        /// </summary>
        private Socket _clientSide;

        private Socket _listener;

        protected override void OnInit()
        {
            base.OnInit();

            var random = new Random();
            _localChannelNumber = (uint)random.Next(0, int.MaxValue);
            _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);
            _remoteWindowSize = 0x100000;
            _remotePacketSize = 0x8000;

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _forwardedPortMock = new Mock<IForwardedPort>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

            _ = _sessionMock.Setup(p => p.SessionLoggerFactory)
                            .Returns(NullLoggerFactory.Instance);
            _ = _sessionMock.Setup(p => p.IsConnected)
                            .Returns(true);
            _ = _sessionMock.Setup(p => p.ConnectionInfo)
                            .Returns(_connectionInfoMock.Object);
            _ = _connectionInfoMock.Setup(p => p.ChannelCloseTimeout)
                                   .Returns(TimeSpan.FromSeconds(10));
            _ = _sessionMock.Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                            .Callback<WaitHandle>(handle => handle.WaitOne());
            _ = _sessionMock.Setup(p => p.TryWait(It.IsAny<EventWaitHandle>(), It.IsAny<TimeSpan>()))
                            .Returns(WaitResult.Success);
            _ = _sessionMock.Setup(p => p.TrySendMessage(It.IsAny<Message>()))
                            .Returns(true);

            // Confirm the channel open as soon as it is requested, the way a server would. Any other
            // message the channel sends (a window adjust, say) is simply accepted.
            _ = _sessionMock.Setup(p => p.SendMessage(It.IsAny<Message>()))
                            .Callback<Message>(message =>
                                {
                                    if (message is ChannelOpenMessage open)
                                    {
                                        _sessionMock.Raise(p => p.ChannelOpenConfirmationReceived += null,
                                                           new MessageEventArgs<ChannelOpenConfirmationMessage>(
                                                               new ChannelOpenConfirmationMessage(open.LocalChannelNumber,
                                                                                                  _remoteWindowSize,
                                                                                                  _remotePacketSize,
                                                                                                  _remoteChannelNumber)));
                                    }
                                });

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Loopback, 0));
            _listener.Listen(1);

            _clientSide = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            _clientSide.Connect(_listener.LocalEndPoint);
            _serverSide = _listener.Accept();
            _serverSide.NoDelay = true;
        }

        protected override void OnCleanup()
        {
            _clientSide?.Dispose();
            _serverSide?.Dispose();
            _listener?.Dispose();

            base.OnCleanup();
        }

        /// <summary>
        /// The defect itself: data that arrives while the port is still composing its reply must not
        /// be written to the client ahead of that reply.
        /// </summary>
        [TestMethod]
        public void TargetDataArrivingBeforeBindIsWrittenAfterThePortsOwnReply()
        {
            using (var channel = CreateOpenChannel())
            {
                RaiseData(TargetGreeting);

                // Nothing may reach the client yet: the port has not written its reply.
                Assert.AreEqual(0, BytesWaitingForClient(), "the target's greeting was written to the client before the port's reply");

                // The forwarded port writes its reply, then hands the socket over.
                _ = _serverSide.Send(Encoding.ASCII.GetBytes(Reply));

                var bind = RunBind(channel);

                Assert.AreEqual(Reply + TargetGreeting, ReadFromClient(Reply.Length + TargetGreeting.Length));

                FinishBind(bind);
            }
        }

        /// <summary>
        /// Everything buffered has to come out, once, in the order it arrived.
        /// </summary>
        [TestMethod]
        public void EveryChunkBufferedBeforeBindIsDeliveredOnceAndInOrder()
        {
            using (var channel = CreateOpenChannel())
            {
                var expected = new StringBuilder(Reply);

                for (var i = 0; i < 25; i++)
                {
                    var chunk = "chunk-" + i.ToString("D2") + ";";
                    RaiseData(chunk);
                    _ = expected.Append(chunk);
                }

                Assert.AreEqual(0, BytesWaitingForClient());

                _ = _serverSide.Send(Encoding.ASCII.GetBytes(Reply));

                var bind = RunBind(channel);

                Assert.AreEqual(expected.ToString(), ReadFromClient(expected.Length));

                FinishBind(bind);
            }
        }

        /// <summary>
        /// A target that speaks and hangs up at once — sshd refusing a connection, say — must still
        /// get its bytes to the client, after the reply, before the socket is shut down.
        /// </summary>
        [TestMethod]
        public void EofArrivingBeforeBindDoesNotDiscardBufferedDataOrPreEmptTheReply()
        {
            using (var channel = CreateOpenChannel())
            {
                RaiseData(TargetGreeting);
                _sessionMock.Raise(p => p.ChannelEofReceived += null,
                                   new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));

                // The reply must still be writable: the send side may not have been shut down yet.
                _ = _serverSide.Send(Encoding.ASCII.GetBytes(Reply));

                var bind = RunBind(channel);

                Assert.AreEqual(Reply + TargetGreeting, ReadFromClient(Reply.Length + TargetGreeting.Length));

                // and only then the end of the stream
                Assert.AreEqual(0, _clientSide.Receive(new byte[16], 0, 16, SocketFlags.None));

                FinishBind(bind);
            }
        }

        /// <summary>
        /// Same again, but with the whole channel closed before the port got its reply out. The socket
        /// may not be disposed from under the reply.
        /// </summary>
        [TestMethod]
        public void ChannelClosedBeforeBindDoesNotDiscardBufferedDataOrPreEmptTheReply()
        {
            using (var channel = CreateOpenChannel())
            {
                RaiseData(TargetGreeting);
                _sessionMock.Raise(p => p.ChannelEofReceived += null,
                                   new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));
                _sessionMock.Raise(p => p.ChannelCloseReceived += null,
                                   new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));

                Assert.IsFalse(channel.IsOpen, "the channel should have been closed");

                _ = _serverSide.Send(Encoding.ASCII.GetBytes(Reply));

                channel.Bind();

                Assert.AreEqual(Reply + TargetGreeting, ReadFromClient(Reply.Length + TargetGreeting.Length));
                Assert.AreEqual(0, _clientSide.Receive(new byte[16], 0, 16, SocketFlags.None));
            }
        }

        /// <summary>
        /// Once the relay is running, data goes straight to the socket — no buffering, no delay until
        /// some later event.
        /// </summary>
        [TestMethod]
        public void DataArrivingAfterBindIsWrittenStraightToTheClient()
        {
            using (var channel = CreateOpenChannel())
            {
                _ = _serverSide.Send(Encoding.ASCII.GetBytes(Reply));

                var bind = RunBind(channel);

                Assert.AreEqual(Reply, ReadFromClient(Reply.Length));

                RaiseData(TargetGreeting);

                Assert.AreEqual(TargetGreeting, ReadFromClient(TargetGreeting.Length));

                FinishBind(bind);
            }
        }

        /// <summary>
        /// The port going away is an abort, not an orderly close: it must still take the socket down
        /// immediately, which is what unblocks a forwarded port that is stopping.
        /// </summary>
        [TestMethod]
        public void ForwardedPortClosingBeforeBindStillTakesTheSocketDown()
        {
            using (var channel = CreateOpenChannel())
            {
                RaiseData(TargetGreeting);

                _forwardedPortMock.Raise(p => p.Closing += null, EventArgs.Empty);

                // FIN, not the buffered greeting
                Assert.AreEqual(0, _clientSide.Receive(new byte[64], 0, 64, SocketFlags.None));

                // and Bind must not block on a socket that is gone
                channel.Bind();
            }
        }

        private ChannelDirectTcpip CreateOpenChannel()
        {
            var channel = new ChannelDirectTcpip(_sessionMock.Object,
                                                 _localChannelNumber,
                                                 localWindowSize: 0x100000,
                                                 localPacketSize: 0x8000);
            channel.Open("target.example.com", 22, _forwardedPortMock.Object, _serverSide);
            Assert.IsTrue(channel.IsOpen);
            return channel;
        }

        private void RaiseData(string text)
        {
            // The array belongs to the message and is reused by the session, so hand over a distinct
            // one each time — a channel that kept a reference rather than a copy must not pass.
            var payload = Encoding.ASCII.GetBytes(text);
            _sessionMock.Raise(p => p.ChannelDataReceived += null,
                               new MessageEventArgs<ChannelDataMessage>(new ChannelDataMessage(_localChannelNumber, payload)));
            Array.Clear(payload, 0, payload.Length);
        }

        /// <summary>
        /// Bind blocks receiving from the client, so it runs on its own thread.
        /// </summary>
        private static Task RunBind(ChannelDirectTcpip channel)
        {
            var started = new ManualResetEventSlim(false);
            var task = Task.Factory.StartNew(() =>
                {
                    started.Set();
                    channel.Bind();
                }, TaskCreationOptions.LongRunning);
            _ = started.Wait(TimeSpan.FromSeconds(5));
            return task;
        }

        private void FinishBind(Task bind)
        {
            _clientSide.Shutdown(SocketShutdown.Send);
            Assert.IsTrue(bind.Wait(TimeSpan.FromSeconds(10)), "Bind did not return after the client shut down its send side");
        }

        /// <summary>
        /// Returns how many bytes are readable by the client, after giving a wrongly-ordered write
        /// long enough to show up. Loopback delivery is immediate, so this does not race the fault it
        /// is looking for; it only gives it time.
        /// </summary>
        private int BytesWaitingForClient()
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(250);
            while (DateTime.UtcNow < deadline)
            {
                if (_clientSide.Available > 0)
                {
                    return _clientSide.Available;
                }

                Thread.Sleep(10);
            }

            return _clientSide.Available;
        }

        private string ReadFromClient(int count)
        {
            var buffer = new byte[count];
            var read = 0;

            _clientSide.ReceiveTimeout = 10000;

            while (read < count)
            {
                var bytes = _clientSide.Receive(buffer, read, count - read, SocketFlags.None);
                if (bytes == 0)
                {
                    break;
                }

                read += bytes;
            }

            return Encoding.ASCII.GetString(buffer, 0, read);
        }
    }
}
