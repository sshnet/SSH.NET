using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_OnSessionChannelRequestReceived_OnRequest_Exception
    {
        private Mock<ISession> _sessionMock;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _localChannelNumber;
        private ChannelStub _channel;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private Exception _onRequestException;
        private SignalRequestInfo _requestInfo;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        private void Arrange()
        {
            var random = new Random();
            _localWindowSize = (uint)random.Next(1000, int.MaxValue);
            _localPacketSize = _localWindowSize - 1;
            _localChannelNumber = (uint)random.Next(0, int.MaxValue);
            _onRequestException = new SystemException();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _requestInfo = new SignalRequestInfo("ABC");

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _sessionMock.Setup(p => p.ConnectionInfo)
                .Returns(new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "password")));

            _sessionMock.Setup(p => p.NextChannelNumber).Returns(_localChannelNumber);

            _channel = new ChannelStub();
            _channel.Initialize(_sessionMock.Object, _localWindowSize, _localPacketSize);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.OnRequestException = _onRequestException;
        }

        private void Act()
        {
            _sessionMock.Raise(s => s.ChannelRequestReceived += null,
                new MessageEventArgs<ChannelRequestMessage>(new ChannelRequestMessage(_localChannelNumber, _requestInfo)));
        }

        [TestMethod]
        public void ExceptionEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelExceptionRegister.Count);
            Assert.AreSame(_onRequestException, _channelExceptionRegister[0].Exception);
        }

        [TestMethod]
        public void OnErrorOccuredShouldBeInvokedOnce()
        {
            Assert.AreEqual(1, _channel.OnErrorOccurredInvocations.Count);
            Assert.AreSame(_onRequestException, _channel.OnErrorOccurredInvocations[0]);
        }
    }
}
