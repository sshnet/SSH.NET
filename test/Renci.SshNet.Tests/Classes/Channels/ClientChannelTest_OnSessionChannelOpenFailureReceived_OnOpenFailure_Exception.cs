using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ClientChannelTest_OnSessionChannelOpenFailureReceived_OnOpenFailure_Exception
    {
        private Mock<ISession> _sessionMock;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private ClientChannelStub _channel;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private Exception _onOpenFailureException;

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
            _localWindowSize = (uint)random.Next(1000, int.MaxValue);
            _localPacketSize = _localWindowSize - 1;
            _onOpenFailureException = new SystemException();
            _channelExceptionRegister = new List<ExceptionEventArgs>();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _channel = new ClientChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.OnOpenFailureException = _onOpenFailureException;
        }

        private void Act()
        {
            _sessionMock.Raise(s => s.ChannelOpenFailureReceived += null,
                new MessageEventArgs<ChannelOpenFailureMessage>(
                    new ChannelOpenFailureMessage(_localChannelNumber, "", ChannelOpenFailureMessage.ConnectFailed)));
        }

        [TestMethod]
        public void ExceptionEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelExceptionRegister.Count);
            Assert.AreSame(_onOpenFailureException, _channelExceptionRegister[0].Exception);
        }

        [TestMethod]
        public void OnErrorOccuredShouldBeInvokedOnce()
        {
            Assert.AreEqual(1, _channel.OnErrorOccurredInvocations.Count);
            Assert.AreSame(_onOpenFailureException, _channel.OnErrorOccurredInvocations[0]);
        }
    }
}
