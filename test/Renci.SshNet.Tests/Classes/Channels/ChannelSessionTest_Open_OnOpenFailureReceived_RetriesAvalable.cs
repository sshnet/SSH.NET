using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelSessionTest_Open_OnOpenFailureReceived_RetriesAvalable : ChannelSessionTestBase
    {
        private ChannelSession _channel;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private IList<ChannelEventArgs> _channelClosedRegister;
        private List<ExceptionEventArgs> _channelExceptionRegister;
        private SemaphoreLight _sessionSemaphore;
        private int _initialSessionSemaphoreCount;
        private uint _failureReasonCode;
        private string _failureDescription;
        private string _failureLanguage;

        protected override void SetupData()
        {
            var random = new Random();

            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _localWindowSize = (uint) random.Next(2000, 3000);
            _localPacketSize = (uint) random.Next(1000, 2000);
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint) random.Next(0, int.MaxValue);
            _remotePacketSize = (uint) random.Next(0, int.MaxValue);
            _initialSessionSemaphoreCount = random.Next(10, 20);
            _sessionSemaphore = new SemaphoreLight(_initialSessionSemaphoreCount);
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();

            _failureReasonCode = (uint) random.Next(0, int.MaxValue);
            _failureDescription = random.Next().ToString(CultureInfo.InvariantCulture);
            _failureLanguage = random.Next().ToString(CultureInfo.InvariantCulture);
        }

        protected override void SetupMocks()
        {
            var sequence = new MockSequence();

            SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
            ConnectionInfoMock.InSequence(sequence).Setup(p => p.RetryAttempts).Returns(2);
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
                                    s => s.ChannelOpenFailureReceived += null,
                                    new MessageEventArgs<ChannelOpenFailureMessage>(
                                        new ChannelOpenFailureMessage(
                                            _localChannelNumber,
                                            _failureDescription,
                                            _failureReasonCode,
                                            _failureLanguage
                                        )));
                                w.WaitOne();
                            });
            SessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(ConnectionInfoMock.Object);
            ConnectionInfoMock.InSequence(sequence).Setup(p => p.RetryAttempts).Returns(2);
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
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelSession(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        }

        protected override void Act()
        {
            _channel.Open();
        }

        [TestMethod]
        public void CurrentCountOfSessionSemaphoreShouldBeOneLessThanToInitialCount()
        {
            Assert.AreEqual(_initialSessionSemaphoreCount - 1, _sessionSemaphore.CurrentCount);
        }

        [TestMethod]
        public void ExceptionShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count);
        }

        [TestMethod]
        public void ClosedEventShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelClosedRegister.Count);
        }

        [TestMethod]
        public void IsOpenShouldReturnTrue()
        {
            Assert.IsTrue(_channel.IsOpen);
        }
    }
}