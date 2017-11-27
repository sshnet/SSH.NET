using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelSessionTest_Dispose_SessionIsConnectedAndChannelIsOpen_NoChannelCloseOrChannelEofReceived : ChannelSessionTestBase
    {
        private ChannelSession _channel;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private uint _remoteChannelNumber;
        private TimeSpan _channelCloseTimeout;
        private SemaphoreLight _sessionSemaphore;
        private IList<ChannelEventArgs> _channelClosedRegister;
        private List<ExceptionEventArgs> _channelExceptionRegister;

        protected override void SetupData()
        {
            var random = new Random();

            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(2000, 3000);
            _localPacketSize = (uint) random.Next(1000, 2000);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(100, 200);
            _channelCloseTimeout = TimeSpan.FromSeconds(random.Next(10, 20));
            _sessionSemaphore = new SemaphoreLight(1);
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
            ConnectionInfoMock.InSequence(sequence).Setup(p => p.RetryAttempts).Returns(1);
            SessionMock.Setup(p => p.SessionSemaphore).Returns(_sessionSemaphore);
            SessionMock.InSequence(sequence)
                        .Setup(
                            p =>
                                p.SendMessage(
                                    It.Is<ChannelOpenMessage>(
                                        m =>
                                            m.LocalChannelNumber == _localChannelNumber &&
                                            m.InitialWindowSize == _localWindowSize && m.MaximumPacketSize == _localPacketSize &&
                                            m.Info is SessionChannelOpenInfo)));
            SessionMock.InSequence(sequence)
                        .Setup(p => p.WaitOnHandle(It.IsNotNull<WaitHandle>()))
                        .Callback<WaitHandle>(
                            w =>
                            {
                                SessionMock.Raise(
                                    s => s.ChannelOpenConfirmationReceived += null,
                                    new MessageEventArgs<ChannelOpenConfirmationMessage>(
                                        new ChannelOpenConfirmationMessage(
                                            _localChannelNumber,
                                            _remoteWindowSize,
                                            _remotePacketSize,
                                            _remoteChannelNumber)));
                                w.WaitOne();
                            });
            SessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            SessionMock.InSequence(sequence)
                        .Setup(
                            p => p.TrySendMessage(It.Is<ChannelEofMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                        .Returns(true);
            SessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            SessionMock.InSequence(sequence)
                        .Setup(
                            p => p.TrySendMessage(It.Is<ChannelCloseMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)))
                        .Returns(true);
            SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
            ConnectionInfoMock.InSequence(sequence).Setup(p => p.ChannelCloseTimeout).Returns(_channelCloseTimeout);
            SessionMock.InSequence(sequence)
                       .Setup(p => p.TryWait(It.IsNotNull<WaitHandle>(), _channelCloseTimeout))
                       .Callback<WaitHandle, TimeSpan>(
                           (waitHandle, channelCloseTimeout) =>
                           {
                               SessionMock.Raise(
                                   s => s.ChannelCloseReceived += null,
                                   new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
                               waitHandle.WaitOne();
                           })
                       .Returns(WaitResult.Success);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelSession(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.Open();
        }

        protected override void Act()
        {
            _channel.Dispose();
        }

        [TestMethod]
        public void ChannelEofMessageShouldBeSentOnce()
        {
            SessionMock.Verify(p => p.TrySendMessage(It.Is<ChannelEofMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)), Times.Once);
        }

        [TestMethod]
        public void ChannelCloseMessageShouldBeSentOnce()
        {
            SessionMock.Verify(p => p.TrySendMessage(It.Is<ChannelCloseMessage>(m => m.LocalChannelNumber == _remoteChannelNumber)), Times.Once);
        }

        [TestMethod]
        public void ExceptionShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count);
        }

        [TestMethod]
        public void ClosedEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelClosedRegister.Count);
            Assert.AreEqual(_localChannelNumber, _channelClosedRegister[0].ChannelNumber);
        }

        [TestMethod]
        public void IsOpenShouldReturnFalse()
        {
            Assert.IsFalse(_channel.IsOpen);
        }
    }
}