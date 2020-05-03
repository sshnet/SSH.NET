using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_OnSessionChannelFailureReceived_OnFailure_Exception : ChannelTestBase
    {
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _localChannelNumber;
        private ChannelStub _channel;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private Exception _onFailureException;

        protected override void SetupData()
        {
            var random = new Random();

            _localWindowSize = (uint) random.Next(0, 1000);
            _localPacketSize = (uint) random.Next(1001, int.MaxValue);
            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _onFailureException = new SystemException();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
        }

        protected override void SetupMocks()
        {
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.OnFailureException = _onFailureException;
        }

        protected override void Act()
        {
            SessionMock.Raise(s => s.ChannelFailureReceived += null,
                               new MessageEventArgs<ChannelFailureMessage>(new ChannelFailureMessage(_localChannelNumber)));
        }

        [TestMethod]
        public void ExceptionEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelExceptionRegister.Count);
            Assert.AreSame(_onFailureException, _channelExceptionRegister[0].Exception);
        }

        [TestMethod]
        public void OnErrorOccuredShouldBeInvokedOnce()
        {
            Assert.AreEqual(1, _channel.OnErrorOccurredInvocations.Count);
            Assert.AreSame(_onFailureException, _channel.OnErrorOccurredInvocations[0]);
        }
    }
}