using System;
using System.Collections.Generic;
using System.Net;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortLocalTest_Dispose_PortDisposed
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private ForwardedPortLocal _forwardedPort;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _forwardedPort?.Dispose();
            _forwardedPort = null;
        }

        protected void Arrange()
        {
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

            var sequence = new MockSequence();
            _sessionMock.InSequence(sequence).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(sequence).Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _connectionInfoMock.InSequence(sequence).Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(30));

            _forwardedPort = new ForwardedPortLocal(IPAddress.Loopback.ToString(), "host", 22);
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;
            _forwardedPort.Start();
            _forwardedPort.Dispose();
        }

        protected void Act()
        {
            _forwardedPort.Dispose();
        }

        [TestMethod]
        public void ClosingShouldHaveFiredOnce()
        {
            Assert.HasCount(1, _closingRegister);
        }

        [TestMethod]
        public void ExceptionShouldNotHaveFired()
        {
            Assert.IsEmpty(_exceptionRegister);
        }

        [TestMethod]
        public void SessionShouldBeNull()
        {
            Assert.IsNull(_forwardedPort.Session);
        }
    }
}
