using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_SendEof_ChannelIsOpen
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
            _channelClosedRegister = new List<ChannelEventArgs>();
            _channelExceptionRegister = new List<ExceptionEventArgs>();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _sessionMock.Setup(p => p.SendMessage(It.Is<ChannelEofMessage>(e => e.LocalChannelNumber == _remoteChannelNumber)));

            _channel = new ChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Closed += (sender, args) => _channelClosedRegister.Add(args);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.InitializeRemoteChannelInfo(_remoteChannelNumber, _remoteWindowSize, _remotePacketSize);
            _channel.SetIsOpen(true);
        }

        private void Act()
        {
            _channel.SendEof();
        }

        [TestMethod]
        public void IsOpenShouldReturnTrue()
        {
            Assert.IsTrue(_channel.IsOpen);
        }

        [TestMethod]
        public void SendMessageOnSessionShouldBeInvokedOnceWithChannelEofMessage()
        {
            _sessionMock.Verify(
                p => p.SendMessage(It.Is<ChannelEofMessage>(e => e.LocalChannelNumber == _remoteChannelNumber)),
                Times.Once);
        }

        [TestMethod]
        public void ClosedEventShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelClosedRegister.Count);
        }

        [TestMethod]
        public void ExceptionShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count);
        }
    }
}
