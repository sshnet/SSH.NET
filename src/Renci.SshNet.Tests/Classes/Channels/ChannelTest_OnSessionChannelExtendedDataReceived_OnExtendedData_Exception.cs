using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Messages.Connection;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_OnSessionChannelExtendedDataReceived_OnExtendedData_Exception : ChannelTestBase
    {
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _localChannelNumber;
        private ChannelStub _channel;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private Exception _onExtendedDataException;

        protected override void SetupData()
        {
            var random = new Random();

            _localWindowSize = (uint) random.Next(1000, int.MaxValue);
            _localPacketSize = _localWindowSize - 1;
            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _onExtendedDataException = new SystemException();
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
            _channel.OnExtendedDataException = _onExtendedDataException;
        }

        protected override void Act()
        {
            SessionMock.Raise(s => s.ChannelExtendedDataReceived += null,
                               new MessageEventArgs<ChannelExtendedDataMessage>(new ChannelExtendedDataMessage(_localChannelNumber, 5, new byte[0])));
        }

        [TestMethod]
        public void ExceptionEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelExceptionRegister.Count);
            Assert.AreSame(_onExtendedDataException, _channelExceptionRegister[0].Exception);
        }

        [TestMethod]
        public void OnErrorOccuredShouldBeInvokedOnce()
        {
            Assert.AreEqual(1, _channel.OnErrorOccurredInvocations.Count);
            Assert.AreSame(_onExtendedDataException, _channel.OnErrorOccurredInvocations[0]);
        }
    }
}