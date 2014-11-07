using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Messages;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_Close_SessionIsConnectedAndChannelIsNotOpen
    {
        private Mock<ISession> _sessionMock;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _localChannelNumber;
        private Channel _channel;
        private List<ChannelEventArgs> _channelClosedRegister;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        private void Arrange()
        {
            var random = new Random();
            _localWindowSize = (uint)random.Next(0, int.MaxValue);
            _localPacketSize = (uint)random.Next(0, int.MaxValue);
            _localChannelNumber = (uint)random.Next(0, int.MaxValue);
            _channelClosedRegister = new List<ChannelEventArgs>();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _sessionMock.Setup(p => p.NextChannelNumber).Returns(_localChannelNumber);
            _sessionMock.Setup(p => p.IsConnected).Returns(true);

            _channel = new ChannelStub();
            _channel.Closed += (sender, args) =>
            {
                lock (this)
                {
                    _channelClosedRegister.Add(args);
                }
            };
            _channel.Initialize(_sessionMock.Object, _localWindowSize, _localPacketSize);
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
        public void SendMessageOnSessionShouldNeverBeInvoked()
        {
            _sessionMock.Verify(p => p.SendMessage(It.IsAny<Message>()), Times.Never);
        }

        [TestMethod]
        public void ClosedEventShouldNeverHaveFired()
        {
            Assert.AreEqual(0, _channelClosedRegister.Count);
        }
    }
}
