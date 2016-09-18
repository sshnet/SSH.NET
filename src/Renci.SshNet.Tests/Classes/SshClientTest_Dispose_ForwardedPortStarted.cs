using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshClientTest_Dispose_ForwardedPortStarted
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private Mock<ForwardedPort> _forwardedPortMock;
        private SshClient _sshClient;
        private ConnectionInfo _connectionInfo;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        protected void Arrange()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));

            var sequence = new MockSequence();

            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _forwardedPortMock = new Mock<ForwardedPort>(MockBehavior.Strict);

            _serviceFactoryMock.InSequence(sequence).Setup(p => p.CreateSession(_connectionInfo)).Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.Connect());
            _forwardedPortMock.InSequence(sequence).Setup(p => p.Start());
            _sessionMock.InSequence(sequence).Setup(p => p.OnDisconnecting());
            _forwardedPortMock.InSequence(sequence).Setup(p => p.Stop());
            _sessionMock.InSequence(sequence).Setup(p => p.Dispose());

            _sshClient = new SshClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _sshClient.Connect();
            _sshClient.AddForwardedPort(_forwardedPortMock.Object);

            _forwardedPortMock.Object.Start();
        }

        protected void Act()
        {
            _sshClient.Dispose();
        }

        [TestMethod]
        public void ForwardedPortShouldBeStopped()
        {
            _forwardedPortMock.Verify(p => p.Stop(), Times.Once);
        }

        [TestMethod]
        public void ForwardedPortShouldBeRemovedFromSshClient()
        {
            Assert.IsFalse(_sshClient.ForwardedPorts.Any());
        }

        [TestMethod]
        public void IsConnectedShouldThrowObjectDisposedException()
        {
            try
            {
                var connected = _sshClient.IsConnected;
                Assert.Fail("IsConnected should have thrown {0} but returned {1}.",
                    typeof (ObjectDisposedException).FullName, connected);
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void DisconnectOnSessionShouldNeverBeInvoked()
        {
            _sessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnSessionShouldBeInvokedOnce()
        {
            _sessionMock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}
