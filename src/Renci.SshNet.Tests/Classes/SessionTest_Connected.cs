using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connected : SessionTest_ConnectedBase
    {
        private IgnoreMessage _ignoreMessage;

        protected override void SetupData()
        {
            base.SetupData();

            var data = new byte[10];
            Random.NextBytes(data);
            _ignoreMessage = new IgnoreMessage(data);
        }

        protected override void Act()
        {
        }

        [TestMethod]
        public void ClientVersionIsRenciSshNet()
        {
            Assert.AreEqual("SSH-2.0-Renci.SshNet.SshClient.0.0.1", Session.ClientVersion);
        }

        [TestMethod]
        public void ConnectionInfoShouldReturnConnectionInfoPassedThroughConstructor()
        {
            Assert.AreSame(ConnectionInfo, Session.ConnectionInfo);
        }

        [TestMethod]
        public void IsConnectedShouldReturnTrue()
        {
            Assert.IsTrue(Session.IsConnected);
        }

        [TestMethod]
        public void SendMessageShouldSendPacketToServer()
        {
            ServerBytesReceivedRegister.Clear();

            Session.SendMessage(_ignoreMessage);

            // give session time to process message
            Thread.Sleep(100);

            Assert.AreEqual(1, ServerBytesReceivedRegister.Count);
        }

        [TestMethod]
        public void SessionIdShouldReturnExchangeHashCalculatedFromKeyExchangeInitMessage()
        {
            Assert.IsNotNull(Session.SessionId);
            Assert.AreSame(SessionId, Session.SessionId);
        }

        [TestMethod]
        public void ServerVersionShouldNotReturnNull()
        {
            Assert.IsNotNull(Session.ServerVersion);
            Assert.AreEqual("SSH-2.0-OurServerStub", Session.ServerVersion);
        }

        [TestMethod]
        public void WaitOnHandle_WaitHandle_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            const WaitHandle waitHandle = null;

            try
            {
                Session.WaitOnHandle(waitHandle);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("waitHandle", ex.ParamName);
            }
        }

        [TestMethod]
        public void WaitOnHandle_WaitHandleAndTimeout_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            const WaitHandle waitHandle = null;
            var timeout = TimeSpan.FromMinutes(5);

            try
            {
                Session.WaitOnHandle(waitHandle, timeout);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("waitHandle", ex.ParamName);
            }
        }

        [TestMethod]
        public void ISession_ConnectionInfoShouldReturnConnectionInfoPassedThroughConstructor()
        {
            var session = (ISession) Session;
            Assert.AreSame(ConnectionInfo, session.ConnectionInfo);
        }

        [TestMethod]
        public void ISession_MessageListenerCompletedShouldNotBeSignaled()
        {
            var session = (ISession) Session;

            Assert.IsNotNull(session.MessageListenerCompleted);
            Assert.IsFalse(session.MessageListenerCompleted.WaitOne(0));
        }

        [TestMethod]
        public void ISession_SendMessageShouldSendPacketToServer()
        {
            var session = (ISession) Session;
            ServerBytesReceivedRegister.Clear();

            session.SendMessage(_ignoreMessage);

            // give session time to process message
            Thread.Sleep(100);

            Assert.AreEqual(1, ServerBytesReceivedRegister.Count);
        }

        [TestMethod]
        public void ISession_TrySendMessageShouldSendPacketToServerAndReturnTrue()
        {
            var session = (ISession) Session;
            ServerBytesReceivedRegister.Clear();

            var actual = session.TrySendMessage(new IgnoreMessage());

            // give session time to process message
            Thread.Sleep(100);

            Assert.IsTrue(actual);
            Assert.AreEqual(1, ServerBytesReceivedRegister.Count);
        }

        [TestMethod]
        public void ISession_WaitOnHandleShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            const WaitHandle waitHandle = null;
            var session = (ISession) Session;

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
        public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnSuccessIfWaitHandleIsSignaled()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(true);

            var result = session.TryWait(waitHandle, TimeSpan.FromMilliseconds(0));

            Assert.AreEqual(WaitResult.Success, result);
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnTimedOutIfWaitHandleIsNotSignaled()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(false);

            var result = session.TryWait(waitHandle, TimeSpan.FromMilliseconds(0));

            Assert.AreEqual(WaitResult.TimedOut, result);
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeout_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            var session = (ISession) Session;
            const WaitHandle waitHandle = null;

            try
            {
                session.TryWait(waitHandle, Session.InfiniteTimeSpan);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("waitHandle", ex.ParamName);
            }
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnSuccessIfWaitHandleIsSignaled()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(true);
            Exception exception;

            var result = session.TryWait(waitHandle, TimeSpan.FromMilliseconds(0), out exception);

            Assert.AreEqual(WaitResult.Success, result);
            Assert.IsNull(exception);
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnTimedOutIfWaitHandleIsNotSignaled()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(false);
            Exception exception;

            var result = session.TryWait(waitHandle, TimeSpan.FromMilliseconds(0), out exception);

            Assert.AreEqual(WaitResult.TimedOut, result);
            Assert.IsNull(exception);
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            var session = (ISession) Session;
            const WaitHandle waitHandle = null;
            Exception exception = null;

            try
            {
                session.TryWait(waitHandle, Session.InfiniteTimeSpan, out exception);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("waitHandle", ex.ParamName);
                Assert.IsNull(exception);
            }
        }

        [TestMethod]
        public void ClientSocketShouldBeConnected()
        {
            Assert.IsNotNull(ClientSocket);
            Assert.IsTrue(ClientSocket.Connected);
        }

        [TestMethod]
        public void CreateConnectorOnServiceFactoryShouldHaveBeenInvokedOnce()
        {
            ServiceFactoryMock.Verify(p => p.CreateConnector(ConnectionInfo, SocketFactoryMock.Object), Times.Once());
        }

        [TestMethod]
        public void ConnectorOnConnectorShouldHaveBeenInvokedOnce()
        {
            ConnectorMock.Verify(p => p.Connect(ConnectionInfo), Times.Once());
        }
    }
}