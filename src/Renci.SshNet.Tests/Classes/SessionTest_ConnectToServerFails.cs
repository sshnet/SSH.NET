using System;
using System.Net;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_ConnectToServerFails : SessionTestBase
    {
        private ConnectionInfo _connectionInfo;
        private Session _session;
        private SshConnectionException _connectException;
        private SshConnectionException _actualException;

        protected override void SetupData()
        {
            base.SetupData();

            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));
            _session = new Session(_connectionInfo, _serviceFactoryMock.Object, _socketFactoryMock.Object);
            _connectException = new SshConnectionException();
        }

        protected override void SetupMocks()
        {
            base.SetupMocks();

            _serviceFactoryMock.Setup(p => p.CreateConnector(_connectionInfo, _socketFactoryMock.Object))
                               .Returns(_connectorMock.Object);
            _connectorMock.Setup(p => p.Connect(_connectionInfo))
                          .Throws(_connectException);
        }

        protected override void Act()
        {
            try
            {
                _session.Connect();
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void ClientVersionIsRenciSshNet()
        {
            Assert.AreEqual("SSH-2.0-Renci.SshNet.SshClient.0.0.1", _session.ClientVersion);
        }

        [TestMethod]
        public void ConnectionInfoShouldReturnConnectionInfoPassedThroughConstructor()
        {
            Assert.AreSame(_connectionInfo, _session.ConnectionInfo);
        }

        public void ConnectShouldHaveRethrownException()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreSame(_connectException, _actualException);
        }

        [TestMethod]
        public void DisconnectShouldNotThrowAnException()
        {
            _session.Disconnect();
        }

        [TestMethod]
        public void DisposeShouldNotThrowException()
        {
            _session.Dispose();
        }

        [TestMethod]
        public void IsConnectedShouldReturnFalse()
        {
            Assert.IsFalse(_session.IsConnected);
        }

        [TestMethod]
        public void SendMessageShouldThrowShhConnectionException()
        {
            try
            {
                _session.SendMessage(new IgnoreMessage());
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                Assert.AreEqual(DisconnectReason.None, ex.DisconnectReason);
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Client not connected.", ex.Message);
            }
        }

        [TestMethod]
        public void SessionIdShouldReturnNull()
        {
            Assert.IsNull(_session.SessionId);
        }

        [TestMethod]
        public void ServerVersionShouldReturnNull()
        {
            Assert.IsNull(_session.ServerVersion);
        }

        [TestMethod]
        public void WaitOnHandle_WaitOnHandle_WaitHandle_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            const WaitHandle waitHandle = null;

            try
            {
                _session.WaitOnHandle(waitHandle);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("waitHandle", ex.ParamName);
            }
        }

        [TestMethod]
        public void WaitOnHandle_WaitOnHandle_WaitHandleAndTimeout_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            const WaitHandle waitHandle = null;
            var timeout = TimeSpan.FromMinutes(5);

            try
            {
                _session.WaitOnHandle(waitHandle, timeout);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("waitHandle", ex.ParamName);
            }
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnDisconnected()
        {
            var session = (ISession)_session;
            var waitHandle = new ManualResetEvent(false);

            var result = session.TryWait(waitHandle, Session.InfiniteTimeSpan);

            Assert.AreEqual(WaitResult.Disconnected, result);
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnDisconnected()
        {
            var session = (ISession)_session;
            var waitHandle = new ManualResetEvent(false);
            Exception exception;

            var result = session.TryWait(waitHandle, Session.InfiniteTimeSpan, out exception);

            Assert.AreEqual(WaitResult.Disconnected, result);
            Assert.IsNull(exception);
        }

        [TestMethod]
        public void ISession_ConnectionInfoShouldReturnConnectionInfoPassedThroughConstructor()
        {
            var session = (ISession)_session;
            Assert.AreSame(_connectionInfo, session.ConnectionInfo);
        }

        [TestMethod]
        public void ISession_MessageListenerCompletedShouldBeSignaled()
        {
            var session = (ISession)_session;

            Assert.IsNotNull(session.MessageListenerCompleted);
            Assert.IsTrue(session.MessageListenerCompleted.WaitOne(0));
        }

        [TestMethod]
        public void ISession_SendMessageShouldThrowShhConnectionException()
        {
            var session = (ISession)_session;

            try
            {
                session.SendMessage(new IgnoreMessage());
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                Assert.AreEqual(DisconnectReason.None, ex.DisconnectReason);
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Client not connected.", ex.Message);
            }
        }

        [TestMethod]
        public void ISession_TrySendMessageShouldReturnFalse()
        {
            var session = (ISession)_session;

            var actual = session.TrySendMessage(new IgnoreMessage());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ISession_WaitOnHandle_WaitHandle_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            const WaitHandle waitHandle = null;
            var session = (ISession)_session;

            try
            {
                session.WaitOnHandle(waitHandle);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("waitHandle", ex.ParamName);
            }
        }

        [TestMethod]
        public void ISession_WaitOnHandle_WaitHandleAndTimeout_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            const WaitHandle waitHandle = null;
            var session = (ISession)_session;

            try
            {
                session.WaitOnHandle(waitHandle, Session.InfiniteTimeSpan);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("waitHandle", ex.ParamName);
            }
        }

        private static ConnectionInfo CreateConnectionInfo(IPEndPoint serverEndPoint, TimeSpan timeout)
        {
            var connectionInfo = new ConnectionInfo(
                serverEndPoint.Address.ToString(),
                serverEndPoint.Port,
                "eric",
                new NoneAuthenticationMethod("eric"));
            connectionInfo.Timeout = timeout;
            return connectionInfo;
        }
    }
}