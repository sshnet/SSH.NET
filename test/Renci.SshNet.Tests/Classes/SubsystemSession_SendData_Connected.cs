﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class SubsystemSession_SendData_Connected
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelSession> _channelMock;
        private string _subsystemName;
        private SubsystemSessionStub _subsystemSession;
        private int _operationTimeout;
        private IList<EventArgs> _disconnectedRegister;
        private IList<ExceptionEventArgs> _errorOccurredRegister;
        private byte[] _data;
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
            _data = new[] {(byte) random.Next(byte.MinValue, byte.MaxValue)};

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelSession>(MockBehavior.Strict);

            _sequence = new MockSequence();
            _sessionMock.InSequence(_sequence).Setup(p => p.CreateChannelSession()).Returns(_channelMock.Object);
            _channelMock.InSequence(_sequence).Setup(p => p.Open());
            _channelMock.InSequence(_sequence).Setup(p => p.SendSubsystemRequest(_subsystemName)).Returns(true);
            _channelMock.InSequence(_sequence).Setup(p => p.IsOpen).Returns(true);
            _channelMock.InSequence(_sequence).Setup(p => p.SendData(_data));

            _subsystemSession = new SubsystemSessionStub(
                _sessionMock.Object,
                _subsystemName,
                _operationTimeout);
            _subsystemSession.Disconnected += (sender, args) => _disconnectedRegister.Add(args);
            _subsystemSession.ErrorOccurred += (sender, args) => _errorOccurredRegister.Add(args);
            _subsystemSession.Connect();
        }

        protected void Act()
        {
            _subsystemSession.SendData(_data);
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
        public void SendDataOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.SendData(_data), Times.Once);
        }

        [TestMethod]
        public void IsOpenShouldReturnTrueWhenChannelIsOpen()
        {
            _channelMock.InSequence(_sequence).Setup(p => p.IsOpen).Returns(true);

            Assert.IsTrue(_subsystemSession.IsOpen);

            _channelMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }

        [TestMethod]
        public void IsOpenShouldReturnFalseWhenChannelIsNotOpen()
        {
            _channelMock.InSequence(_sequence).Setup(p => p.IsOpen).Returns(false);

            Assert.IsFalse(_subsystemSession.IsOpen);

            _channelMock.Verify(p => p.IsOpen, Times.Exactly(2));
        }
    }
}
