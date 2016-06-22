using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortDynamicTest_Dispose_PortStarted_ChannelBound
    {
        private Mock<ISession> _sessionMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private Mock<IChannelDirectTcpip> _channelMock;
        private ForwardedPortDynamic _forwardedPort;
        private IList<EventArgs> _closingRegister;
        private IList<ExceptionEventArgs> _exceptionRegister;
        private IPEndPoint _endpoint;
        private Socket _client;
        private IPEndPoint _remoteEndpoint;
        private string _userName;
        private TimeSpan _expectedElapsedTime;
        private TimeSpan _elapsedTimeOfStop;

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
        }

        protected void Arrange()
        {
            var random = new Random();

            _closingRegister = new List<EventArgs>();
            _exceptionRegister = new List<ExceptionEventArgs>();
            _endpoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
            _expectedElapsedTime = TimeSpan.FromMilliseconds(random.Next(100, 500));
            _userName = random.Next().ToString(CultureInfo.InvariantCulture);
            _forwardedPort = new ForwardedPortDynamic(_endpoint.Address.ToString(), (uint)_endpoint.Port);

            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelDirectTcpip>(MockBehavior.Strict);

            Socket handlerSocket = null;

            _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(15));
            _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _sessionMock.Setup(p => p.CreateChannelDirectTcpip()).Returns(_channelMock.Object);
            _channelMock.Setup(p => p.Open(_remoteEndpoint.Address.ToString(), (uint)_remoteEndpoint.Port, _forwardedPort, It.IsAny<Socket>())).Callback<string, uint, IForwardedPort, Socket>((address, port, forwardedPort, socket) => handlerSocket = socket);
            _channelMock.Setup(p => p.IsOpen).Returns(true);
            _channelMock.Setup(p => p.Bind()).Callback(() =>
                {
                    Thread.Sleep(_expectedElapsedTime);
                    if (handlerSocket != null && handlerSocket.Connected)
                        handlerSocket.Shutdown(SocketShutdown.Both);
                });
            _channelMock.Setup(p => p.Close());
            _channelMock.Setup(p => p.Dispose());

            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;
            _forwardedPort.Start();

            _client = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = 500,
                    SendTimeout = 500,
                    SendBufferSize = 0
                };
            EstablishSocks4Connection(_client);
        }

        protected void Act()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            _forwardedPort.Dispose();

            stopwatch.Stop();
            _elapsedTimeOfStop = stopwatch.Elapsed;
        }

        [TestMethod]
        public void StopShouldBlockUntilBoundChannelHasClosed()
        {
            Assert.IsTrue(_elapsedTimeOfStop >= _expectedElapsedTime);
            Assert.IsTrue(_elapsedTimeOfStop < _expectedElapsedTime.Add(TimeSpan.FromMilliseconds(200)));
        }

        [TestMethod]
        public void IsStartedShouldReturnFalse()
        {
            Assert.IsFalse(_forwardedPort.IsStarted);
        }

        [TestMethod]
        public void ForwardedPortShouldRefuseNewConnections()
        {
            using (var client = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp))
            {
                try
                {
                    client.Connect(_endpoint);
                    Assert.Fail();
                }
                catch (SocketException ex)
                {
                    Assert.AreEqual(SocketError.ConnectionRefused, ex.SocketErrorCode);
                }
            }
        }

        [TestMethod]
        public void ExistingConnectionShouldBeClosed()
        {
            try
            {
                _client.Send(new byte[] { 0x0a }, 0, 1, SocketFlags.None);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                Assert.AreEqual(SocketError.ConnectionReset, ex.SocketErrorCode);
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
            _channelMock.Verify(
                p =>
                    p.Open(_remoteEndpoint.Address.ToString(), (uint) _remoteEndpoint.Port, _forwardedPort,
                        It.IsAny<Socket>()), Times.Once);
        }

        [TestMethod]
        public void BindOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Bind(), Times.Once);
        }

        [TestMethod]
        public void CloseOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Close(), Times.Once);
        }

        [TestMethod]
        public void DisposeOnChannelShouldBeInvokedOnce()
        {
            _channelMock.Verify(p => p.Dispose(), Times.Once);
        }

        private void EstablishSocks4Connection(Socket client)
        {
            var userNameBytes = Encoding.ASCII.GetBytes(_userName);
            var addressBytes = _remoteEndpoint.Address.GetAddressBytes();
            var portBytes = BitConverter.GetBytes((ushort)_remoteEndpoint.Port).Reverse().ToArray();

            _client.Connect(_endpoint);

            // send SOCKS version
            client.Send(new byte[] { 0x04 }, 0, 1, SocketFlags.None);
            // send command byte
            client.Send(new byte[] { 0x00 }, 0, 1, SocketFlags.None);
            // send port
            client.Send(portBytes, 0, portBytes.Length, SocketFlags.None);
            // send address
            client.Send(addressBytes, 0, addressBytes.Length, SocketFlags.None);
            // send user name
            client.Send(userNameBytes, 0, userNameBytes.Length, SocketFlags.None);
            // terminate user name with null
            client.Send(new byte[] { 0x00 }, 0, 1, SocketFlags.None);

            var buffer = new byte[16];
            client.Receive(buffer, 0, buffer.Length, SocketFlags.None);
        }
    }
}
