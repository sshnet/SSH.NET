using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_Dispose_SessionIsConnectedAndChannelIsOpen_EofReceived
    {
        private Mock<ISession> _sessionMock;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private ChannelStub _channel;
        private List<ChannelEventArgs> _channelClosedRegister;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private ManualResetEvent _channelClosedReceived;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_channelClosedReceived != null)
            {
                _channelClosedReceived.Dispose();
                _channelClosedReceived = null;
            }
        }

        private void Arrange()
        {
            var random = new Random();
            _localChannelNumber = (uint)random.Next(0, int.MaxValue);
            _localWindowSize = (uint)random.Next(0, int.MaxValue);
            _localPacketSize = (uint)random.Next(0, int.MaxValue);
            _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint)random.Next(0, int.MaxValue);
            _remotePacketSize = (uint)random.Next(0, int.MaxValue);
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _channelClosedReceived = new ManualResetEvent(false);

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber))).Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                .Callback<WaitHandle>(w =>
                    {
                        new Thread(() =>
                            {
                                Thread.Sleep(100);
                                // signal that the ChannelCloseMessage was received; we use this to verify whether we've actually
                                // waited on the EventWaitHandle to be set
                                _channelClosedReceived.Set();
                                // raise ChannelCloseReceived event to set waithandle for receiving
                                // SSH_MSG_CHANNEL_CLOSE message from server which is waited on after
                                // sending the SSH_MSG_CHANNEL_CLOSE message to the server
                                // 
                                // we're mocking the wait on the ChannelCloseMessage, but we still want
                                // to get the channel in the state that it would have after actually receiving
                                // the ChannelCloseMessage
                                _sessionMock.Raise(s => s.ChannelCloseReceived += null, new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                            }).Start();
                        w.WaitOne();
                    });

            _channel = new ChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
            _channel.SetIsOpen(true);

            _sessionMock.Raise(
                s => s.ChannelEofReceived += null,
                new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));
        }

        private void Act()
        {
            _channel.Dispose();
        }

        [TestMethod]
        public void IsOpenShouldReturnFalse()
        {
            Assert.IsFalse(_channel.IsOpen);
        }

        [TestMethod]
        public void TrySendMessageOnSessionShouldBeInvokedOnceForChannelCloseMessage()
        {
            _sessionMock.Verify(
                p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Once);
        }

        [TestMethod]
        public void TrySendMessageOnSessionShouldNeverBeInvokedForChannelEofMessage()
        {
            _sessionMock.Verify(
                p => p.TrySendMessage(It.Is<ChannelEofMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Never);
        }

        [TestMethod]
        public void WaitOnHandleOnSessionShouldBeInvokedOnce()
        {
            _sessionMock.Verify(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()), Times.Once);
        }

        [TestMethod]
        public void WaitOnHandleOnSessionShouldWaitForChannelCloseMessageToBeReceived()
        {
            Assert.IsTrue(_channelClosedReceived.WaitOne(0));
        }

        [TestMethod]
        public void ClosedEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelClosedRegister.Count);
            Assert.AreEqual(_localChannelNumber, _channelClosedRegister[0].ChannelNumber);
        }

        [TestMethod]
        public void ExceptionShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count);
        }
    }
}
