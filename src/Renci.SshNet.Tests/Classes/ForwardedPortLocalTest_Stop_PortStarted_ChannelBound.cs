using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortLocalTest_Stop_PortStarted_ChannelBound
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private Mock<IChannelDirectTcpip> _channelMock;
        private ForwardedPortLocal _forwardedPort;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private IPEndPoint _localEndpoint;
        private IPEndPoint _remoteEndpoint;
        private Socket _client;
        private TimeSpan _bindSleepTime;
        private ManualResetEvent _channelBound;
        private ManualResetEvent _channelBindCompleted;

        [TestInitialize]
        public void Setup()
        {
            Arrange();
            Act();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (_client != null)
            {
                _client.Dispose();
                _client = null;
            }
            if (_forwardedPort != null)
            {
                _forwardedPort.Dispose();
                _forwardedPort = null;
            }
            if (_channelBound != null)
            {
                _channelBound.Dispose();
                _channelBound = null;
            }
            if (_channelBindCompleted != null)
            {
                _channelBindCompleted.Dispose();
                _channelBindCompleted = null;
            }
        }

        private void CreateMocks()
        {
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelDirectTcpip>(MockBehavior.Strict);
        }

        private void SetupData()
        {
            var random = new Random();

            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _localEndpoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
            _bindSleepTime = TimeSpan.FromMilliseconds(random.Next(100, 500));
            _channelBound = new ManualResetEvent(false);
            _channelBindCompleted = new ManualResetEvent(false);

            _forwardedPort = new ForwardedPortLocal(_localEndpoint.Address.ToString(), (uint) _localEndpoint.Port, _remoteEndpoint.Address.ToString(), (uint) _remoteEndpoint.Port);
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;

            _client = new Socket(_localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = 100,
                    SendTimeout = 100,
                    SendBufferSize = 0
                };
        }

        private void SetupMocks()
        {
            _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(15));
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _sessionMock.Setup(p => p.CreateChannelDirectTcpip()).Returns(_channelMock.Object);
            _channelMock.Setup(p => p.Open(_forwardedPort.Host, _forwardedPort.Port, _forwardedPort, It.IsAny<Socket>()));
            _channelMock.Setup(p => p.Bind()).Callback(() =>
                {
                    _channelBound.Set();
                    Thread.Sleep(_bindSleepTime);
                    _channelBindCompleted.Set();
                });
            _channelMock.Setup(p => p.Dispose());
        }

        protected void Arrange()
        {
            CreateMocks();
            SetupData();
            SetupMocks();

            // start port
            _forwardedPort.Start();
            // connect to port
            _client.Connect(_localEndpoint);
            // wait for SOCKS client to bind to channel
            Assert.IsTrue(_channelBound.WaitOne(TimeSpan.FromMilliseconds(200)));
        }

        protected void Act()
        {
            _forwardedPort.Stop();
        }

        [TestMethod]
        public void ShouldBlockUntilBindHasCompleted()
        {
            Assert.IsTrue(_channelBindCompleted.WaitOne(0));
        }

        [TestMethod]
        public void IsStartedShouldReturnFalse()
        {
            Assert.IsFalse(_forwardedPort.IsStarted);
        }

        [TestMethod]
        public void ForwardedPortShouldRefuseNewConnections()
        {
            using (var client = new Socket(_localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    client.Connect(_localEndpoint);
                    Assert.Fail();
                }
                catch (SocketException ex)
                {
                    Assert.AreEqual(SocketError.ConnectionRefused, ex.SocketErrorCode);
                }
            }
        }

        [TestMethod]
        public void BoundClientShouldNotBeClosed()
        {
            // the forwarded port itself does not close the client connection; when the channel is closed properly
            // it's the channel that will take care of closing the client connection
            //
            // we'll check if the client connection is still alive by attempting to receive, which should time out
            // as the forwarded port (or its channel) are not sending anything

            var buffer = new byte[1];

            try
            {
                _client.Receive(buffer);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(SocketError.TimedOut, ex.SocketErrorCode);
            }
        }

        [TestMethod]
        public void ClosingShouldHaveFiredOnce()
        {
            Assert.AreEqual(1, _closingRegister.Count);
        }

        [TestMethod]
        public void ExceptionShouldNotHaveFired()
        {
            Assert.AreEqual(0, _exceptionRegister.Count);
        }

        [TestMethod]
        public void OpenOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Open(_forwardedPort.Host, _forwardedPort.Port, _forwardedPort, It.IsAny<Socket>()), Times.Once);
        }

        [TestMethod]
        public void BindOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Bind(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}
