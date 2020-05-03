using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelDirectTcpipTestTest : TestBase
    {
        private Mock<ISession> _sessionMock;
        private Mock<IForwardedPort> _forwardedPortMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private string _remoteHost;
        private uint _port;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private uint _remoteChannelNumber;
        private TimeSpan _channelCloseTimeout;

        protected override void OnInit()
        {
            base.OnInit();

            var random = new Random();

            _localWindowSize = (uint) random.Next(2000, 3000);
            _localPacketSize = (uint) random.Next(1000, 2000);
            _remoteHost = random.Next().ToString(CultureInfo.InvariantCulture);
            _port = (uint) random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort);
            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(100, 200);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _channelCloseTimeout = TimeSpan.FromSeconds(random.Next(10, 20));

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _forwardedPortMock = new Mock<IForwardedPort>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
        }

        [TestMethod]
        public void SocketShouldBeClosedAndBindShouldEndWhenForwardedPortSignalsClosingEvent()
        {
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(p => p.SendMessage(It.IsAny<ChannelOpenMessage>()))
                        .Callback<Message>(m => _sessionMock.Raise(p => p.ChannelOpenConfirmationReceived += null,
                                                                   new MessageEventArgs<ChannelOpenConfirmationMessage>(
                                                                       new ChannelOpenConfirmationMessage(((ChannelOpenMessage) m).LocalChannelNumber,
                                                                                                          _remoteWindowSize,
                                                                                                          _remotePacketSize,
                                                                                                          _remoteChannelNumber))));
            _sessionMock.Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                        .Callback<WaitHandle>(p => p.WaitOne(Session.Infinite));

            var localPortEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            using (var localPortListener = new AsyncSocketListener(localPortEndPoint))
            {
                localPortListener.Start();

                localPortListener.Connected += socket =>
                {
                    var channel = new ChannelDirectTcpip(_sessionMock.Object,
                                                         _localChannelNumber,
                                                         _localWindowSize,
                                                         _localPacketSize);
                    channel.Open(_remoteHost, _port, _forwardedPortMock.Object, socket);

                    var closeForwardedPortThread =
                        new Thread(() =>
                        {
                            // sleep for a short period to allow channel to actually start receiving from socket
                            Thread.Sleep(100);
                            // raise Closing event on forwarded port
                            _forwardedPortMock.Raise(p => p.Closing += null, EventArgs.Empty);
                        });
                    closeForwardedPortThread.Start();

                    channel.Bind();

                    closeForwardedPortThread.Join();
                };

                var client = new Socket(localPortEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(localPortEndPoint);

                // attempt to receive from socket to verify it was shut down by channel
                var buffer = new byte[16];
                var bytesReceived = client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                Assert.AreEqual(0, bytesReceived);
                Assert.IsTrue(client.Connected);
                // signal to server that we also shut down the socket at our end
                client.Shutdown(SocketShutdown.Send);
            }
        }

        [TestMethod]
        public void SocketShouldBeClosedAndBindShouldEndWhenOnErrorOccurredIsInvoked()
        {
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(p => p.SendMessage(It.IsAny<ChannelOpenMessage>()))
                        .Callback<Message>(m => _sessionMock.Raise(p => p.ChannelOpenConfirmationReceived += null,
                                                                   new MessageEventArgs<ChannelOpenConfirmationMessage>(
                                                                       new ChannelOpenConfirmationMessage(((ChannelOpenMessage) m).LocalChannelNumber,
                                                                                                          _remoteWindowSize,
                                                                                                          _remotePacketSize,
                                                                                                          _remoteChannelNumber))));
            _sessionMock.Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                        .Callback<WaitHandle>(p => p.WaitOne(Session.Infinite));

            var localPortEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            using (var localPortListener = new AsyncSocketListener(localPortEndPoint))
            {
                localPortListener.Start();

                localPortListener.Connected += socket =>
                {
                    var channel = new ChannelDirectTcpip(_sessionMock.Object,
                                                         _localChannelNumber,
                                                         _localWindowSize,
                                                         _localPacketSize);
                    channel.Open(_remoteHost, _port, _forwardedPortMock.Object, socket);

                    var signalSessionErrorOccurredThread =
                        new Thread(() =>
                        {
                            // sleep for a short period to allow channel to actually start receiving from socket
                            Thread.Sleep(100);
                            // raise ErrorOccured event on session
                            _sessionMock.Raise(s => s.ErrorOccured += null,
                                               new ExceptionEventArgs(new SystemException()));
                        });
                    signalSessionErrorOccurredThread.Start();

                    channel.Bind();

                    signalSessionErrorOccurredThread.Join();
                };

                var client = new Socket(localPortEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(localPortEndPoint);

                // attempt to receive from socket to verify it was shut down by channel
                var buffer = new byte[16];
                var bytesReceived = client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                Assert.AreEqual(0, bytesReceived);
                Assert.IsTrue(client.Connected);
                // signal to server that we also shut down the socket at our end
                client.Shutdown(SocketShutdown.Send);
            }
        }

        [TestMethod]
        public void SocketShouldBeClosedAndEofShouldBeSentToServerWhenClientShutsDownSocket()
        {
            var sequence = new MockSequence();

            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.SendMessage(It.IsAny<ChannelOpenMessage>()))
                        .Callback<Message>(m => _sessionMock.Raise(p => p.ChannelOpenConfirmationReceived += null,
                                                                   new MessageEventArgs<ChannelOpenConfirmationMessage>(
                                                                       new ChannelOpenConfirmationMessage(((ChannelOpenMessage) m).LocalChannelNumber,
                                                                                                          _remoteWindowSize,
                                                                                                          _remotePacketSize,
                                                                                                          _remoteChannelNumber))));
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                        .Callback<WaitHandle>(p => p.WaitOne(Session.Infinite));
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.TrySendMessage(It.IsAny<ChannelEofMessage>()))
                        .Returns(true)
                        .Callback<Message>(
                            m => new Thread(() =>
                            {
                                Thread.Sleep(50);
                                _sessionMock.Raise(s => s.ChannelEofReceived += null,
                                                   new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));
                            }).Start());
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.TrySendMessage(It.IsAny<ChannelCloseMessage>()))
                        .Returns(true)
                        .Callback<Message>(
                            m => new Thread(() =>
                            {
                                Thread.Sleep(50);
                                _sessionMock.Raise(s => s.ChannelCloseReceived += null,
                                                   new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                            }).Start());
            _sessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _connectionInfoMock.InSequence(sequence).Setup(p => p.ChannelCloseTimeout).Returns(_channelCloseTimeout);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.TryWait(It.IsAny<EventWaitHandle>(), _channelCloseTimeout))
                        .Callback<WaitHandle, TimeSpan>((waitHandle, channelCloseTimeout) => waitHandle.WaitOne())
                        .Returns(WaitResult.Success);

            var channelBindFinishedWaitHandle = new ManualResetEvent(false);
            Socket handler = null;
            ChannelDirectTcpip channel = null;

            var localPortEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            using (var localPortListener = new AsyncSocketListener(localPortEndPoint))
            {
                localPortListener.Start();

                localPortListener.Connected += socket =>
                {
                    channel = new ChannelDirectTcpip(_sessionMock.Object,
                                                     _localChannelNumber,
                                                     _localWindowSize,
                                                     _localPacketSize);
                    channel.Open(_remoteHost, _port, _forwardedPortMock.Object, socket);
                    channel.Bind();
                    channel.Dispose();

                    handler = socket;

                    channelBindFinishedWaitHandle.Set();
                };

                var client = new Socket(localPortEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                client.Connect(localPortEndPoint);
                client.Shutdown(SocketShutdown.Send);
                Assert.IsFalse(client.Connected);

                channelBindFinishedWaitHandle.WaitOne();

                Assert.IsNotNull(handler);
                Assert.IsFalse(handler.Connected);

                _sessionMock.Verify(p => p.TrySendMessage(It.IsAny<ChannelEofMessage>()), Times.Once);
                _sessionMock.Verify(p => p.TrySendMessage(It.IsAny<ChannelCloseMessage>()), Times.Once);

                channel.Dispose();

                _sessionMock.Verify(p => p.TrySendMessage(It.IsAny<ChannelEofMessage>()), Times.Once);
                _sessionMock.Verify(p => p.TrySendMessage(It.IsAny<ChannelCloseMessage>()), Times.Once);
            }
        }
    }
}