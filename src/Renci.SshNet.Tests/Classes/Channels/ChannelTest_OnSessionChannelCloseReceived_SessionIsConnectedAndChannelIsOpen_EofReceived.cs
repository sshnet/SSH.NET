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
    public class ChannelTest_OnSessionChannelCloseReceived_SessionIsConnectedAndChannelIsOpen_EofReceived
    {
        private Mock<ISession> _sessionMock;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private IList<ChannelEventArgs> _channelClosedRegister;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private ChannelStub _channel;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
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

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(
                p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)))
                .Returns(true);

            _channel = new ChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
            _channel.SetIsOpen(true);

            _sessionMock.Raise(p => p.ChannelEofReceived += null,
                new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));
        }

        private void Act()
        {
            _sessionMock.Raise(p => p.ChannelCloseReceived += null,
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
        public void SendMessageOnSessionShouldNeverBeInvokedForChannelEofMessage()
        {
            _sessionMock.Verify(
                p => p.TrySendMessage(It.Is<ChannelEofMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Never);
        }

        [TestMethod]
        public void WaitOnHandleOnSessionShouldNeverBeInvoked()
        {
            _sessionMock.Verify(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()), Times.Never);
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
