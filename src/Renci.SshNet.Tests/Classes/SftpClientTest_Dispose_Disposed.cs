using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SftpClientTest_Dispose_Disposed
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private Mock<ISftpResponseFactory> _sftpResponseFactoryMock;
        private Mock<ISftpSession> _sftpSessionMock;
        private SftpClient _sftpClient;
        private ConnectionInfo _connectionInfo;
        private int _operationTimeout;

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
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _sftpResponseFactoryMock = new Mock<ISftpResponseFactory>(MockBehavior.Strict);
            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);

            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _operationTimeout = new Random().Next(1000, 10000);
            _sftpClient = new SftpClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _sftpClient.OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout);

            var sequence = new MockSequence();
            _serviceFactoryMock.InSequence(sequence)
                .Setup(p => p.CreateSession(_connectionInfo))
                .Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSftpResponseFactory())
                               .Returns(_sftpResponseFactoryMock.Object);
            _serviceFactoryMock.InSequence(sequence)
                .Setup(p => p.CreateSftpSession(_sessionMock.Object, _operationTimeout, _connectionInfo.Encoding, _sftpResponseFactoryMock.Object))
                .Returns(_sftpSessionMock.Object);
            _sftpSessionMock.InSequence(sequence).Setup(p => p.Connect());
            _sessionMock.InSequence(sequence).Setup(p => p.OnDisconnecting());
            _sftpSessionMock.InSequence(sequence).Setup(p => p.Dispose());
            _sessionMock.InSequence(sequence).Setup(p => p.Dispose());

            _sftpClient.Connect();
            _sftpClient.Dispose();
        }

        protected void Act()
        {
            _sftpClient.Dispose();
        }

        [TestMethod]
        public void CreateSftpMessageFactoryOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(p => p.CreateSftpResponseFactory(), Times.Once);
        }

        [TestMethod]
        public void CreateSftpSessionOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(
                p => p.CreateSftpSession(_sessionMock.Object, _operationTimeout, _connectionInfo.Encoding, _sftpResponseFactoryMock.Object),
                Times.Once);
        }

        [TestMethod]
        public void CreateSessionOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(p => p.CreateSession(_connectionInfo), Times.Once);
        }

        [TestMethod]
        public void DisconnectOnNetConfSessionShouldNeverBeInvoked()
        {
            _sftpSessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisconnectOnSessionShouldNeverBeInvoked()
        {
            _sessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnNetConfSessionShouldBeInvokedOnce()
        {
            _sftpSessionMock.Verify(p => p.Dispose(), Times.Once);
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
