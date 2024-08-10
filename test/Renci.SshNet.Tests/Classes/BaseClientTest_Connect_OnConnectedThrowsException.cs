﻿using System;
using System.Linq;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class BaseClientTest_Connect_OnConnectedThrowsException : BaseClientTestBase
    {
        private MyClient _client;
        private ConnectionInfo _connectionInfo;
        private ApplicationException _onConnectException;
        private ApplicationException _actualException;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "pwd"));
            _onConnectException = new ApplicationException();
        }

        protected override void SetupMocks()
        {
            ServiceFactoryMock.Setup(p => p.CreateSocketFactory())
                               .Returns(SocketFactoryMock.Object);
            ServiceFactoryMock.Setup(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object))
                               .Returns(SessionMock.Object);
            SessionMock.Setup(p => p.Connect());
            SessionMock.Setup(p => p.Dispose());
        }

        protected override void TearDown()
        {
            if (_client != null)
            {
                SessionMock.Setup(p => p.OnDisconnecting());
                SessionMock.Setup(p => p.Dispose());
                _client.Dispose();
            }
        }

        protected override void Arrange()
        {
            base.Arrange();

            _client = new MyClient(_connectionInfo, false, ServiceFactoryMock.Object)
            {
                OnConnectedException = _onConnectException
            };
        }

        protected override void Act()
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
        public void ConnectOnSessionShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.Connect(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnSessionShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void ErrorOccuredOnSessionShouldNoLongerBeSignaledViaErrorOccurredOnBaseClient()
        {
            var errorOccurredSignalCount = 0;

            _client.ErrorOccurred += (sender, args) => Interlocked.Increment(ref errorOccurredSignalCount);

            SessionMock.Raise(p => p.ErrorOccured += null, new ExceptionEventArgs(new Exception()));

            Assert.AreEqual(0, errorOccurredSignalCount);
        }

        [TestMethod]
        public void HostKeyReceivedOnSessionShouldNoLongerBeSignaledViaHostKeyReceivedOnBaseClient()
        {
            var hostKeyReceivedSignalCount = 0;

            _client.HostKeyReceived += (sender, args) => Interlocked.Increment(ref hostKeyReceivedSignalCount);

            SessionMock.Raise(p => p.HostKeyReceived += null, new HostKeyEventArgs(GetKeyHostAlgorithm()));

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
            using (var s = TestBase.GetData("Key.RSA.txt"))
            {
                var privateKey = new PrivateKeyFile(s);
                return (KeyHostAlgorithm)privateKey.HostKeyAlgorithms.First();
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
