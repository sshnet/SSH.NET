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
    public class ChannelSessionTest_Open_ExceptionWaitingOnOpenConfirmation
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private ChannelSession _channel;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private IList<ChannelEventArgs> _channelClosedRegister;
        private List<ExceptionEventArgs> _channelExceptionRegister;
        private SemaphoreLight _sessionSemaphore;
        private int _initialSessionSemaphoreCount;
        private Exception _waitOnConfirmationException;
        private SystemException _actualException;

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
            _initialSessionSemaphoreCount = random.Next(10, 20);
            _sessionSemaphore = new SemaphoreLight(_initialSessionSemaphoreCount);
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _waitOnConfirmationException = new SystemException();

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
                .Throws(_waitOnConfirmationException);

            _channel = new ChannelSession(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        }

        private void Act()
        {
            try
            {
                _channel.Open();
            }
            catch (SystemException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void OpenShouldHaveRethrownExceptionThrownByWaitOnHandle()
        {
            
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
        public void ClosedEventShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelClosedRegister.Count);
        }

        [TestMethod]
        public void IsOpenShouldReturnFalse()
        {
            Assert.IsFalse(_channel.IsOpen);
        }
    }
}