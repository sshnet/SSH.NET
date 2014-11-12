using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_OnSessionChannelSuccessReceived_OnSuccess_Exception
    {
        private Mock<ISession> _sessionMock;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _localChannelNumber;
        private ChannelStub _channel;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private Exception _onSuccessException;

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
            _localWindowSize = (uint)random.Next(0, 1000);
            _localPacketSize = (uint)random.Next(1001, int.MaxValue);
            _onSuccessException = new SystemException();
            _channelExceptionRegister = new List<ExceptionEventArgs>();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _channel = new ChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.OnSuccessException = _onSuccessException;
        }

        private void Act()
        {
            _sessionMock.Raise(s => s.ChannelSuccessReceived += null,
                new MessageEventArgs<ChannelSuccessMessage>(new ChannelSuccessMessage(_localChannelNumber)));
        }

        [TestMethod]
        public void ExceptionEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelExceptionRegister.Count);
            Assert.AreSame(_onSuccessException, _channelExceptionRegister[0].Exception);
        }

        [TestMethod]
        public void OnErrorOccuredShouldBeInvokedOnce()
        {
            Assert.AreEqual(1, _channel.OnErrorOccurredInvocations.Count);
            Assert.AreSame(_onSuccessException, _channel.OnErrorOccurredInvocations[0]);
        }
    }
}
