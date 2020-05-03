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
    [Ignore]
    public class ChannelTest_Dispose_SessionIsConnectedAndChannelIsOpen_EofNotReceived_SendEofInvoked : ChannelTestBase
    {
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private ChannelStub _channel;
        private Stopwatch _closeTimer;
        private ManualResetEvent _channelClosedEventHandlerCompleted;
        private List<ChannelEventArgs> _channelClosedRegister;
        private IList<ExceptionEventArgs> _channelExceptionRegister;

        [TestCleanup]
        public void TearDown()
        {
            if (_channelClosedEventHandlerCompleted != null)
            {
                _channelClosedEventHandlerCompleted.Dispose();
                _channelClosedEventHandlerCompleted = null;
            }
        }

        protected override void SetupData()
        {
            var random = new Random();

            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(0, int.MaxValue);
            _localPacketSize = (uint) random.Next(0, int.MaxValue);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(0, int.MaxValue);
            _closeTimer = new Stopwatch();
            _channelClosedEventHandlerCompleted = new ManualResetEvent(false);
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            SessionMock.InSequence(sequence).Setup(p => p.SendMessage(It.Is<ChannelEofMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)));
            SessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            SessionMock.InSequence(sequence).Setup(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber))).Returns(true);
            SessionMock.InSequence(sequence).Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                        .Callback<WaitHandle>(w =>
                        {
                            new Thread(() =>
                            {
                                Thread.Sleep(100);
                                // raise ChannelCloseReceived event to set waithandle for receiving
                                // SSH_MSG_CHANNEL_CLOSE message from server which is waited on after
                                // sending the SSH_MSG_CHANNEL_CLOSE message to the server
                                SessionMock.Raise(s => s.ChannelCloseReceived += null,
                                                   new MessageEventArgs<ChannelCloseMessage>(
                                                       new ChannelCloseMessage(_localChannelNumber)));
                            }).Start();
                            _closeTimer.Start();
                            try
                            {
                                w.WaitOne();
                            }
                            finally
                            {
                                _closeTimer.Stop();
                            }
                        });
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) =>
            {
                _channelClosedRegister.Add(args);
                Thread.Sleep(50);
                _channelClosedEventHandlerCompleted.Set();
            };
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
            _channel.SetIsOpen(true);
            //_channel.SendEof();
        }

        protected override void Act()
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
            SessionMock.Verify(
                p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Once);
        }

        [TestMethod]
        public void SendMessageOnSessionShouldBeInvokedOnceForChannelEofMessage()
        {
            SessionMock.Verify(
                p => p.SendMessage(It.Is<ChannelEofMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Once);
        }

        [TestMethod]
        public void WaitOnHandleOnSessionShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()), Times.Once);
        }

        [TestMethod]
        public void WaitOnHandleOnSessionShouldWaitForChannelCloseMessageToBeReceived()
        {
            Assert.IsTrue(_closeTimer.ElapsedMilliseconds >= 100, "Elapsed milliseconds=" + _closeTimer.ElapsedMilliseconds);
        }

        [TestMethod]
        public void ClosedEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelClosedRegister.Count);
            Assert.AreEqual(_localChannelNumber, _channelClosedRegister[0].ChannelNumber);
        }

        [TestMethod]
        public void DisposeShouldBlockUntilClosedEventHandlerHasCompleted()
        {
            Assert.IsTrue(_channelClosedEventHandlerCompleted.WaitOne(0));
        }

        [TestMethod]
        public void ExceptionShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count);
        }
    }
}