using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes.Channels
{
    [TestClass]
    public class ChannelTest_OnSessionErrorOccurred_OnErrorOccurred_Exception
    {
        private Mock<ISession> _sessionMock;
        private uint _localWindowSize;
        private uint _localPacketSize;
        private uint _localChannelNumber;
        private ChannelStub _channel;
        private IList<ExceptionEventArgs> _channelExceptionRegister;
        private Exception _onErrorOccurredException;
        private Exception _errorOccurredException;

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
            _onErrorOccurredException = new SystemException();
            _channelExceptionRegister = new List<ExceptionEventArgs>();
            _errorOccurredException = new SystemException();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);

            _sessionMock.Setup(p => p.NextChannelNumber).Returns(_localChannelNumber);

            _channel = new ChannelStub();
            _channel.Initialize(_sessionMock.Object, _localWindowSize, _localPacketSize);
            _channel.Exception += (sender, args) => _channelExceptionRegister.Add(args);
            _channel.OnErrorOccurredException = _onErrorOccurredException;
        }

        private void Act()
        {
            _sessionMock.Raise(s => s.ErrorOccured += null, new ExceptionEventArgs(_errorOccurredException));
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
