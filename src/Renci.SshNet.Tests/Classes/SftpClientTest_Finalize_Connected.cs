using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

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
            _sftpClient = new SftpClient(_connectionInfo, false, ServiceFactoryMock.Object)
                {
                    OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout)
                };
            _sftpClientWeakRefence = new WeakReference(_sftpClient);
        }

        protected override void SetupMocks()
        {
            _ = ServiceFactoryMock.Setup(p => p.CreateSocketFactory())
                                   .Returns(SocketFactoryMock.Object);
            _ = ServiceFactoryMock.Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                                   .Returns(SessionMock.Object);
            _ = SessionMock.Setup(p => p.Connect());
            _ = ServiceFactoryMock.Setup(p => p.CreateSftpResponseFactory())
                                   .Returns(SftpResponseFactoryMock.Object);
            _ = ServiceFactoryMock.Setup(p => p.CreateSftpSession(SessionMock.Object, _operationTimeout, _connectionInfo.Encoding, SftpResponseFactoryMock.Object))
                                   .Returns(SftpSessionMock.Object);
            _ = SftpSessionMock.Setup(p => p.Connect());
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
            SftpSessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnSftpSessionShouldNeverBeInvoked()
        {
            // Since we recreated the mocks, this test has no value
            // We'll leaving ths test just in case we have a solution that does not require us
            // to recreate the mocks
            SftpSessionMock.Verify(p => p.Dispose(), Times.Never);
        }

        [TestMethod]
        public void SftpClientShouldHaveBeenFinalized()
        {
            Assert.IsNull(_sftpClientWeakRefence.Target);
        }
    }
}
