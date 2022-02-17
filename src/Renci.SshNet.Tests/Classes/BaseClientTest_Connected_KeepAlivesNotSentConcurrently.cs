using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class BaseClientTest_Connected_KeepAlivesNotSentConcurrently : BaseClientTestBase
    {
        private MockSequence _mockSequence;
        private BaseClient _client;
        private ConnectionInfo _connectionInfo;
        private ManualResetEvent _keepAliveSent;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"));
            _keepAliveSent = new ManualResetEvent(false);
        }

        protected override void SetupMocks()
        {
            _mockSequence = new MockSequence();

            _serviceFactoryMock.InSequence(_mockSequence)
                               .Setup(p => p.CreateSocketFactory())
                               .Returns(_socketFactoryMock.Object);
            _serviceFactoryMock.InSequence(_mockSequence)
                               .Setup(p => p.CreateSession(_connectionInfo, _socketFactoryMock.Object))
                               .Returns(_sessionMock.Object);
            _sessionMock.InSequence(_mockSequence)
                        .Setup(p => p.Connect());
            _sessionMock.InSequence(_mockSequence)
                        .Setup(p => p.TrySendMessage(It.IsAny<IgnoreMessage>()))
                        .Returns(true)
                        .Callback(() =>
                            {
                                Thread.Sleep(300);
                                _keepAliveSent.Set();
                            });
        }

        protected override void Arrange()
        {
            base.Arrange();

            _client = new MyClient(_connectionInfo, false, _serviceFactoryMock.Object)
                {
                    KeepAliveInterval = TimeSpan.FromMilliseconds(50d)
                };
            _client.Connect();
        }

        protected override void TearDown()
        {
            if (_client != null)
            {
                _sessionMock.InSequence(_mockSequence).Setup(p => p.OnDisconnecting());
                _sessionMock.InSequence(_mockSequence).Setup(p => p.Dispose());
                _client.Dispose();
            }
        }

        protected override void Act()
        {
            // should keep-alive message be sent concurrently, then multiple keep-alive
            // message would be sent during this sleep period
            Thread.Sleep(200);

            // disable further keep-alives
            _client.KeepAliveInterval = Session.InfiniteTimeSpan;

            // wait until keep-alive has been sent at least once
            Assert.IsTrue(_keepAliveSent.WaitOne(500));
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
