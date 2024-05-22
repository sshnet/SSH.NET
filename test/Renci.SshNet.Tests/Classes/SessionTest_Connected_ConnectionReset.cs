﻿using System.Diagnostics;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SessionTest_Connected_ConnectionReset : SessionTest_ConnectedBase
    {
        protected override void Act()
        {
            ServerSocket.Close();

            // give session some time to react to connection reset
            Thread.Sleep(300);
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
        public void ErrorOccurredIsRaisedOnce()
        {
            Assert.AreEqual(1, ErrorOccurredRegister.Count, ErrorOccurredRegister.AsString());

            var errorOccurred = ErrorOccurredRegister[0];
            Assert.IsNotNull(errorOccurred);

            var exception = errorOccurred.Exception;
            Assert.IsNotNull(exception);
            Assert.AreEqual(typeof(SshConnectionException), exception.GetType());

            var connectionException = (SshConnectionException)exception;
            Assert.AreEqual(DisconnectReason.ConnectionLost, connectionException.DisconnectReason);
            Assert.IsNull(connectionException.InnerException);
            Assert.AreEqual("An established connection was aborted by the server.", connectionException.Message);
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
            var session = (ISession)Session;

            Assert.IsNotNull(session.MessageListenerCompleted);
            Assert.IsTrue(session.MessageListenerCompleted.WaitOne());
        }

        [TestMethod]
        public void ISession_SendMessageShouldThrowSshConnectionException()
        {
            var session = (ISession)Session;

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
            var session = (ISession)Session;

            var actual = session.TrySendMessage(new IgnoreMessage());

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ISession_WaitOnHandle_WaitHandle_ShouldThrowSshConnectionException()
        {
            var session = (ISession)Session;
            var waitHandle = new ManualResetEvent(false);

            try
            {
                session.WaitOnHandle(waitHandle);
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                Assert.AreEqual("An established connection was aborted by the server.", ex.Message);
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(DisconnectReason.ConnectionLost, ex.DisconnectReason);
            }
        }

        [TestMethod]
        public void ISession_WaitOnHandle_WaitHandleAndTimeout_ShouldThrowSshConnectionException()
        {
            var session = (ISession)Session;
            var waitHandle = new ManualResetEvent(false);

            try
            {
                session.WaitOnHandle(waitHandle, Timeout.InfiniteTimeSpan);
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                Assert.AreEqual(DisconnectReason.ConnectionLost, ex.DisconnectReason);
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("An established connection was aborted by the server.", ex.Message);
            }
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnDisconnected()
        {
            var session = (ISession)Session;
            var waitHandle = new ManualResetEvent(false);

            var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan);

            Assert.AreEqual(WaitResult.Disconnected, result);
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnDisconnected()
        {
            var session = (ISession)Session;
            var waitHandle = new ManualResetEvent(false);

            var result = session.TryWait(waitHandle, Timeout.InfiniteTimeSpan, out var exception);

            Assert.AreEqual(WaitResult.Disconnected, result);
            Assert.IsNull(exception);
        }
    }
}
