﻿using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Transport;

namespace Renci.SshNet.Tests.Classes
{
    public class SessionTest_Connected_ServerSendsDisconnectMessage : SessionTest_ConnectedBase
    {
        private DisconnectMessage _disconnectMessage;

        protected override void SetupData()
        {
            base.SetupData();

            _disconnectMessage = new DisconnectMessage(DisconnectReason.ServiceNotAvailable, "Not today!");
        }

        protected override void Act()
        {
            var disconnect = _disconnectMessage.GetPacket(8, null);
            ServerSocket.Send(disconnect, 4, disconnect.Length - 4, SocketFlags.None);

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
        public void DisconnectedIsRaisedOnce()
        {
            Assert.AreEqual(1, DisconnectedRegister.Count);
        }

        [TestMethod]
        public void DisconnectReceivedIsRaisedOnce()
        {
            Assert.AreEqual(1, DisconnectReceivedRegister.Count);

            var disconnectMessage = DisconnectReceivedRegister[0].Message;
            Assert.IsNotNull(disconnectMessage);
            Assert.AreEqual(_disconnectMessage.Description, disconnectMessage.Description);
            Assert.AreEqual("en", disconnectMessage.Language);
            Assert.AreEqual(_disconnectMessage.ReasonCode, disconnectMessage.ReasonCode);
        }

        [TestMethod]
        public void ErrorOccurredIsNeverRaised()
        {
            Assert.AreEqual(0, ErrorOccurredRegister.Count);
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
            var session = (ISession)Session;

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
        public void ISession_WaitOnHandleShouldThrowSshConnectionExceptionDetailingDisconnectReason()
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
                Assert.AreEqual(DisconnectReason.ServiceNotAvailable, ex.DisconnectReason);
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, "The connection was closed by the server: {0} ({1}).", _disconnectMessage.Description, _disconnectMessage.ReasonCode), ex.Message);
            }
        }
    }
}
