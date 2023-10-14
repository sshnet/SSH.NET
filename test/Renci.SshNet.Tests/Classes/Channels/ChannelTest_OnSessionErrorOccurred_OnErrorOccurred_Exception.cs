using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_OnSessionErrorOccurred_OnErrorOccurred_Exception : ChannelTestBase
    {
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _localChannelNumber;
        private ChannelStub _channel;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private Exception _onErrorOccurredException;
        private Exception _errorOccurredException;

        protected override void SetupData()
        {
            var random = new Random();

            _localWindowSize = (uint) random.Next(1000, int.MaxValue);
            _localPacketSize = _localWindowSize - 1;
            _localChannelNumber = (uint) random.Next(0, int.MaxValue);
            _onErrorOccurredException = new SystemException();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _errorOccurredException = new SystemException();
        }

        protected override void SetupMocks()
        {
        }

        protected override void Arrange()
        {
            base.Arrange();

            _channel = new ChannelStub(SessionMock.Object, _localChannelNumber, _localWindowSize, _localPacketSize);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.OnErrorOccurredException = _onErrorOccurredException;
        }

        protected override void Act()
        {
            SessionMock.Raise(s => s.ErrorOccured += null, new ExceptionEventArgs(_errorOccurredException));
        }

        [TestMethod]
        public void ExceptionEventShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _channelExceptionRegister.Count);
            Assert.AreSame(_onErrorOccurredException, _channelExceptionRegister[0].Exception);
        }

        [TestMethod]
        public void OnErrorOccuredShouldBeInvokedOnce()
        {
            Assert.AreEqual(1, _channel.OnErrorOccurredInvocations.Count);
            Assert.AreSame(_errorOccurredException, _channel.OnErrorOccurredInvocations[0]);
        }
    }
}