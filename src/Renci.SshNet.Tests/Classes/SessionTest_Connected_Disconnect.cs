using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connected_Disconnect : SessionTest_ConnectedBase
    {
        protected override void Act()
        {
            Session.Disconnect();
        }

        [TestMethod]
        public void IsConnectedShouldReturnFalse()
        {
            Assert.IsFalse(Session.IsConnected);
        }

        [TestMethod]
        public void DisconnectShouldFinishImmediately()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Session.Disconnect();

            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500);
        }

        [TestMethod]
        public void DisconnectedIsNeverRaised()
        {
            Assert.AreEqual(0, DisconnectedRegister.Count);
        }

        [TestMethod]
        public void DisconnectReceivedIsNeverRaised()
        {
            Assert.AreEqual(0, DisconnectReceivedRegister.Count);
        }

        [TestMethod]
        public void ErrorOccurredIsNeverRaised()
        {
            Assert.AreEqual(0, ErrorOccurredRegister.Count, ErrorOccurredRegister.AsString());
        }

        [TestMethod]
        public void DisposeShouldFinishImmediately()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Session.Dispose();

            stopwatch.Stop();
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500);
        }

        [TestMethod]
        public void ReceiveOnServerSocketShouldReturnZero()
        {
            var buffer = new byte[1];

            var actual = ServerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void SendMessageShouldThrowSshConnectionException()
        {
            try
            {
                Session.SendMessage(new IgnoreMessage());
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
        public void ISession_MessageListenerCompletedShouldBeSignaled()
        {
            var session = (ISession) Session;

            Assert.IsNotNull(session.MessageListenerCompleted);
            Assert.IsTrue(session.MessageListenerCompleted.WaitOne());
        }

        [TestMethodAttribute]
        public void ISession_SendMessageShouldThrowSshConnectionException()
        {
            var session = (ISession) Session;

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
            var session = (ISession) Session;

            var actual = session.TrySendMessage(new IgnoreMessage());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ISession_WaitOnHandle_WaitHandle_ShouldThrowSshConnectionExceptionDetailingBadPacket()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(false);

            try
            {
                session.WaitOnHandle(waitHandle);
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
        public void ISession_WaitOnHandle_WaitHandleAndTimeout_ShouldThrowSshConnectionExceptionDetailingBadPacket()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(false);

            try
            {
                session.WaitOnHandle(waitHandle, Session.InfiniteTimeSpan);
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
        public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnDisconnected()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(false);

            var result = session.TryWait(waitHandle, Session.InfiniteTimeSpan);

            Assert.AreEqual(WaitResult.Disconnected, result);
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnDisconnected()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(false);
            Exception exception;

            var result = session.TryWait(waitHandle, Session.InfiniteTimeSpan, out exception);

            Assert.AreEqual(WaitResult.Disconnected, result);
            Assert.IsNull(exception);
        }

        [TestMethod]
        public void ClientSocketShouldNotBeConnected()
        {
            Assert.IsNotNull(ClientSocket);
            Assert.IsFalse(ClientSocket.Connected);
        }
    }
}