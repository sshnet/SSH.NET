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
    public class ChannelTest_Close_SessionIsConnectedAndChannelIsOpen_EofNotReceived_SendEofInvoked
    {
        private Mock<ISession> _sessionMock;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private ChannelStub _channel;
        private Stopwatch _closeTimer;
        private List<ChannelEventArgs> _channelClosedRegister;
        private IList<ExceptionEventArgs> _channelExceptionRegister;

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
            _closeTimer = new Stopwatch();
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _sessionMock.InSequence(sequence).Setup(p => p.SendMessage(It.Is<ChannelEofMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)));
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber))).Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()))
                .Callback<WaitHandle>(w =>
                {
                    new Thread(() =>
                    {
                        Thread.Sleep(100);
                        // raise ChannelCloseReceived event to set waithandle for receiving
                        // SSH_MSG_CHANNEL_CLOSE message from server which is waited on after
                        // sending the SSH_MSG_CHANNEL_CLOSE message to the server
                        _sessionMock.Raise(s => s.ChannelCloseReceived += null,
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

            _channel = new ChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
            _channel.SetIsOpen(true);
            _channel.SendEof();
        }

        private void Act()
        {
            _channel.Close();
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
        public void SendMessageOnSessionShouldBeInvokedOnceForChannelEofMessage()
        {
            _sessionMock.Verify(
                p => p.SendMessage(It.Is<ChannelEofMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)),
                Times.Once);
        }

        [TestMethod]
        public void WaitOnHandleOnSessionShouldBeInvokedOnce()
        {
            _sessionMock.Verify(p => p.WaitOnHandle(It.IsAny<EventWaitHandle>()), Times.Once);
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
        public void ExceptionShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count);
        }
    }
}
