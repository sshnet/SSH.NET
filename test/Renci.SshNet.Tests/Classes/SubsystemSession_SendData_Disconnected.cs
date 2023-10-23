using System;
using System.Collections.Generic;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SubsystemSession_SendData_Disconnected
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelMock;
        private string _subsystemName;
        private SubsystemSessionStub _subsystemSession;
        private int _operationTimeout;
        private List<EventArgs> _disconnectedRegister;
        private List<ExceptionEventArgs> _errorOccurredRegister;
        private byte[] _data;
        private MockSequence _sequence;
        private InvalidOperationException _actualException;

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
            _data = new[] { (byte) random.Next(byte.MinValue, byte.MaxValue) };

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelSession>(MockBehavior.Strict);

            _sequence = new MockSequence();
            _sessionMock.InSequence(_sequence).Setup(p => p.CreateChannelSession()).Returns(_channelMock.Object);
            _channelMock.InSequence(_sequence).Setup(p => p.Open());
            _channelMock.InSequence(_sequence).Setup(p => p.SendSubsystemRequest(_subsystemName)).Returns(true);
            _channelMock.InSequence(_sequence).Setup(p => p.Dispose());
            _channelMock.InSequence(_sequence).Setup(p => p.SendData(_data));

            _subsystemSession = new SubsystemSessionStub(
                _sessionMock.Object,
                _subsystemName,
                _operationTimeout);
            _subsystemSession.Disconnected += (sender, args) => _disconnectedRegister.Add(args);
            _subsystemSession.ErrorOccurred += (sender, args) => _errorOccurredRegister.Add(args);
            _subsystemSession.Connect();
            _subsystemSession.Disconnect();
        }

        protected void Act()
        {
            try
            {
                _subsystemSession.SendData(_data);
                Assert.Fail();
            }
            catch (InvalidOperationException ex)
            {
                _actualException = ex;
            }
        }

        public void SendShouldHaveThrownAnInvalidOperationException()
        {
            Assert.IsNotNull(_actualException);
            Assert.AreEqual(typeof(InvalidOperationException), _actualException.GetType());
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("The session is not open.", _actualException.Message);
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
        public void SendDataOnChannelShouldNeverBeInvoked()
        {
            _channelMock.Verify(p => p.SendData(_data), Times.Never);
        }

        [TestMethod]
        public void DisposeOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Dispose(), Times.Once);
        }

        [TestMethod]
        public void IsOpenShouldReturnFalse()
        {
            Assert.IsFalse(_subsystemSession.IsOpen);

            // IsOpen should not have been invoked on the channel
            _channelMock.Verify(p => p.IsOpen, Times.Never);
        }
    }
}
