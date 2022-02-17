using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class NetConfClientTest_Finalize_Connected : NetConfClientTestBase
    {
        private NetConfClient _netConfClient;
        private ConnectionInfo _connectionInfo;
        private int _operationTimeout;
        private WeakReference _netConfClientWeakRefence;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _operationTimeout = new Random().Next(1000, 10000);
            _netConfClient = new NetConfClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _netConfClient.OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout);
            _netConfClientWeakRefence = new WeakReference(_netConfClient);
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
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.Connect());
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateNetConfSession(_sessionMock.Object, _operationTimeout))
                               .Returns(_netConfSessionMock.Object);
            _netConfSessionMock.InSequence(sequence)
                               .Setup(p => p.Connect());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _netConfClient.Connect();
            _netConfClient = null;

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
        public void DisconnectOnNetConfSessionShouldBeInvokedOnce()
        {
            // Since we recreated the mocks, this test has no value
            // We'll leaving ths test just in case we have a solution that does not require us
            // to recreate the mocks
            _netConfSessionMock.Verify(p => p.Disconnect(), Times.Never);
        }

        [TestMethod]
        public void DisposeOnNetConfSessionShouldBeInvokedOnce()
        {
            // Since we recreated the mocks, this test has no value
            // We'll leaving ths test just in case we have a solution that does not require us
            // to recreate the mocks
            _netConfSessionMock.Verify(p => p.Dispose(), Times.Never);
        }

        [TestMethod]
        public void NetConfClientShouldHaveBeenFinalized()
        {
            Assert.IsNull(_netConfClientWeakRefence.Target);
        }
    }
}
