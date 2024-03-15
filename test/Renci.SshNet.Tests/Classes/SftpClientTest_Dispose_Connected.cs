using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    internal class SftpClientTest_Dispose_Connected : SftpClientTestBase
    {
        private SftpClient _sftpClient;
        private ConnectionInfo _connectionInfo;
        private int _operationTimeout;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _operationTimeout = new Random().Next(1000, 10000);
            _sftpClient = new SftpClient(_connectionInfo, false, ServiceFactoryMock.Object)
                {
                    OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout)
                };
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSocketFactory())
                                   .Returns(SocketFactoryMock.Object);
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                                   .Returns(SessionMock.Object);
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.Connect());
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSftpResponseFactory())
                                   .Returns(SftpResponseFactoryMock.Object);
            _ = ServiceFactoryMock.InSequence(sequence)
                                   .Setup(p => p.CreateSftpSession(SessionMock.Object, _operationTimeout, _connectionInfo.Encoding, SftpResponseFactoryMock.Object))
                                   .Returns(SftpSessionMock.Object);
            _ = SftpSessionMock.InSequence(sequence)
                                .Setup(p => p.Connect());
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.OnDisconnecting());
            _ = SftpSessionMock.InSequence(sequence)
                                .Setup(p => p.Dispose());
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.Dispose());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _sftpClient.Connect();
        }

        protected override void Act()
        {
            _sftpClient.Dispose();
        }

        [TestMethod]
        public void CreateSftpMessageFactoryOnServiceFactoryShouldBeInvokedOnce()
        {
            ServiceFactoryMock.Verify(p => p.CreateSftpResponseFactory(), Times.Once);
        }

        [TestMethod]
        public void CreateSftpSessionOnServiceFactoryShouldBeInvokedOnce()
        {
            ServiceFactoryMock.Verify(
                p => p.CreateSftpSession(SessionMock.Object, _operationTimeout, _connectionInfo.Encoding, SftpResponseFactoryMock.Object),
                Times.Once);
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
        public void DisconnectOnNetConfSessionShouldNeverBeInvoked()
        {
            SftpSessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisconnectOnSessionShouldNeverBeInvoked()
        {
            SessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnNetConfSessionShouldBeInvokedOnce()
        {
            SftpSessionMock.Verify(p => p.Dispose(), Times.Once);
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
