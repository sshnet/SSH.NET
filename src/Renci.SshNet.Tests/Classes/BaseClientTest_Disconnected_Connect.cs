using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Connection;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class BaseClientTest_Disconnected_Connect : BaseClientTestBase
    {
        private Mock<ISocketFactory> _socketFactory2Mock;
        private Mock<ISession> _session2Mock;
        private BaseClient _client;
        private ConnectionInfo _connectionInfo;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"));
        }

        protected override void CreateMocks()
        {
            base.CreateMocks();

            _socketFactory2Mock = new Mock<ISocketFactory>(MockBehavior.Strict);
            _session2Mock = new Mock<ISession>(MockBehavior.Strict);
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
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.OnDisconnecting());
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.Dispose());
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSocketFactory())
                               .Returns(_socketFactory2Mock.Object);
            _serviceFactoryMock.InSequence(sequence)
                               .Setup(p => p.CreateSession(_connectionInfo, _socketFactory2Mock.Object))
                               .Returns(_session2Mock.Object);
            _session2Mock.InSequence(sequence)
                        .Setup(p => p.Connect());
        }

        protected override void Arrange()
        {
            base.Arrange();

            _client = new MyClient(_connectionInfo, false, _serviceFactoryMock.Object);
            _client.Connect();
            _client.Disconnect();
        }

        protected override void TearDown()
        {
            if (_client != null)
            {
                _session2Mock.Setup(p => p.OnDisconnecting());
                _session2Mock.Setup(p => p.Dispose());
                _client.Dispose();
            }
        }

        protected override void Act()
        {
            _client.Connect();
        }

        [TestMethod]
        public void CreateSocketFactoryOnServiceFactoryShouldBeInvokedTwic()
        {
            _serviceFactoryMock.Verify(p => p.CreateSocketFactory(), Times.Exactly(2));
        }

        [TestMethod]
        public void CreateSessionOnServiceFactoryShouldBeInvokedTwice()
        {
            _serviceFactoryMock.Verify(p => p.CreateSession(_connectionInfo, _socketFactoryMock.Object),
                                       Times.Once);
            _serviceFactoryMock.Verify(p => p.CreateSession(_connectionInfo, _socketFactory2Mock.Object),
                                       Times.Once);
        }

        [TestMethod]
        public void ConnectOnSessionShouldBeInvokedTwice()
        {
            _sessionMock.Verify(p => p.Connect(), Times.Once);
            _session2Mock.Verify(p => p.Connect(), Times.Once);
        }

        private class MyClient : BaseClient
        {
            public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
            {
            }
        }
    }
}
