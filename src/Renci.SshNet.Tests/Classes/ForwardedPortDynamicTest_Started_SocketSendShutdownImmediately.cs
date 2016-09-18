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
    public class ForwardedPortDynamicTest_Started_SocketSendShutdownImmediately
    {
        private Mock<ISession> _sessionMock;
        private Mock<IChannelDirectTcpip> _channelMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private ForwardedPortDynamic _forwardedPort;
        private Socket _client;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private TimeSpan _connectionTimeout;
        private ManualResetEvent _channelDisposed;
        private IPEndPoint _forwardedPortEndPoint;

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
                if (_client.Connected)
                {
                    _client.Shutdown(SocketShutdown.Both);
                    _client.Close();
                    _client = null;
                }
            }

            if (_channelDisposed != null)
            {
                _channelDisposed.Dispose();
                _channelDisposed = null;
            }
        }

        private void SetupData()
        {
            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _connectionTimeout = TimeSpan.FromSeconds(5);
            _channelDisposed = new ManualResetEvent(false);
            _forwardedPortEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);

            _forwardedPort = new ForwardedPortDynamic((uint) _forwardedPortEndPoint.Port);
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;

            _client = new Socket(_forwardedPortEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
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
            _channelMock.InSequence(seq).Setup(p => p.Dispose()).Callback(() => _channelDisposed.Set());
        }

        private void Arrange()
        {
            CreateMocks();
            SetupData();
            SetupMocks();

            _forwardedPort.Start();

            _client.Connect(_forwardedPortEndPoint);
        }

        private void Act()
        {
            _client.Shutdown(SocketShutdown.Send);

            // wait for channel to be disposed
            _channelDisposed.WaitOne(TimeSpan.FromMilliseconds(200));
        }

        [TestMethod]
        public void SocketShouldNotBeConnected()
        {
            Assert.IsFalse(_client.Connected);
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
        public void ExceptionShouldNeverBeFired()
        {
            Assert.AreEqual(0, _exceptionRegister.Count, _exceptionRegister.AsString());
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
