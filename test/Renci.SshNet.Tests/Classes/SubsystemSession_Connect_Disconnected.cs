using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SubsystemSession_Connect_Disconnected
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelBeforeDisconnectMock;
        private Mock<IChannelSession> _channelAfterDisconnectMock;
        private string _subsystemName;
        private SubsystemSessionStub _subsystemSession;
        private int _operationTimeout;
        private IList<EventArgs> _disconnectedRegister;
        private IList<ExceptionEventArgs> _errorOccurredRegister;
        private MockSequence _sequence;

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

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
            _channelBeforeDisconnectMock = new Mock<IChannelSession>(MockBehavior.Strict);
            _channelAfterDisconnectMock = new Mock<IChannelSession>(MockBehavior.Strict);

            _sequence = new MockSequence();

            _ = _sessionMock.InSequence(_sequence)
                            .Setup(p => p.CreateChannelSession())
                            .Returns(_channelBeforeDisconnectMock.Object);
            _ = _channelBeforeDisconnectMock.InSequence(_sequence)
                                            .Setup(p => p.Open());
            _ = _channelBeforeDisconnectMock.InSequence(_sequence)
                                            .Setup(p => p.SendSubsystemRequest(_subsystemName))
                                            .Returns(true);
            _ = _channelBeforeDisconnectMock.InSequence(_sequence)
                                            .Setup(p => p.Dispose());
            _ = _sessionMock.InSequence(_sequence)
                            .Setup(p => p.CreateChannelSession())
                            .Returns(_channelAfterDisconnectMock.Object);
            _ = _channelAfterDisconnectMock.InSequence(_sequence)
                                           .Setup(p => p.Open());
            _ = _channelAfterDisconnectMock.InSequence(_sequence)
                                           .Setup(p => p.SendSubsystemRequest(_subsystemName))
                                           .Returns(true);

            _subsystemSession = new SubsystemSessionStub(_sessionMock.Object,
                                                         _subsystemName,
                                                         _operationTimeout);
            _subsystemSession.Disconnected += (sender, args) => _disconnectedRegister.Add(args);
            _subsystemSession.ErrorOccurred += (sender, args) => _errorOccurredRegister.Add(args);
            _subsystemSession.Connect();
            _subsystemSession.Disconnect();
        }

        protected void Act()
        {
            _subsystemSession.Connect();
        }

        [TestMethod]
        public void DisconnectHasNeverFired()
        {
            Assert.AreEqual(0, _disconnectedRegister.Count);
        }

        [TestMethod]
        public void ErrorOccurredHasNeverFired()
        {
            Assert.AreEqual(0, _errorOccurredRegister.Count);
        }

        [TestMethod]
        public void IsOpenShouldReturnTrueWhenChannelIsOpen()
        {
            _ = _channelAfterDisconnectMock.InSequence(_sequence)
                                           .Setup(p => p.IsOpen)
                                           .Returns(true);

            Assert.IsTrue(_subsystemSession.IsOpen);

            _channelAfterDisconnectMock.Verify(p => p.IsOpen, Times.Once);
        }

        [TestMethod]
        public void IsOpenShouldReturnFalseWhenChannelIsNotOpen()
        {
            _ = _channelAfterDisconnectMock.InSequence(_sequence)
                                           .Setup(p => p.IsOpen)
                                           .Returns(false);

            Assert.IsFalse(_subsystemSession.IsOpen);

            _channelAfterDisconnectMock.Verify(p => p.IsOpen, Times.Once);
        }

        [TestMethod]
        public void DisposeOnChannelBeforeDisconnectShouldBeInvokedOnce()
        {
            _channelBeforeDisconnectMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnChannelAfterDisconnectShouldNeverBeInvoked()
        {
            _channelAfterDisconnectMock.Verify(p => p.Dispose(), Times.Never);
        }
    }
}
