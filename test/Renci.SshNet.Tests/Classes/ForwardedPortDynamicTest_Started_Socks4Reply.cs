using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Channels;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// A SOCKS4 reply is a fixed-width eight byte record - version, code, destination port,
    /// destination address - whether or not the request was granted. The last six fields are defined
    /// as ignored by the client, but reading them is not optional: a client that reads the record as
    /// a record, which SSH.NET's own <see cref="Renci.SshNet.Connection.Socks4Connector"/> does on the
    /// granted path, blocks waiting for bytes that never come if a rejection is short.
    /// </summary>
    [TestClass]
    public class ForwardedPortDynamicTest_Started_Socks4Reply
    {
        private const string UserName = "socks-user";

        private Mock<ISession> _sessionMock;
        private Mock<IChannelDirectTcpip> _channelMock;
        private Mock<IConnectionInfo> _connectionInfoMock;
        private ForwardedPortDynamic _forwardedPort;
        private Socket _client;
        private IPEndPoint _remoteEndpoint;
        private IList<ExceptionEventArgs> _exceptionRegister;

        [TestCleanup]
        public void Cleanup()
        {
            if (_forwardedPort is not null && _forwardedPort.IsStarted)
            {
                _forwardedPort.Stop();
            }

            _client?.Dispose();
            _client = null;
            _forwardedPort?.Dispose();
            _forwardedPort = null;
        }

        /// <summary>
        /// The case that was short: the channel could not be opened, so the request is rejected.
        /// </summary>
        [TestMethod]
        public void RejectedRequestShouldBeAnsweredWithAFullEightByteReply()
        {
            Arrange(channelOpen: false);

            var reply = RequestConnect();

            Assert.AreEqual(8, reply.Length, "a SOCKS4 reply is eight bytes, granted or not");
            Assert.AreEqual(0x00, reply[0], "reply version");
            Assert.AreEqual(0x5b, reply[1], "request rejected or failed");
            AssertEchoesTheRequest(reply);
        }

        /// <summary>
        /// And the granted case still is what it was.
        /// </summary>
        [TestMethod]
        public void GrantedRequestShouldBeAnsweredWithAFullEightByteReply()
        {
            Arrange(channelOpen: true);

            var reply = RequestConnect();

            Assert.AreEqual(8, reply.Length);
            Assert.AreEqual(0x00, reply[0], "reply version");
            Assert.AreEqual(0x5a, reply[1], "request granted");
            AssertEchoesTheRequest(reply);
        }

        private void AssertEchoesTheRequest(byte[] reply)
        {
            var address = _remoteEndpoint.Address.GetAddressBytes();

            Assert.AreEqual((byte)(_remoteEndpoint.Port >> 8), reply[2], "destination port, high byte");
            Assert.AreEqual((byte)(_remoteEndpoint.Port & 0xFF), reply[3], "destination port, low byte");
            CollectionAssert.AreEqual(address, new[] { reply[4], reply[5], reply[6], reply[7] }, "destination address");
        }

        private void Arrange(bool channelOpen)
        {
            _exceptionRegister = new List<ExceptionEventArgs>();
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse("193.168.1.5"), 8122);

            _sessionMock = new Mock<ISession>(MockBehavior.Strict);
            _channelMock = new Mock<IChannelDirectTcpip>(MockBehavior.Strict);
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);

            _ = _sessionMock.Setup(p => p.SessionLoggerFactory).Returns(NullLoggerFactory.Instance);
            _ = _sessionMock.Setup(p => p.IsConnected).Returns(true);
            _ = _sessionMock.Setup(p => p.ConnectionInfo).Returns(_connectionInfoMock.Object);
            _ = _sessionMock.Setup(p => p.CreateChannelDirectTcpip()).Returns(_channelMock.Object);
            _ = _connectionInfoMock.Setup(p => p.Timeout).Returns(TimeSpan.FromSeconds(5));
            _ = _channelMock.Setup(p => p.Open(_remoteEndpoint.Address.ToString(),
                                               (uint)_remoteEndpoint.Port,
                                               It.IsAny<IForwardedPort>(),
                                               It.IsAny<Socket>()));
            _ = _channelMock.Setup(p => p.IsOpen).Returns(channelOpen);
            _ = _channelMock.Setup(p => p.Bind());
            _ = _channelMock.Setup(p => p.Dispose());

            // Bind to an ephemeral port rather than a fixed one, so that the test does not collide
            // with anything else that happens to be listening.
            _forwardedPort = new ForwardedPortDynamic("127.0.0.1", 0);
            _forwardedPort.Exception += (sender, args) => _exceptionRegister.Add(args);
            _forwardedPort.Session = _sessionMock.Object;
            _forwardedPort.Start();

            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = 10000
            };
            _client.Connect(new IPEndPoint(IPAddress.Loopback, (int)_forwardedPort.BoundPort));
        }

        /// <summary>
        /// Sends a SOCKS4 CONNECT and reads back exactly what the protocol says the reply is, without
        /// giving up early. A reply that is short shows up here as a receive timeout, which is what a
        /// real client would see too.
        /// </summary>
        private byte[] RequestConnect()
        {
            var user = Encoding.ASCII.GetBytes(UserName);
            var address = _remoteEndpoint.Address.GetAddressBytes();
            var request = new byte[8 + user.Length + 1];

            request[0] = 0x04; // SOCKS version
            request[1] = 0x01; // CONNECT
            request[2] = (byte)(_remoteEndpoint.Port >> 8);
            request[3] = (byte)(_remoteEndpoint.Port & 0xFF);
            Buffer.BlockCopy(address, 0, request, 4, address.Length);
            Buffer.BlockCopy(user, 0, request, 8, user.Length);
            request[request.Length - 1] = 0x00;

            _ = _client.Send(request, 0, request.Length, SocketFlags.None);

            var reply = new byte[8];
            var received = 0;

            try
            {
                while (received < reply.Length)
                {
                    var bytes = _client.Receive(reply, received, reply.Length - received, SocketFlags.None);
                    if (bytes == 0)
                    {
                        break;
                    }

                    received += bytes;
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
            {
                Assert.Fail(string.Format(CultureInfo.InvariantCulture,
                                          "timed out after {0} of {1} reply bytes: {2}",
                                          received,
                                          reply.Length,
                                          BitConverter.ToString(reply, 0, received)));
            }

            Assert.AreEqual(0, _exceptionRegister.Count, _exceptionRegister.AsString());

            var actual = new byte[received];
            Buffer.BlockCopy(reply, 0, actual, 0, received);
            return actual;
        }
    }
}
