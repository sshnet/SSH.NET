using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortDynamicTest_Start_PortStarted
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private Mock<IChannelDirectTcpip> _channelMock;
        private ForwardedPortDynamic _forwardedPort;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private IPEndPoint _endpoint;
        private InvalidOperationException _actualException;

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
                _forwardedPort.Dispose();
                _forwardedPort = null;
            }
        }

        protected void Arrange()
        {
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _endpoint = new IPEndPoint(IPAddress.Loopback, 8122);

            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelDirectTcpip>(MockBehavior.Strict);

            _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(15));
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _sessionMock.Setup(p => p.CreateChannelDirectTcpip()).Returns(_channelMock.Object);

            _forwardedPort = new ForwardedPortDynamic(_endpoint.Address.ToString(), (uint)_endpoint.Port);
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;
            _forwardedPort.Start();
        }

        protected void Act()
        {
            try
            {
                _forwardedPort.Start();
                Assert.Fail();
            }
            catch (InvalidOperationException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void StartShouldThrowInvalidOperatationException()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreEqual("Forwarded port is already started.", _actualException.Message);
        }

        [TestMethod]
        public void IsStartedShouldReturnTrue()
        {
            Assert.IsTrue(_forwardedPort.IsStarted);
        }

        [TestMethod]
        public void ForwardedPortShouldAcceptNewConnections()
        {
            using (var client = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                client.Connect(_endpoint);
            }
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
