using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SubsystemSession_OnChannelDataReceived_OnDataReceived_Exception
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelMock;
        private string _subsystemName;
        private SubsystemSessionStub _subsystemSession;
        private int _operationTimeout;
        private IList<EventArgs> _disconnectedRegister;
        private IList<ExceptionEventArgs> _errorOccurredRegister;
        private ChannelDataEventArgs _channelDataEventArgs;
        private Exception _onDataReceivedException;
            
        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        protected void Arrange()
        {
            var random = new Random();
            _subsystemName = random.Next().ToString(CultureInfo.InvariantCulture);
            _operationTimeout = 30000;
            _disconnectedRegister = new List<EventArgs>();
            _errorOccurredRegister = new List<ExceptionEventArgs>();
            _channelDataEventArgs = new ChannelDataEventArgs(
                (uint) random.Next(0, int.MaxValue),
                new [] {(byte) random.Next(byte.MinValue, byte.MaxValue)});
            _onDataReceivedException = new SystemException();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelSession>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _sessionMock.InSequence(sequence).Setup(p => p.CreateChannelSession()).Returns(_channelMock.Object);
            _channelMock.InSequence(sequence).Setup(p => p.Open());
            _channelMock.InSequence(sequence).Setup(p => p.SendSubsystemRequest(_subsystemName)).Returns(true);

            _subsystemSession = new SubsystemSessionStub(
                _sessionMock.Object,
                _subsystemName,
                _operationTimeout);
            _subsystemSession.Disconnected += (sender, args) => _disconnectedRegister.Add(args);
            _subsystemSession.ErrorOccurred += (sender, args) => _errorOccurredRegister.Add(args);
            _subsystemSession.OnDataReceivedException = _onDataReceivedException;
            _subsystemSession.Connect();
        }

        protected void Act()
        {
            _channelMock.Raise(s => s.DataReceived += null, _channelDataEventArgs);
        }

        [TestMethod]
        public void DisconnectHasNeverFired()
        {
            Assert.AreEqual(0, _disconnectedRegister.Count);
        }

        [TestMethod]
        public void ErrorOccurredHaveFiredOnce()
        {
            Assert.AreEqual(1, _errorOccurredRegister.Count, _errorOccurredRegister.AsString());
            Assert.AreSame(_onDataReceivedException, _errorOccurredRegister[0].Exception, _errorOccurredRegister.AsString());
        }

        [TestMethod]
        public void OnDataReceivedShouldBeInvokedOnce()
        {
            Assert.AreEqual(1, _subsystemSession.OnDataReceivedInvocations.Count);

            var received = _subsystemSession.OnDataReceivedInvocations[0];
            Assert.AreEqual(_channelDataEventArgs.Data, received.Data);
        }
    }
}
