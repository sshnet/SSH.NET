using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Abstractions;
using Renci.SshNet.Channels;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ForwardedPortDynamicTest_Stop_PortStarted_ChannelBound
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
        private TimeSpan _bindSleepTime;
        private ManualResetEvent _channelBindStarted;
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
            if (_channelBindStarted != null)
            {
                _channelBindStarted.Dispose();
                _channelBindStarted = null;
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
            _endpoint = new IPEndPoint(IPAddress.Loopback, 8122);
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), random.Next(IPEndPoint.MinPort, IPEndPoint.MaxPort));
            _bindSleepTime = TimeSpan.FromMilliseconds(random.Next(100, 500));
            _userName = random.Next().ToString(CultureInfo.InvariantCulture);
            _channelBindStarted = new ManualResetEvent(false);
            _channelBindCompleted = new ManualResetEvent(false);

            _forwardedPort = new ForwardedPortDynamic(_endpoint.Address.ToString(), (uint)_endpoint.Port);
            _forwardedPort.Closing += (sender, args) => _closingRegister.Add(args);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;

            _client = new Socket(_endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
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
            _channelMock.Setup(p => p.Open(_remoteEndpoint.Address.ToString(), (uint)_remoteEndpoint.Port, _forwardedPort, It.IsAny<Socket>()));
            _channelMock.Setup(p => p.IsOpen).Returns(true);
            _channelMock.Setup(p => p.Bind()).Callback(() =>
                {
                    _channelBindStarted.Set();
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
            EstablishSocks4Connection(_client);
            // wait until SOCKS client is bound to channel
            Assert.IsTrue(_channelBindStarted.WaitOne(TimeSpan.FromMilliseconds(200)));
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

            var buffer = new byte[8];
            var bytesRead = SocketAbstraction.Read(client, buffer, 0, buffer.Length, TimeSpan.FromMilliseconds(500));
            Assert.AreEqual(buffer.Length, bytesRead);
        }
    }
}
