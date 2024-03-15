using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    internal class NetConfClientTest_Dispose_Disposed : NetConfClientTestBase
    {
        private NetConfClient _netConfClient;
        private ConnectionInfo _connectionInfo;
        private int _operationTimeout;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _operationTimeout = new Random().Next(1000, 10000);
            _netConfClient = new NetConfClient(_connectionInfo, false, ServiceFactoryMock.Object)
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
                                   .Setup(p => p.CreateNetConfSession(SessionMock.Object, _operationTimeout))
                                   .Returns(NetConfSessionMock.Object);
            _ = NetConfSessionMock.InSequence(sequence)
                                   .Setup(p => p.Connect());
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.OnDisconnecting());
            _ = NetConfSessionMock.InSequence(sequence)
                                   .Setup(p => p.Disconnect());
            _ = SessionMock.InSequence(sequence)
                            .Setup(p => p.Dispose());
            _ = NetConfSessionMock.InSequence(sequence)
                                   .Setup(p => p.Dispose());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _netConfClient.Connect();
            _netConfClient.Dispose();
        }

        protected override void Act()
        {
            _netConfClient.Dispose();
        }

        [TestMethod]
        public void CreateNetConfSessionOnServiceFactoryShouldBeInvokedOnce()
        {
            ServiceFactoryMock.Verify(p => p.CreateNetConfSession(SessionMock.Object, _operationTimeout), Times.Once);
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
        public void DisconnectOnNetConfSessionShouldBeInvokedOnce()
        {
            NetConfSessionMock.Verify(p => p.Disconnect(), Times.Once);
        }

        [TestMethod]
        public void DisconnectOnSessionShouldNeverBeInvoked()
        {
            SessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnNetConfSessionShouldBeInvokedOnce()
        {
            NetConfSessionMock.Verify(p => p.Dispose(), Times.Once);
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
