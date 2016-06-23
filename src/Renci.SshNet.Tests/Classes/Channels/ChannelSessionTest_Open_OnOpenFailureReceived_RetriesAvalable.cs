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
    public class ChannelSessionTest_Open_OnOpenFailureReceived_RetriesAvalable
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
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
            _localWindowSize = (uint)random.Next(2000, 3000);
            _localPacketSize = (uint)random.Next(1000, 2000);
            _remoteChannelNumber = (uint)random.Next(0, int.MaxValue);
            _remoteWindowSize = (uint)random.Next(0, int.MaxValue);
            _remotePacketSize = (uint)random.Next(0, int.MaxValue);
            _initialSessionSemaphoreCount = random.Next(10, 20);
            _sessionSemaphore = new SemaphoreLight(_initialSessionSemaphoreCount);
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();

            _failureReasonCode = (uint)random.Next(0, int.MaxValue);
            _failureDescription = random.Next().ToString(CultureInfo.InvariantCulture);
            _failureLanguage = random.Next().ToString(CultureInfo.InvariantCulture);

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _sessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _connectionInfoMock.InSequence(sequence).Setup(p => p.RetryAttempts).Returns(2);
            _sessionMock.Setup(p => p.SessionSemaphore).Returns(_sessionSemaphore);
            _sessionMock.InSequence(sequence)
                .Setup(
                    p =>
                        p.SendMessage(
                            It.Is<ChannelOpenMessage>(
                                m =>
                                    m.LocalChannelNumber == _localChannelNumber &&
                                    m.InitialWindowSize == _localWindowSize && m.MaximumPacketSize == _localPacketSize &&
                                    m.Info is SessionChannelOpenInfo)));
            _sessionMock.InSequence(sequence)
                .Setup(p => p.WaitOnHandle(It.IsNotNull<WaitHandle>()))
                .Callback<WaitHandle>(
                    w =>
                        {
                            _sessionMock.Raise(
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
            _sessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _connectionInfoMock.InSequence(sequence).Setup(p => p.RetryAttempts).Returns(2);
            _sessionMock.Setup(p => p.SessionSemaphore).Returns(_sessionSemaphore);
            _sessionMock.InSequence(sequence)
                .Setup(
                    p =>
                        p.SendMessage(
                            It.Is<ChannelOpenMessage>(
                                m =>
                                    m.LocalChannelNumber == _localChannelNumber &&
                                    m.InitialWindowSize == _localWindowSize && m.MaximumPacketSize == _localPacketSize &&
                                    m.Info is SessionChannelOpenInfo)));
            _sessionMock.InSequence(sequence)
                .Setup(p => p.WaitOnHandle(It.IsNotNull<WaitHandle>()))
                .Callback<WaitHandle>(
                    w =>
                    {
                        _sessionMock.Raise(
                            s => s.ChannelOpenConfirmationReceived += null,
                            new MessageEventArgs<ChannelOpenConfirmationMessage>(
                                new ChannelOpenConfirmationMessage(
                                    _localChannelNumber,
                                    _remoteWindowSize,
                                    _remotePacketSize,
                                    _remoteChannelNumber)));
                        w.WaitOne();
                    });

            _channel = new ChannelSession(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        }

        private void Act()
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