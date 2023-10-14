using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_OnSessionChannelCloseReceived_SessionIsConnectedAndChannelIsOpen_EofReceived : ChannelTestBase
    {
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private TimeSpan _channelCloseTimeout;
        private IList<ChannelEventArgs> _channelClosedRegister;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private ManualResetEvent _channelClosedEventHandlerCompleted;
        private ChannelStub _channel;

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
            _channelClosedRegister = new List<ChannelEventArgs>();
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
                        .Callback<WaitHandle, TimeSpan>((waitHandle, channelCloseTimeout) => waitHandle.WaitOne())
                        .Returns(WaitResult.Success);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) =>
            {
                _channelClosedRegister.Add(args);
                Thread.Sleep(100);
                _channelClosedEventHandlerCompleted.Set();
            };
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
            _channel.SetIsOpen(true);

            SessionMock.Raise(p => p.ChannelEofReceived += null,
                               new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));
        }

        protected override void Act()
        {
            SessionMock.Raise(p => p.ChannelCloseReceived += null,
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
        public void SendMessageOnSessionShouldNeverBeInvokedForChannelEofMessage()
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
        public void ClosedEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelClosedRegister.Count);
            Assert.AreEqual(_localChannelNumber, _channelClosedRegister[0].ChannelNumber);
        }

        [TestMethod]
        public void ExceptionShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count, _channelExceptionRegister.AsString());
        }

        [TestMethod]
        public void ChannelCloseReceivedShouldBlockUntilClosedEventHandlerHasCompleted()
        {
            Assert.IsTrue(_channelClosedEventHandlerCompleted.WaitOne(0));
        }
    }
}