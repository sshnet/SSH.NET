using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelForwardedTcpipTest_ChannelEofReceived
    {
        private Mock<ISession> _sessionMock;
        private Mock<IForwardedPort> _forwardedPortMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private ChannelForwardedTcpip _channel;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private uint _remoteChannelNumber;
        private TimeSpan _channelCloseTimeout;
        private TcpListener _remoteListener;
        private Socket _remoteSocket;
        private Exception _channelException;
        private Thread _channelThread;
        private List<byte> _dataSentToServer;
        private ManualResetEvent _dataSentToServerWaitHandle;
        private int _bytesReceivedAfterEof;
        private byte[] _dataToSendAfterEof;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void CleanUp()
        {
            _remoteSocket?.Dispose();
            _remoteSocket = null;

            _channelThread?.Join(TimeSpan.FromSeconds(5));
            _channelThread = null;

            _channel?.Dispose();
            _channel = null;

            _remoteListener?.Stop();
            _remoteListener = null;

            _dataSentToServerWaitHandle?.Dispose();
            _dataSentToServerWaitHandle = null;
        }

        private void Arrange()
        {
            var random = new Random();
            _localChannelNumber = (uint)random.Next(0, int.MaxValue);
            _localWindowSize = (uint)random.Next(2000, 3000);
            _localPacketSize = (uint)random.Next(1000, 2000);
            _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint)random.Next(1000, int.MaxValue);
            _remotePacketSize = (uint)random.Next(100, 200);
            _channelCloseTimeout = TimeSpan.FromSeconds(random.Next(10, 20));
            _channelException = null;
            _dataSentToServer = new List<byte>();
            _dataSentToServerWaitHandle = new ManualResetEvent(false);
            _dataToSendAfterEof = new byte[] { 0x0a, 0x0b, 0x0c };

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _forwardedPortMock = new Mock<IForwardedPort>(MockBehavior.Strict);

            _ = _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
            _ = _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _ = _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _ = _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(5));
            _ = _connectionInfoMock.Setup(p => p.ChannelCloseTimeout).Returns(_channelCloseTimeout);
            _ = _sessionMock.Setup(p => p.SendMessage(It.IsAny<ChannelOpenConfirmationMessage>()));
            _ = _sessionMock.Setup(p => p.SendMessage(It.IsAny<ChannelDataMessage>()))
                            .Callback<Message>(m =>
                                {
                                    var dataMessage = (ChannelDataMessage)m;
                                    lock (_dataSentToServer)
                                    {
                                        _dataSentToServer.AddRange(dataMessage.Data.Skip(dataMessage.Offset).Take(dataMessage.Size));
                                    }

                                    _ = _dataSentToServerWaitHandle.Set();
                                });
            _ = _sessionMock.Setup(p => p.TrySendMessage(It.IsAny<ChannelCloseMessage>()))
                            .Returns(true);
            _ = _sessionMock.Setup(p => p.TryWait(It.IsAny<EventWaitHandle>(), _channelCloseTimeout))
                            .Callback<WaitHandle, TimeSpan>((waitHandle, channelCloseTimeout) =>
                                {
                                    _sessionMock.Raise(
                                        s => s.ChannelCloseReceived += null,
                                        new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                                    _ = waitHandle.WaitOne();
                                })
                            .Returns(WaitResult.Success);

            _remoteListener = new TcpListener(IPAddress.Loopback, 0);
            _remoteListener.Start();
            var remoteEndpoint = (IPEndPoint)_remoteListener.LocalEndpoint;

            _channel = new ChannelForwardedTcpip(_sessionMock.Object,
                                                 _localChannelNumber,
                                                 _localWindowSize,
                                                 _localPacketSize,
                                                 _remoteChannelNumber,
                                                 _remoteWindowSize,
                                                 _remotePacketSize);

            _channelThread = new Thread(() =>
            {
                try
                {
                    _channel.Bind(remoteEndpoint, _forwardedPortMock.Object);
                }
                catch (Exception ex)
                {
                    _channelException = ex;
                }
            });
            _channelThread.Start();

            _remoteSocket = _remoteListener.AcceptSocket();
            _remoteSocket.ReceiveTimeout = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
        }

        private void Act()
        {
            _sessionMock.Raise(s => s.ChannelEofReceived += null,
                               new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));

            var buffer = new byte[16];
            _bytesReceivedAfterEof = _remoteSocket.Receive(buffer);

            // only the send direction of the socket should be shut down, so the connected host can still send data
            _ = _remoteSocket.Send(_dataToSendAfterEof);
            _ = _dataSentToServerWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

            _remoteSocket.Shutdown(SocketShutdown.Send);
        }

        [TestMethod]
        public void ConnectedHostShouldReceiveEndOfStream()
        {
            Assert.AreEqual(0, _bytesReceivedAfterEof);
        }

        [TestMethod]
        public void DataSentByConnectedHostAfterEofShouldBeForwardedToServer()
        {
            lock (_dataSentToServer)
            {
                CollectionAssert.AreEqual(_dataToSendAfterEof, _dataSentToServer);
            }
        }

        [TestMethod]
        public void BindShouldFinishWithoutExceptionWhenConnectedHostShutsDownSocket()
        {
            Assert.IsTrue(_channelThread.Join(TimeSpan.FromSeconds(5)));
            Assert.IsNull(_channelException, _channelException?.ToString());
        }
    }
}
