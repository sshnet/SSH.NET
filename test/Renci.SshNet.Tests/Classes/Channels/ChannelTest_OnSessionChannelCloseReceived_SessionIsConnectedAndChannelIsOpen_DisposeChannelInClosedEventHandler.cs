using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_OnSessionChannelCloseReceived_SessionIsConnectedAndChannelIsOpen_DisposeChannelInClosedEventHandler : ChannelTestBase
    {
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private TimeSpan _channelCloseTimeout;
        private ChannelStub _channel;
        private List<ChannelEventArgs> _channelClosedRegister;
        private List<ChannelEventArgs> _channelEndOfDataRegister;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private ManualResetEvent _channelClosedEventHandlerCompleted;
        private bool _channelClosedWaitHandleSignaled;

        protected override void SetupData()
        {
            var random = new Random();

            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(0, int.MaxValue);
            _localPacketSize = (uint) random.Next(0, int.MaxValue);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(0, int.MaxValue);
            _channelCloseTimeout = TimeSpan.FromSeconds(random.Next(10, 20));
            _channelClosedWaitHandleSignaled = false;
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelEndOfDataRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _channelClosedEventHandlerCompleted = new ManualResetEvent(false);
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            SessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            SessionMock.InSequence(sequence).Setup(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber))).Returns(true);
            SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
            ConnectionInfoMock.InSequence(sequence).Setup(p => p.ChannelCloseTimeout).Returns(_channelCloseTimeout);
            SessionMock.InSequence(sequence)
                        .Setup(p => p.TryWait(It.IsAny<EventWaitHandle>(), _channelCloseTimeout))
                        .Callback<WaitHandle, TimeSpan>((waitHandle, channelCloseTimeout) => _channelClosedWaitHandleSignaled = waitHandle.WaitOne(0))
                        .Returns(WaitResult.Success);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) =>
                {
                    _channelClosedRegister.Add(args);
                    _channel.Dispose();
                    _channelClosedEventHandlerCompleted.Set();
                };
            _channel.EndOfData += (sender, args) => _channelEndOfDataRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
            _channel.SetIsOpen(true);
        }

        protected override void Act()
        {
            SessionMock.Raise(
                s => s.ChannelCloseReceived += null,
                new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
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
        public void TrySendMessageOnSessionShouldNeverBeInvokedForChannelEofMessage()
        {
            SessionMock.Verify(
                p => p.TrySendMessage(It.Is<ChannelEofMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Never);
        }

        [TestMethod]
        public void TryWaitOnSessionShouldBeInvokedOnce()
        {
            SessionMock.Verify(p => p.TryWait(It.IsAny<EventWaitHandle>(), _channelCloseTimeout), Times.Once);
        }

        [TestMethod]
        public void ChannelCloseWaitHandleShouldHaveBeenSignaledImmediately()
        {
            Assert.IsTrue(_channelClosedWaitHandleSignaled);
        }

        [TestMethod]
        public void ClosedEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelClosedRegister.Count);
            Assert.AreEqual(_localChannelNumber, _channelClosedRegister[0].ChannelNumber);
        }

        [TestMethod]
        public void EndOfDataEventShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelEndOfDataRegister.Count);
        }

        [TestMethod]
        public void ExceptionShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count);
        }

        [TestMethod]
        public void ChannelCloseReceivedShouldBlockUntilClosedEventHandlerHasCompleted()
        {
            Assert.IsTrue(_channelClosedEventHandlerCompleted.WaitOne(0));
        }
    }
}