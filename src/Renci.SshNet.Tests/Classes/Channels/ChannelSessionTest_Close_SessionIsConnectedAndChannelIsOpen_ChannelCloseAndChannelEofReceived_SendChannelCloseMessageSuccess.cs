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
    public class ChannelSessionTest_Close_SessionIsConnectedAndChannelIsOpen_ChannelCloseAndChannelEofReceived_SendChannelCloseMessageSuccess
    {
        private Mock<ISession> _sessionMock;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private uint _remoteWindowSize;
        private uint _remotePacketSize;
        private IList<ChannelEventArgs> _channelClosedRegister;
        private List<ExceptionEventArgs> _channelExceptionRegister;
        private ChannelSession _channel;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private MockSequence _sequence;
        private SemaphoreLight _sessionSemaphore;
        private int _initialSessionSemaphoreCount;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        private void Arrange()
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

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

            _sequence = new MockSequence();
            _sessionMock.InSequence(_sequence).Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _connectionInfoMock.InSequence(_sequence).Setup(p => p.RetryAttempts).Returns(1);
            _sessionMock.Setup(p => p.SessionSemaphore).Returns(_sessionSemaphore);
            _sessionMock.InSequence(_sequence)
                .Setup(
                    p =>
                        p.SendMessage(
                            It.Is<ChannelOpenMessage>(
                                m =>
                                    m.LocalChannelNumber == _localChannelNumber &&
                                    m.InitialWindowSize == _localWindowSize && m.MaximumPacketSize == _localPacketSize &&
                                    m.Info is SessionChannelOpenInfo)));
            _sessionMock.InSequence(_sequence)
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
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(_sequence)
                .Setup(
                    p => p.TrySendMessage(It.Is<ChannelCloseMessage>(c => c.LocalChannelNumber == _remoteChannelNumber)))
                .Returns(true);

            _channel = new ChannelSession(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.Open();

            _sessionMock.Raise(
                p => p.ChannelEofReceived += null,
                new MessageEventArgs<ChannelEofMessage>(new ChannelEofMessage(_localChannelNumber)));
            _sessionMock.Raise(
                p => p.ChannelCloseReceived += null,
                new MessageEventArgs<ChannelCloseMessage>(new ChannelCloseMessage(_localChannelNumber)));
        }

        private void Act()
        {
            _channel.Close();
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
