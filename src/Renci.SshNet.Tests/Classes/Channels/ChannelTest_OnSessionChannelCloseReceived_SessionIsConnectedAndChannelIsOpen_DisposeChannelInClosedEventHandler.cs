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
    public class ChannelTest_OnSessionChannelCloseReceived_SessionIsConnectedAndChannelIsOpen_DisposeChannelInClosedEventHandler
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
        private List<ChannelEventArgs> _channelEndOfDataRegister;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private ManualResetEvent _channelClosedEventHandlerCompleted;

        private void SetupData()
        {
            var random = new Random();

            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(0, int.MaxValue);
            _localPacketSize = (uint) random.Next(0, int.MaxValue);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(0, int.MaxValue);
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelEndOfDataRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _channelClosedEventHandlerCompleted = new ManualResetEvent(false);
        }

        private void CreateMocks()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            var sequence = new MockSequence();

            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)))
                        .Returns(true);
            _sessionMock.InSequence(sequence)
                        .Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                        .Callback<WaitHandle>(w => w.WaitOne());
        }

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        private void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _channel = new ChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
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

        private void Act()
        {
            _sessionMock.Raise(
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