using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ClientChannelTest_OnSessionChannelOpenConfirmationReceived_OnOpenConfirmation_Exception
    {
        private Mock<ISession> _sessionMock;
        private uint _localChannelNumber;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _remoteChannelNumber;
        private ClientChannelStub _channel;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private Exception _onOpenConfirmationException;

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
            _localWindowSize = (uint) random.Next(1000, int.MaxValue);
            _localPacketSize = _localWindowSize - 1;
            _remoteChannelNumber = (uint) random.Next(0, int.MaxValue);
            _onOpenConfirmationException = new SystemException();
            _channelExceptionRegister = new List<ExceptionEventArgs>();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _channel = new ClientChannelStub(_sessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.OnOpenConfirmationException = _onOpenConfirmationException;
        }

        private void Act()
        {
            _sessionMock.Raise(s => s.ChannelOpenConfirmationReceived += null,
                new MessageEventArgs<ChannelOpenConfirmationMessage>(
                    new ChannelOpenConfirmationMessage(_localChannelNumber, _localWindowSize, _localPacketSize,
                        _remoteChannelNumber)));
        }

        [TestMethod]
        public void ExceptionEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelExceptionRegister.Count);
            Assert.AreSame(_onOpenConfirmationException, _channelExceptionRegister[0].Exception);
        }

        [TestMethod]
        public void OnErrorOccuredShouldBeInvokedOnce()
        {
            Assert.AreEqual(1, _channel.OnErrorOccurredInvocations.Count);
            Assert.AreSame(_onOpenConfirmationException, _channel.OnErrorOccurredInvocations[0]);
        }
    }
}
