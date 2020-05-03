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
    public class ChannelSessionTest_Dispose_SessionIsNotConnectedAndChannelIsOpen_ChannelCloseReceived : ChannelSessionTestBase
    {
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private IList<ChannelEventArgs> _channelClosedRegister;
        private List<ExceptionEventArgs> _channelExceptionRegister;
        private ChannelSession _channel;
        private MockSequence _sequence;
        private SemaphoreLight _sessionSemaphore;
        private int _initialSessionSemaphoreCount;

        protected override void SetupData()
        {
            var random = new Random();

            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(0, int.MaxValue);
            _localPacketSize = (uint) random.Next(0, int.MaxValue);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(0, int.MaxValue);
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _initialSessionSemaphoreCount = random.Next(10, 20);
            _sessionSemaphore = new SemaphoreLight(_initialSessionSemaphoreCount);
        }

        protected override void SetupMocks()
        {
            _sequence = new MockSequence();

            SessionMock.InSequence(_sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
            ConnectionInfoMock.InSequence(_sequence).Setup(p => p.RetryAttempts).Returns(1);
            SessionMock.Setup(p => p.SessionSemaphore).Returns(_sessionSemaphore);
            SessionMock.InSequence(_sequence)
                        .Setup(
                            p =>
                                p.SendMessage(
                                    It.Is<ChannelOpenMessage>(
                                        m =>
                                            m.LocalChannelNumber == _localChannelNumber &&
                                            m.InitialWindowSize == _localWindowSize && m.MaximumPacketSize == _localPacketSize &&
                                            m.Info is SessionChannelOpenInfo)));
            SessionMock.InSequence(_sequence)
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
            SessionMock.Setup(p => p.IsConnected).Returns(false);
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelSession(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.Open();

            SessionMock.Raise(
                p => p.ChannelCloseReceived += null,
                new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
        }

        protected override void Act()
        {
            _channel.Dispose();
        }

        [TestMethod]
        public void CurrentCountOfSessionSemaphoreShouldBeEqualToInitialCount()
        {
            Assert.AreEqual(_initialSessionSemaphoreCount, _sessionSemaphore.CurrentCount);
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