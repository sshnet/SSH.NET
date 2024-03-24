using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;
using Renci.SshNet.Messages;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_OnSessionChannelRequestReceived_HandleUnknownMessage : ChannelTestBase
    {
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _localChannelNumber;
        private ChannelStub _channel;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private UnknownRequestInfo _requestInfo;

        protected override void SetupData()
        {
            var random = new Random();

            _localWindowSize = (uint) random.Next(1000, int.MaxValue);
            _localPacketSize = _localWindowSize - 1;
            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _requestInfo = new UnknownRequestInfo();
        }

        protected override void SetupMocks()
        {
            _ = SessionMock.Setup(p => p.ConnectionInfo)
                           .Returns(new ConnectionInfo("host", "user", new PasswordAuthenticationMethod("user", "password")));
            _ = SessionMock.Setup(p => p.SendMessage(It.IsAny<Message>()));
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.SetIsOpen(true);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
        }

        protected override void Act()
        {
            SessionMock.Raise(s => s.ChannelRequestReceived += null,
                               new MessageEventArgs<ChannelRequestMessage>(new ChannelRequestMessage(_localChannelNumber, _requestInfo)));
        }

        [TestMethod]
        public void FailureMessageWasSent()
        {
            SessionMock.Verify(p => p.SendMessage(It.Is<ChannelFailureMessage>(m => m.LocalChannelNumber == _localChannelNumber)), Times.Once);
        }

        [TestMethod]
        public void NoExceptionShouldHaveFired()
        {
            Assert.AreEqual(0, _channelExceptionRegister.Count);
        }
    }

    internal class UnknownRequestInfo : RequestInfo
    {
        public override string RequestName
        {
            get
            {
                return nameof(UnknownRequestInfo);
            }
        }

    }
}
