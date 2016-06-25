using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.NetConf;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class NetConfClientTest_Finalize_Connected
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private NetConfClient _netConfClient;
        private ConnectionInfo _connectionInfo;
        private Mock<INetConfSession> _netConfSessionMock;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        protected void Arrange()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Loose);
            _sessionMock = new Mock<ISession>(MockBehavior.Loose);
            _netConfSessionMock = new Mock<INetConfSession>(MockBehavior.Loose);

            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _netConfClient = new NetConfClient(_connectionInfo, false, _serviceFactoryMock.Object);

            var sequence = new MockSequence();
            _serviceFactoryMock.InSequence(sequence)
                .Setup(p => p.CreateSession(_connectionInfo))
                .Returns(_sessionMock.Object);
            _sessionMock.InSequence(sequence).Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence)
                .Setup(p => p.CreateNetConfSession(_sessionMock.Object, _netConfClient.OperationTimeout))
                .Returns(_netConfSessionMock.Object);
            _netConfSessionMock.InSequence(sequence).Setup(p => p.Connect());

            _netConfClient.Connect();
            _netConfClient = null;

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
        public void DisconnectOnNetConfSessionShouldBeInvokedOnce()
        {
            _netConfSessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnNetConfSessionShouldBeInvokedOnce()
        {
            _netConfSessionMock.Verify(p => p.Dispose(), Times.Never);
        }
    }
}
