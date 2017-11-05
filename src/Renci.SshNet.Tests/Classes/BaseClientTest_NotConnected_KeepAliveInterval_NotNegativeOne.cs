using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class BaseClientTest_NotConnected_KeepAliveInterval_NotNegativeOne
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private BaseClient _client;
        private ConnectionInfo _connectionInfo;
        private TimeSpan _keepAliveInterval;
        private int _keepAliveCount;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_client != null)
            {
                _sessionMock.Setup(p => p.OnDisconnecting());
                _sessionMock.Setup(p => p.Dispose());
                _client.Dispose();
            }
        }

        private void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"));
            _keepAliveInterval = TimeSpan.FromMilliseconds(100d);
            _keepAliveCount = 0;
        }

        private void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        }

        private static void SetupMocks()
        {
        }

        protected void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _client = new MyClient(_connectionInfo, false, _serviceFactoryMock.Object);
        }

        protected void Act()
        {
            _client.KeepAliveInterval = _keepAliveInterval;

            // allow keep-alive to be sent at least once
            Thread.Sleep(150);
        }

        [TestMethod]
        public void KeepAliveIntervalShouldReturnConfiguredValue()
        {
            Assert.AreEqual(_keepAliveInterval, _client.KeepAliveInterval);
        }

        [TestMethod]
        public void ConnectShouldActivateKeepAliveIfSessionIs()
        {
            _serviceFactoryMock.Setup(p => p.CreateSession(_connectionInfo)).Returns(_sessionMock.Object);
            _sessionMock.Setup(p => p.Connect());
            _sessionMock.Setup(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()))
                        .Returns(true)
                        .Callback(() => Interlocked.Increment(ref _keepAliveCount));

            _client.Connect();

            // allow keep-alive to be sent twice
            Thread.Sleep(250);

            // Exactly two keep-alives should be sent
            _sessionMock.Verify(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()), Times.Exactly(2));
        }

        private class MyClient : BaseClient
        {
            public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
            {
            }
        }
    }
}
