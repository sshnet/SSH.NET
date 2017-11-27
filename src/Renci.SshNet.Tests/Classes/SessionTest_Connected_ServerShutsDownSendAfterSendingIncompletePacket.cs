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
    public class SessionTest_Connected_ServerShutsDownSendAfterSendingIncompletePacket : SessionTest_ConnectedBase
    {
        protected override void Act()
        {
            var incompletePacket = new byte[] {0x0a, 0x05, 0x05};
            ServerSocket.Send(incompletePacket, 0, incompletePacket.Length, SocketFlags.None);

            // give session some time to start reading packet
            Thread.Sleep(100);

            ServerSocket.Shutdown(SocketShutdown.Send);

            // give session some time to process shut down of server socket
            Thread.Sleep(100);
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

            var connectionException = (SshConnectionException) exception;
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
        public void ReceiveOnServerSocketShouldReturnZero()
        {
            var buffer = new byte[1];

            var actual = ServerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void SendMessageShouldSucceed()
        {
            try
            {
                Session.SendMessage(new IgnoreMessage());
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
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

        [TestMethod]
        public void ISession_SendMessageShouldSucceed()
        {
            var session = (ISession) Session;

            try
            {
                session.SendMessage(new IgnoreMessage());
                Assert.Fail();
            }
            catch (SshConnectionException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Client not connected.", ex.Message);
            }
        }

        [TestMethod]
        public void ISession_TrySendMessageShouldReturnTrue()
        {
            var session = (ISession) Session;

            Assert.IsFalse(session.TrySendMessage(new IgnoreMessage()));
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
                Assert.AreEqual(DisconnectReason.ConnectionLost, ex.DisconnectReason);
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("An established connection was aborted by the server.", ex.Message);
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
                Assert.AreEqual(DisconnectReason.ConnectionLost, ex.DisconnectReason);
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("An established connection was aborted by the server.", ex.Message);
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
    }
}