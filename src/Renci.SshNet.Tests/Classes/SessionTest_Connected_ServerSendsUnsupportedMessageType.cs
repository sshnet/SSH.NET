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
    /// <summary>
    /// This test verifies the current behavior, but this is not necessarily the behavior we want.
    /// We should consider treating any exception as a "disconnect" since we're effectively interrupting
    /// the message loop.
    /// </summary>
    [TestClass]
    public class SessionTest_Connected_ServerSendsUnsupportedMessageType : SessionTest_ConnectedBase
    {
        private byte[] _packet;

        protected override void SetupData()
        {
            base.SetupData();

            _packet = CreatePacketForUnsupportedMessageType();
        }

        protected override void Act()
        {
            ServerSocket.Send(_packet, 0, _packet.Length, SocketFlags.None);

            // give session some time to process packet
            Thread.Sleep(200);
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
            Assert.AreEqual(typeof(SshException), exception.GetType());

            var sshException = (SshException) exception;
            Assert.IsNull(sshException.InnerException);
            Assert.AreEqual("Message type 255 is not supported.", sshException.Message);
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
        public void ReceiveOnServerSocketShouldTimeout()
        {
            var buffer = new byte[1];

            ServerSocket.ReceiveTimeout = 500;
            try
            {
                ServerSocket.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(SocketError.TimedOut, ex.SocketErrorCode);
            }
        }

        [TestMethod]
        public void SendMessageShouldSendMessageToServer()
        {
            byte[] bytesReceivedByServer = null;
            ServerListener.BytesReceived += (received, socket) => bytesReceivedByServer = received;

            Session.SendMessage(new IgnoreMessage());

            // allow "server" some time to receive message
            Thread.Sleep(100);

            Assert.IsNotNull(bytesReceivedByServer);
            Assert.AreEqual(24, bytesReceivedByServer.Length);
        }

        [TestMethod]
        public void ISession_MessageListenerCompletedShouldBeSignaled()
        {
            var session = (ISession) Session;

            Assert.IsNotNull(session.MessageListenerCompleted);
            Assert.IsTrue(session.MessageListenerCompleted.WaitOne());
        }

        [TestMethodAttribute]
        public void ISession_SendMessageShouldSendMessageToServer()
        {
            var session = (ISession) Session;

            byte[] bytesReceivedByServer = null;
            ServerListener.BytesReceived += (received, socket) => bytesReceivedByServer = received;

            session.SendMessage(new IgnoreMessage());

            // allow "server" some time to receive message
            Thread.Sleep(100);

            Assert.IsNotNull(bytesReceivedByServer);
            Assert.AreEqual(24, bytesReceivedByServer.Length);
        }

        [TestMethod]
        public void ISession_TrySendMessageShouldReturnTrueAndSendMessageToServer()
        {
            var session = (ISession) Session;

            byte[] bytesReceivedByServer = null;
            ServerListener.BytesReceived += (received, socket) => bytesReceivedByServer = received;

            var actual = session.TrySendMessage(new IgnoreMessage());

            Assert.IsTrue(actual);

            // allow "server" some time to receive message
            Thread.Sleep(100);

            Assert.IsNotNull(bytesReceivedByServer);
            Assert.AreEqual(24, bytesReceivedByServer.Length);
        }

        [TestMethod]
        public void ISession_WaitOnHandleShouldThrowSshExceptionDetailingError()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(false);

            try
            {
                session.WaitOnHandle(waitHandle);
                Assert.Fail();
            }
            catch (SshException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Message type 255 is not supported.", ex.Message);
            }
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeout_ShouldReturnFailed()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(false);

            var result = session.TryWait(waitHandle, Session.InfiniteTimeSpan);

            Assert.AreEqual(WaitResult.Failed, result);
        }

        [TestMethod]
        public void ISession_TryWait_WaitHandleAndTimeoutAndException_ShouldReturnFailed()
        {
            var session = (ISession) Session;
            var waitHandle = new ManualResetEvent(false);
            Exception exception;

            var result = session.TryWait(waitHandle, Session.InfiniteTimeSpan, out exception);

            Assert.AreEqual(WaitResult.Failed, result);
            Assert.IsNotNull(exception);
            Assert.AreEqual(typeof(SshException), exception.GetType());

            var sshException = exception as SshException;
            Assert.IsNotNull(sshException);
            Assert.IsNull(sshException.InnerException);
            Assert.AreEqual("Message type 255 is not supported.", sshException.Message);
        }

        private static byte[] CreatePacketForUnsupportedMessageType()
        {
            byte messageType = 255;
            byte messageLength = 1;
            byte paddingLength = 10;
            var packetDataLength = (uint) messageLength + paddingLength + 1;

            var sshDataStream = new SshDataStream(4 + 1 + messageLength + paddingLength);
            sshDataStream.Write(packetDataLength);
            sshDataStream.WriteByte(paddingLength);
            sshDataStream.WriteByte(messageType);
            sshDataStream.Write(new byte[paddingLength]);

            return sshDataStream.ToArray();
        }
    }
}