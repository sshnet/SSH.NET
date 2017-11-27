using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
#if !FEATURE_SOCKET_DISPOSE
using Renci.SshNet.Common;
#endif // !FEATURE_SOCKET_DISPOSE
using Renci.SshNet.Channels;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelDirectTcpipTest_Dispose_SessionIsConnectedAndChannelIsOpen
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private Mock<IForwardedPort> _forwardedPortMock;
        private ChannelDirectTcpip _channel;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private uint _remoteChannelNumber;
        private TimeSpan _channelCloseTimeout;
        private string _remoteHost;
        private uint _port;
        private AsyncSocketListener _listener;
        private EventWaitHandle _channelBindFinishedWaitHandle;
        private EventWaitHandle _clientReceivedFinishedWaitHandle;
        private Socket _client;
        private Exception _channelException;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }

            if (_listener != null)
            {
                _listener.Stop();
                _listener = null;
            }
        }

        private void Arrange()
        {
            var random = new Random();

            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(2000, 3000);
            _localPacketSize = (uint) random.Next(1000, 2000);
            _channelCloseTimeout = TimeSpan.FromSeconds(random.Next(10, 20));
            _remoteHost = random.Next().ToString(CultureInfo.InvariantCulture);
            _port = (uint) random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort);
            _channelBindFinishedWaitHandle = new ManualResetEvent(false);
            _clientReceivedFinishedWaitHandle = new ManualResetEvent(false);
            _channelException = null;

            _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint)random.Next(0, int.MaxValue);
            _remotePacketSize = (uint)random.Next(100, 200);

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _forwardedPortMock = new Mock<IForwardedPort>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence)
                .Setup(p => p.SendMessage(It.Is<ChannelOpenMessage>(m => AssertExpectedMessage(m))));
            _sessionMock.InSequence(sequence)
                .Setup(p => p.WaitOnHandle(It.IsNotNull<WaitHandle>()))
                .Callback<WaitHandle>(
                    w =>
                    {
                        _sessionMock.Raise(
                            s => s.ChannelOpenConfirmationReceived += null,
                            new MessageEventArgs<ChannelOpenConfirmationMessage>(
                                new ChannelOpenConfirmationMessage(
                                    _localChannelNumber,
                                    _remoteWindowSize,
                                    _remotePacketSize,
                                    _remoteChannelNumber)));
                        w.WaitOne();
                    });
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence)
                .Setup(
                    p => p.TrySendMessage(It.Is<ChannelEofMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                .Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence)
                .Setup(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                .Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _connectionInfoMock.InSequence(sequence).Setup(p => p.ChannelCloseTimeout).Returns(_channelCloseTimeout);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.TryWait(It.IsAny<EventWaitHandle>(), _channelCloseTimeout))
                        .Callback<WaitHandle, TimeSpan>((waitHandle, channelCloseTimeout) =>
                        {
                            _sessionMock.Raise(
                                s => s.ChannelCloseReceived += null,
                                new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                            waitHandle.WaitOne();
                        })
                        .Returns(WaitResult.Success);

            var localEndpoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _listener = new AsyncSocketListener(localEndpoint);
            _listener.Connected += socket =>
                {
                    try
                    {
                        _channel = new ChannelDirectTcpip(_sessionMock.Object,
                                                          _localChannelNumber,
                                                          _localWindowSize,
                                                          _localPacketSize);
                        _channel.Open(_remoteHost, _port, _forwardedPortMock.Object, socket);
                        _channel.Bind();
                    }
                    catch (Exception ex)
                    {
                        _channelException = ex;
                    }
                    finally
                    {
                        _channelBindFinishedWaitHandle.Set();
                    }
                };
            _listener.Start();

            _client = new Socket(localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(localEndpoint);

            var clientReceiveThread = new Thread(
                () =>
                    {
                        var buffer = new byte[16];
                        var bytesReceived = _client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                        if (bytesReceived == 0)
                        {
                            _client.Shutdown(SocketShutdown.Send);
                            _clientReceivedFinishedWaitHandle.Set();
                        }
                    }
                );
            clientReceiveThread.Start();

            // give channel time to bind to socket
            Thread.Sleep(200);
        }

        private void Act()
        {
            if (_channel != null)
            {
                _channel.Dispose();
            }
        }

        [TestMethod]
        public void BindShouldHaveFinishedWithoutException()
        {
            Assert.IsTrue(_channelBindFinishedWaitHandle.WaitOne(0));
            Assert.IsNull(_channelException, _channelException != null ? _channelException.ToString() : null);
        }

        [TestMethod]
        public void ClientShouldHaveFinished()
        {
            Assert.IsTrue(_clientReceivedFinishedWaitHandle.WaitOne(0));
        }

        [TestMethod]
        public void ChannelEofMessageShouldBeSentOnce()
        {
            _sessionMock.Verify(p => p.TrySendMessage(It.Is<ChannelEofMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)), Times.Once);
        }

        [TestMethod]
        public void ChannelCloseMessageShouldBeSentOnce()
        {
            _sessionMock.Verify(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)), Times.Once);
        }

        [TestMethod]
        public void IsOpenShouldReturnFalse()
        {
            Assert.IsFalse(_channel.IsOpen);
        }

        private bool AssertExpectedMessage(ChannelOpenMessage channelOpenMessage)
        {
            if (channelOpenMessage == null)
                return false;
            if (channelOpenMessage.LocalChannelNumber != _localChannelNumber)
                return false;
            if (channelOpenMessage.InitialWindowSize != _localWindowSize)
                return false;
            if (channelOpenMessage.MaximumPacketSize != _localPacketSize)
                return false;

            var directTcpipChannelInfo = channelOpenMessage.Info as DirectTcpipChannelInfo;
            if (directTcpipChannelInfo == null)
                return false;
            if (directTcpipChannelInfo.HostToConnect != _remoteHost)
                return false;
            if (directTcpipChannelInfo.PortToConnect != _port)
                return false;

            var clientEndpoint = _client.LocalEndPoint as IPEndPoint;
            if (clientEndpoint == null)
                return false;
            if (directTcpipChannelInfo.OriginatorAddress != clientEndpoint.Address.ToString())
                return false;
            if (directTcpipChannelInfo.OriginatorPort != clientEndpoint.Port)
                return false;

            return true;
        }
    }
}
