using System;
using System.Reflection;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Security;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class BaseClientTest_Connect_OnConnectedThrowsException
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISession> _sessionMock;
        private MyClient _client;
        private ConnectionInfo _connectionInfo;
        private ApplicationException _onConnectException;
        private ApplicationException _actualException;

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
            _onConnectException = new ApplicationException();
        }

        private void CreateMocks()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            _serviceFactoryMock.Setup(p => p.CreateSession(_connectionInfo))
                               .Returns(_sessionMock.Object);
            _sessionMock.Setup(p => p.Connect());
            _sessionMock.Setup(p => p.Dispose());
        }

        protected void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _client = new MyClient(_connectionInfo, false, _serviceFactoryMock.Object)
                {
                    OnConnectedException = _onConnectException
                };
        }

        protected void Act()
        {
            try
            {
                _client.Connect();
                Assert.Fail();
            }
            catch (ApplicationException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void ConnectShouldRethrowExceptionThrownByOnConnect()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreSame(_onConnectException, _actualException);
        }

        [TestMethod]
        public void CreateSessionOnServiceFactoryShouldBeInvokedOnce()
        {
            _serviceFactoryMock.Verify(p => p.CreateSession(_connectionInfo), Times.Once);
        }

        [TestMethod]
        public void ConnectOnSessionShouldBeInvokedOnce()
        {
            _sessionMock.Verify(p => p.Connect(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnSessionShouldBeInvokedOnce()
        {
            _sessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void ErrorOccuredOnSessionShouldNoLongerBeSignaledViaErrorOccurredOnBaseClient()
        {
            var errorOccurredSignalCount = 0;

            _client.ErrorOccurred += (sender, args) => Interlocked.Increment(ref errorOccurredSignalCount);

            _sessionMock.Raise(p => p.ErrorOccured += null, new ExceptionEventArgs(new Exception()));

            Assert.AreEqual(0, errorOccurredSignalCount);
        }

        [TestMethod]
        public void HostKeyReceivedOnSessionShouldNoLongerBeSignaledViaHostKeyReceivedOnBaseClient()
        {
            var hostKeyReceivedSignalCount = 0;

            _client.HostKeyReceived += (sender, args) => Interlocked.Increment(ref hostKeyReceivedSignalCount);

            _sessionMock.Raise(p => p.HostKeyReceived += null, new HostKeyEventArgs(GetKeyHostAlgorithm()));

            Assert.AreEqual(0, hostKeyReceivedSignalCount);
        }

        [TestMethod]
        public void SessionShouldBeNull()
        {
            Assert.IsNull(_client.Session);
        }

        [TestMethod]
        public void IsConnectedShouldReturnFalse()
        {
            Assert.IsFalse(_client.IsConnected);
        }

        private static KeyHostAlgorithm GetKeyHostAlgorithm()
        {
            var executingAssembly = Assembly.GetExecutingAssembly();

            using (var s = executingAssembly.GetManifestResourceStream(string.Format("Renci.SshNet.Tests.Data.{0}", "Key.RSA.txt")))
            {
                var privateKey = new PrivateKeyFile(s);
                return (KeyHostAlgorithm) privateKey.HostKey;
            }
        }

        private class MyClient : BaseClient
        {
            private int _onConnectedCount;

            public MyClient(ConnectionInfo connectionInfo, bool ownsConnectionInfo, IServiceFactory serviceFactory) : base(connectionInfo, ownsConnectionInfo, serviceFactory)
            {
            }

            public Exception OnConnectedException { get; set; }

            protected override void OnConnected()
            {
                base.OnConnected();

                Interlocked.Increment(ref _onConnectedCount);

                if (OnConnectedException != null)
                {
                    throw OnConnectedException;
                }
            }
        }


    }
}
