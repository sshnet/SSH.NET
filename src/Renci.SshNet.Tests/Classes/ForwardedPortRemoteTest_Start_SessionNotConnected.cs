using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortRemoteTest_Start_SessionNotConnected
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private ForwardedPortRemote _forwardedPort;
        private IPEndPoint _bindEndpoint;
        private IPEndPoint _remoteEndpoint;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private SshConnectionException _actualException;

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
                _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
                _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(1));
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

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

            _sessionMock.Setup(p => p.IsConnected).Returns(false);

            _forwardedPort = new ForwardedPortRemote(_bindEndpoint.Address, (uint)_bindEndpoint.Port, _remoteEndpoint.Address, (uint)_remoteEndpoint.Port); 
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;
        }

        protected void Act()
        {
            try
            {
                _forwardedPort.Start();
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void StartShouldThrowSshConnectionException()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreEqual("Client not connected.", _actualException.Message);
        }

        [TestMethod]
        public void IsStartedShouldReturnFalse()
        {
            Assert.IsFalse(_forwardedPort.IsStarted);
        }

        [TestMethod]
        public void ForwardedPortShouldIgnoreReceivedSignalForNewConnection()
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

            _sessionMock.Raise(p => p.ChannelOpenReceived += null,
                new MessageEventArgs<ChannelOpenMessage>(new ChannelOpenMessage(channelNumber, initialWindowSize,
                    maximumPacketSize,
                    new ForwardedTcpipChannelInfo(_forwardedPort.BoundHost, _forwardedPort.BoundPort, originatorAddress,
                        originatorPort))));

            _sessionMock.Verify(p => p.CreateChannelForwardedTcpip(channelNumber, initialWindowSize, maximumPacketSize), Times.Never);
            _sessionMock.Verify(p => p.SendMessage(new ChannelOpenFailureMessage(channelNumber, string.Empty, ChannelOpenFailureMessage.AdministrativelyProhibited)), Times.Never);
        }

        [TestMethod]
        public void ClosingShouldNotHaveFired()
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

