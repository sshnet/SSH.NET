using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelForwardedTcpipTest_Dispose_SessionIsConnectedAndChannelIsOpen
    {
        private Mock<ISession> _sessionMock;
        private Mock<IForwardedPort> _forwardedPortMock;
        private Mock<ISshConnectionInfo> _connectionInfoMock;
        private ChannelForwardedTcpip _channel;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private uint _remoteChannelNumber;
        private TimeSpan _channelCloseTimeout;
        private IPEndPoint _remoteEndpoint;
        private AsyncSocketListener _remoteListener;
        private Exception _channelException;
        private IList<Socket> _connectedRegister;
        private IList<Socket> _disconnectedRegister;
        private Thread _channelThread;
        private TimeSpan _connectionInfoTimeout;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (_remoteListener != null)
            {
                _remoteListener.Stop();
                _remoteListener = null;
            }

            if (_channelThread != null)
            {
                _channelThread.Join();
                _channelThread = null;
            }

            if (_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }
        }

        private void Arrange()
        {
            var random = new Random();
            _localChannelNumber = (uint)random.Next(0, int.MaxValue);
            _localWindowSize = (uint)random.Next(2000, 3000);
            _localPacketSize = (uint)random.Next(1000, 2000);
            _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint)random.Next(0, int.MaxValue);
            _remotePacketSize = (uint)random.Next(100, 200);
            _channelCloseTimeout = TimeSpan.FromSeconds(random.Next(10, 20));
            _channelException = null;
            _connectedRegister = new List<Socket>();
            _disconnectedRegister = new List<Socket>();
            _connectionInfoTimeout = TimeSpan.FromSeconds(5);

            _remoteEndpoint = new IPEndPoint(IPAddress.Loopback, 0);

            _remoteListener = new AsyncSocketListener(_remoteEndpoint);
            _remoteListener.Connected += _connectedRegister.Add;
            _remoteListener.Disconnected += _disconnectedRegister.Add;
            _remoteListener.Start();

            _remoteEndpoint.Port = ((IPEndPoint)_remoteListener.ListenerEndPoint).Port;

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<ISshConnectionInfo>(MockBehavior.Strict);
            _forwardedPortMock = new Mock<IForwardedPort>(MockBehavior.Strict);

            var sequence = new MockSequence();

            _ = _sessionMock.InSequence(sequence)
                            .Setup(p => p.IsConnected)
                            .Returns(true);
            _ = _sessionMock.InSequence(sequence)
                            .Setup(p => p.ConnectionInfo)
                            .Returns(_connectionInfoMock.Object);
            _ = _connectionInfoMock.InSequence(sequence)
                                   .Setup(p => p.Timeout)
                                   .Returns(_connectionInfoTimeout);
            _ = _sessionMock.InSequence(sequence)
                            .Setup(p => p.SendMessage(It.Is<ChannelOpenConfirmationMessage>(m =>
                                    m.LocalChannelNumber == _remoteChannelNumber &&
                                    m.InitialWindowSize == _localWindowSize &&
                                    m.MaximumPacketSize == _localPacketSize &&
                                    m.RemoteChannelNumber == _localChannelNumber)));
            _ = _sessionMock.InSequence(sequence)
                            .Setup(p => p.IsConnected)
                            .Returns(true);
            _ = _sessionMock.InSequence(sequence)
                            .Setup(p => p.TrySendMessage(It.Is<ChannelEofMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                            .Returns(true);
            _ = _sessionMock.InSequence(sequence)
                            .Setup(p => p.IsConnected)
                            .Returns(true);
            _ = _sessionMock.InSequence(sequence)
                            .Setup(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                            .Returns(true);
            _ = _sessionMock.InSequence(sequence)
                            .Setup(p => p.ConnectionInfo)
                            .Returns(_connectionInfoMock.Object);
            _ = _connectionInfoMock.InSequence(sequence)
                                   .Setup(p => p.ChannelCloseTimeout)
                                   .Returns(_channelCloseTimeout);
            _ = _sessionMock.InSequence(sequence)
                            .Setup(p => p.TryWait(It.IsAny<EventWaitHandle>(), _channelCloseTimeout))
                            .Callback<WaitHandle, TimeSpan>((waitHandle, channelCloseTimeout) =>
                                {
                                    _sessionMock.Raise(
                                        s => s.ChannelCloseReceived += null,
                                        new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                                    _ = waitHandle.WaitOne();
                                })
                            .Returns(WaitResult.Success);

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
                    _channel.Bind(_remoteEndpoint, _forwardedPortMock.Object);
                }
                catch (Exception ex)
                {
                    _channelException = ex;
                }
            });
            _channelThread.Start();

            // give channel time to bind to remote endpoint
            Thread.Sleep(300);
        }

        private void Act()
        {
            _channel.Dispose();

            Assert.IsTrue(_channelThread.Join(TimeSpan.FromSeconds(1)));
        }

        [TestMethod]
        public void ChannelShouldShutdownSocketToRemoteListener()
        {
            Assert.AreEqual(1, _connectedRegister.Count);
            Assert.AreEqual(1, _disconnectedRegister.Count);
            Assert.AreSame(_connectedRegister[0], _disconnectedRegister[0]);
        }

        [TestMethod]
        public void BindShouldHaveFinishedWithoutException()
        {
            Assert.IsNull(_channelException, _channelException?.ToString());
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
    }
}
