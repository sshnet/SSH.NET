using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortRemoteTest_Start_PortNeverStarted
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private ForwardedPortRemote _forwardedPort;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private IPEndPoint _bindEndpoint;
        private IPEndPoint _remoteEndpoint;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_forwardedPort != null)
            {
                _sessionMock.Setup(
                    p =>
                        p.SendMessage(
                            It.Is<CancelTcpIpForwardGlobalRequestMessage>(
                                g =>
                                    g.AddressToBind == _forwardedPort.BoundHost &&
                                    g.PortToBind == _forwardedPort.BoundPort)));
                _sessionMock.Setup(p => p.MessageListenerCompleted).Returns(new ManualResetEvent(true));
                _forwardedPort.Dispose();
                _forwardedPort = null;
            }
        }

        protected void Arrange()
        {
            var random = new Random();
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _bindEndpoint = new IPEndPoint(IPAddress.Any, random.Next(IPEndPoint.MinPort, 1000));
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
            _forwardedPort = new ForwardedPortRemote(_bindEndpoint.Address, (uint)_bindEndpoint.Port, _remoteEndpoint.Address, (uint)_remoteEndpoint.Port);

            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(15));
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _sessionMock.Setup(p => p.RegisterMessage("SSH_MSG_REQUEST_FAILURE"));
            _sessionMock.Setup(p => p.RegisterMessage("SSH_MSG_REQUEST_SUCCESS"));
            _sessionMock.Setup(p => p.RegisterMessage("SSH_MSG_CHANNEL_OPEN"));
            _sessionMock.Setup(
                p =>
                    p.SendMessage(
                        It.Is<TcpIpForwardGlobalRequestMessage>(
                            g =>
                                g.AddressToBind == _forwardedPort.BoundHost &&
                                g.PortToBind == _forwardedPort.BoundPort)))
                .Callback(
                    () =>
                        _sessionMock.Raise(s => s.RequestSuccessReceived += null,
                            new MessageEventArgs<RequestSuccessMessage>(new RequestSuccessMessage())));
            _sessionMock.Setup(p => p.WaitOnHandle(It.IsAny<WaitHandle>()))
                .Callback<WaitHandle>(handle => handle.WaitOne());

            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;
        }

        protected void Act()
        {
            _forwardedPort.Start();
        }

        [TestMethod]
        public void IsStartedShouldReturnTrue()
        {
            Assert.IsTrue(_forwardedPort.IsStarted);
        }

        [TestMethod]
        public void ForwardedPortShouldAcceptNewConnections()
        {
            var channelNumber = (uint)new Random().Next(1001, int.MaxValue);
            var initialWindowSize = (uint)new Random().Next(0, int.MaxValue);
            var maximumPacketSize = (uint)new Random().Next(0, int.MaxValue);
            var originatorAddress = new Random().Next().ToString(CultureInfo.InvariantCulture);
            var originatorPort = (uint)new Random().Next(0, int.MaxValue);
            var channelMock = new Mock<IChannelForwardedTcpip>(MockBehavior.Strict);

            _sessionMock.Setup(
                p =>
                    p.CreateChannelForwardedTcpip(channelNumber, initialWindowSize, maximumPacketSize)).Returns(channelMock.Object);
            channelMock.Setup(
                p =>
                    p.Bind(
                        It.Is<IPEndPoint>(
                            ep => ep.Address.Equals(_remoteEndpoint.Address) && ep.Port == _remoteEndpoint.Port),
                        _forwardedPort));
            channelMock.Setup(p => p.Dispose());

            _sessionMock.Raise(p => p.ChannelOpenReceived += null,
                new MessageEventArgs<ChannelOpenMessage>(new ChannelOpenMessage(channelNumber, initialWindowSize,
                    maximumPacketSize,
                    new ForwardedTcpipChannelInfo(_forwardedPort.BoundHost, _forwardedPort.BoundPort, originatorAddress,
                        originatorPort))));

            _sessionMock.Verify(p => p.CreateChannelForwardedTcpip(channelNumber, initialWindowSize, maximumPacketSize), Times.Once);
            channelMock.Verify(p => p.Bind(It.Is<IPEndPoint>(ep => ep.Address.Equals(_remoteEndpoint.Address) && ep.Port == _remoteEndpoint.Port), _forwardedPort), Times.Once);
            channelMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void ClosingShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _closingRegister.Count);
        }

        [TestMethod]
        public void ExceptionShouldNotHaveFired()
        {
            Assert.AreEqual(0, _exceptionRegister.Count);
        }
    }
}
