using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SshClientTest_Dispose_Connected : BaseClientTestBase
    {
        private SshClient _sshClient;
        private ConnectionInfo _connectionInfo;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            ServiceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSocketFactory())
                               .Returns(SocketFactoryMock.Object);
            ServiceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                               .Returns(SessionMock.Object);
            SessionMock.InSequence(sequence).Setup(p => p.Connect());
            SessionMock.InSequence(sequence).Setup(p => p.OnDisconnecting());
            SessionMock.InSequence(sequence).Setup(p => p.Dispose());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _sshClient = new SshClient(_connectionInfo, false, ServiceFactoryMock.Object);
            _sshClient.Connect();
        }

        protected override void Act()
        {
            _sshClient.Dispose();
        }

        [TestMethod]
        public void CreateSocketFactoryOnServiceFactoryShouldBeInvokedOnce()
        {
            ServiceFactoryMock.Verify(p => p.CreateSocketFactory(), Times.Once);
        }

        [TestMethod]
        public void CreateSessionOnServiceFactoryShouldBeInvokedOnce()
        {
            ServiceFactoryMock.Verify(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object),
                                       Times.Once);
        }

        [TestMethod]
        public void DisconnectOnSessionShouldNeverBeInvoked()
        {
            SessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnSessionShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void OnDisconnectingOnSessionShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.OnDisconnecting(), Times.Once);
        }
    }
}
