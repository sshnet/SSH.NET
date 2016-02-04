using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class BaseClientTest_Connected_KeepAlivesNotSentConcurrently
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private BaseClient _client;
        private ConnectionInfo _connectionInfo;

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

        protected void Arrange()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"));

            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _serviceFactoryMock.Setup(p => p.CreateSession(_connectionInfo)).Returns(_sessionMock.Object);
            _sessionMock.Setup(p => p.Connect());
            _sessionMock.Setup(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()))
                .Returns(true)
                .Callback(() => Thread.Sleep(300));

            _client = new MyClient(_connectionInfo, false, _serviceFactoryMock.Object)
                {
                    KeepAliveInterval = TimeSpan.FromMilliseconds(50d)
                };
            _client.Connect();
        }

        protected void Act()
        {
            // should keep-alive message be sent concurrently, then multiple keep-alive
            // message would be sent during this sleep period
            Thread.Sleep(200);
        }

        [TestMethod]
        public void SendMessageOnSessionShouldBeInvokedOnce()
        {
            _sessionMock.Verify(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()), Times.Once);
        }

        private class MyClient : BaseClient
        {
            public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
            {
            }
        }
    }
}
