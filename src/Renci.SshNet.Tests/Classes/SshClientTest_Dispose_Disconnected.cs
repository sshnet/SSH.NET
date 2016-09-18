using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshClientTest_Dispose_Disconnected
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
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

            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _serviceFactoryMock.InSequence(sequence).Setup(p => p.CreateSession(_connectionInfo)).Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.Connect());
            _sessionMock.InSequence(sequence).Setup(p => p.OnDisconnecting());
            _sessionMock.InSequence(sequence).Setup(p => p.Dispose());

            _sshClient = new SshClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _sshClient.Connect();
            _sshClient.Disconnect();
        }

        protected void Act()
        {
            _sshClient.Dispose();
        }

        [TestMethod]
        public void CreateSessionOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(p => p.CreateSession(_connectionInfo), Times.Once);
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

        [TestMethod]
        public void OnDisconnectingOnSessionShouldBeInvokedOnce()
        {
            _sessionMock.Verify(p => p.OnDisconnecting(), Times.Once);
        }
    }
}
