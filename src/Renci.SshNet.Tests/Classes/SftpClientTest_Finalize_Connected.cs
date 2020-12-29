using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Sftp;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SftpClientTest_Finalize_Connected : SftpClientTestBase
    {
        private SftpClient _sftpClient;
        private ConnectionInfo _connectionInfo;
        private int _operationTimeout;
        private WeakReference _sftpClientWeakRefence;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _operationTimeout = new Random().Next(1000, 10000);
            _sftpClient = new SftpClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _sftpClient.OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout);
            _sftpClientWeakRefence = new WeakReference(_sftpClient);
        }

        protected override void SetupMocks()
        {
            _serviceFactoryMock.Setup(p => p.CreateSocketFactory())
                               .Returns(_socketFactoryMock.Object);
            _serviceFactoryMock.Setup(p => p.CreateSession(_connectionInfo, _socketFactoryMock.Object))
                               .Returns(_sessionMock.Object);
            _sessionMock.Setup(p => p.Connect());
            _serviceFactoryMock.Setup(p => p.CreateSftpResponseFactory())
                               .Returns(_sftpResponseFactoryMock.Object);
            _serviceFactoryMock.Setup(p => p.CreateSftpSession(_sessionMock.Object, _operationTimeout, _connectionInfo.Encoding, _sftpResponseFactoryMock.Object))
                               .Returns(_sftpSessionMock.Object);
            _sftpSessionMock.Setup(p => p.Connect());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _sftpClient.Connect();
            _sftpClient = null;

            // We need to dereference all mocks as they might otherwise hold the target alive
            //(through recorded invocations?)
            CreateMocks();
        }

        protected override void Act()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [TestMethod]
        public void DisconnectOnSftpSessionShouldNeverBeInvoked()
        {
            // Since we recreated the mocks, this test has no value
            // We'll leaving ths test just in case we have a solution that does not require us
            // to recreate the mocks
            _sftpSessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnSftpSessionShouldNeverBeInvoked()
        {
            // Since we recreated the mocks, this test has no value
            // We'll leaving ths test just in case we have a solution that does not require us
            // to recreate the mocks
            _sftpSessionMock.Verify(p => p.Dispose(), Times.Never);
        }

        [TestMethod]
        public void SftpClientShouldHaveBeenFinalized()
        {
            Assert.IsNull(_sftpClientWeakRefence.Target);
        }
    }
}
