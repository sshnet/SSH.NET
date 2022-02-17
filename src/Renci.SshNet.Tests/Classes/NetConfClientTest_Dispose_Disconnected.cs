using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.NetConf;
using System;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class NetConfClientTest_Dispose_Disconnected : NetConfClientTestBase
    {
        private NetConfClient _netConfClient;
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

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _operationTimeout = new Random().Next(1000, 10000);
            _netConfClient = new NetConfClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _netConfClient.OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout);
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSocketFactory())
                               .Returns(_socketFactoryMock.Object);
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSession(_connectionInfo, _socketFactoryMock.Object))
                               .Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateNetConfSession(_sessionMock.Object, _operationTimeout))
                               .Returns(_netConfSessionMock.Object);
            _netConfSessionMock.InSequence(sequence).Setup(p => p.Connect());
            _sessionMock.InSequence(sequence).Setup(p => p.OnDisconnecting());
            _netConfSessionMock.InSequence(sequence).Setup(p => p.Disconnect());
            _sessionMock.InSequence(sequence).Setup(p => p.Dispose());
            _netConfSessionMock.InSequence(sequence).Setup(p => p.Disconnect());
            _netConfSessionMock.InSequence(sequence).Setup(p => p.Dispose());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _netConfClient.Connect();
            _netConfClient.Disconnect();
        }

        protected override void Act()
        {
            _netConfClient.Dispose();
        }

        [TestMethod]
        public void CreateNetConfSessionOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(p => p.CreateNetConfSession(_sessionMock.Object, _operationTimeout), Times.Once);
        }

        [TestMethod]
        public void CreateSocketFactoryOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(p => p.CreateSocketFactory(), Times.Once);
        }

        [TestMethod]
        public void CreateSessionOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(p => p.CreateSession(_connectionInfo, _socketFactoryMock.Object),
                                       Times.Once);
        }

        [TestMethod]
        public void DisconnectOnNetConfSessionShouldBeInvokedTwice()
        {
            _netConfSessionMock.Verify(p => p.Disconnect(), Times.Exactly(2));
        }

        [TestMethod]
        public void DisconnectOnSessionShouldNeverBeInvoked()
        {
            _sessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnNetConfSessionShouldBeInvokedOnce()
        {
            _netConfSessionMock.Verify(p => p.Dispose(), Times.Once);
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
