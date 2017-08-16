using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SftpClientTest_Finalize_Connected
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

        protected void Arrange()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Loose);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _sftpResponseFactoryMock = new Mock<ISftpResponseFactory>(MockBehavior.Strict);
            _sftpSessionMock = new Mock<ISftpSession>(MockBehavior.Strict);

            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _operationTimeout = new Random().Next(1000, 10000);
            _sftpClient = new SftpClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _sftpClient.OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout);

            _serviceFactoryMock.Setup(p => p.CreateSession(_connectionInfo))
                .Returns(_sessionMock.Object);
            _sessionMock.Setup(p => p.Connect());
            _serviceFactoryMock.Setup(p => p.CreateSftpResponseFactory())
                               .Returns(_sftpResponseFactoryMock.Object);
            _serviceFactoryMock.Setup(p => p.CreateSftpSession(_sessionMock.Object, _operationTimeout, _connectionInfo.Encoding, _sftpResponseFactoryMock.Object))
                               .Returns(_sftpSessionMock.Object);
            _sftpSessionMock.Setup(p => p.Connect());

            _sftpClient.Connect();
            _sftpClient = null;

            // we need to dereference all other mocks as they might otherwise hold the target alive
            _sessionMock = null;
            _connectionInfo = null;
            _serviceFactoryMock = null;
        }

        protected void Act()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void DisconnectOnSftpSessionShouldNeverBeInvoked()
        {
            _sftpSessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnSftpSessionShouldNeverBeInvoked()
        {
            _sftpSessionMock.Verify(p => p.Dispose(), Times.Never);
        }
    }
}
