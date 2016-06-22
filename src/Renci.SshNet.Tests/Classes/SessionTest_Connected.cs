using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connected : SessionTest_ConnectedBase
    {
        private IgnoreMessage _ignoreMessage;

        protected override void Arrange()
        {
            base.Arrange();

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
            Assert.AreEqual("SSH-2.0-SshStub", Session.ServerVersion);
        }

        [TestMethod]
        public void WaitOnHandle_WaitHandle_ShouldThrowArgumentNullExceptionWhenWaitHandleIsNull()
        {
            WaitHandle waitHandle = null;

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
            WaitHandle waitHandle = null;
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
            WaitHandle waitHandle = null;
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
    }
}
