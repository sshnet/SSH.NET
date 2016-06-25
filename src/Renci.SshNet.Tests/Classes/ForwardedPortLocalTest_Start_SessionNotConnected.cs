using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortLocalTest_Start_SessionNotConnected
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private ForwardedPortLocal _forwardedPort;
        private IPEndPoint _localEndpoint;
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
            _localEndpoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"),
                random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

            _sessionMock.Setup(p => p.IsConnected).Returns(false);

            _forwardedPort = new ForwardedPortLocal(_localEndpoint.Address.ToString(), (uint)_localEndpoint.Port,
                _remoteEndpoint.Address.ToString(), (uint)_remoteEndpoint.Port);
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
        public void ForwardedPortShouldRejectNewConnections()
        {
            using (var client = new Socket(_localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    client.Connect(_localEndpoint);
                }
                catch (SocketException ex)
                {
                    Assert.AreEqual(SocketError.ConnectionRefused, ex.SocketErrorCode);
                }
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
