using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortDynamicTest_Started_SocketVersionNotSupported
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelDirectTcpip> _channelMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private ForwardedPortDynamic _forwardedPort;
        private Socket _client;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private ManualResetEvent _exceptionFired;
        private TimeSpan _connectionTimeout;

        [TestInitialize]
        public void Initialize()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_forwardedPort != null && _forwardedPort.IsStarted)
            {
                _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
                _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(5));
                _forwardedPort.Stop();
            }

            if (_client != null)
            {
                _client.Close();
                _client = null;
            }

            if (_exceptionFired != null)
            {
                _exceptionFired.Dispose();
                _exceptionFired = null;
            }
        }

        private void SetupData()
        {
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _exceptionFired = new ManualResetEvent(false);
            _connectionTimeout = TimeSpan.FromSeconds(5);
        }

        private void CreateMocks()
        {
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelDirectTcpip>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
        }

        private void SetupMocks()
        {
            var seq = new MockSequence();

            _sessionMock.InSequence(seq).Setup(p => p.IsConnected).Returns(true);
            _sessionMock.InSequence(seq).Setup(p => p.CreateChannelDirectTcpip()).Returns(_channelMock.Object);
            _sessionMock.InSequence(seq).Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _connectionInfoMock.InSequence(seq).Setup(p => p.Timeout).Returns(_connectionTimeout);
            _channelMock.InSequence(seq).Setup(p => p.Dispose());
        }

        private void Arrange()
        {
            SetupData();
            CreateMocks();
            SetupMocks();

            _forwardedPort = new ForwardedPortDynamic(8122);
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) =>
                {
                    _exceptionRegister.Add(args);
                    _exceptionFired.Set();
                };
            _forwardedPort.Session = _sessionMock.Object;
            _forwardedPort.Start();

            var endPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            _client = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _client.Connect(endPoint);
        }

        private void Act()
        {
            var buffer = new byte[] {0x07};
            _client.Send(buffer, 0, buffer.Length, SocketFlags.None);

            // wait for Exception event to be fired as a way to ensure that SOCKS
            // request has been handled completely
            _exceptionFired.WaitOne(TimeSpan.FromMilliseconds(200));
        }

        [TestMethod]
        public void SocketShouldBeConnected()
        {
            Assert.IsTrue(_client.Connected);
        }

        [TestMethod]
        public void ForwardedPortShouldShutdownSendOnSocket()
        {
            var buffer = new byte[1];

            var bytesReceived = _client.Receive(buffer, 0, buffer.Length, SocketFlags.None);

            Assert.AreEqual(0, bytesReceived);
        }

        [TestMethod]
        public void ClosingShouldNotHaveFired()
        {
            Assert.AreEqual(0, _closingRegister.Count);
        }

        [TestMethod]
        public void ExceptionShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _exceptionRegister.Count, _exceptionRegister.AsString());

            var exception = _exceptionRegister[0].Exception;
            Assert.IsNotNull(exception);
            var notSupportedException = exception as NotSupportedException;
            Assert.IsNotNull(notSupportedException, exception.ToString());
            Assert.AreEqual("SOCKS version 7 is not supported.", notSupportedException.Message);
        }

        [TestMethod]
        public void CreateChannelDirectTcpipOnSessionShouldBeInvokedOnce()
        {
            _sessionMock.Verify(p => p.CreateChannelDirectTcpip(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}
