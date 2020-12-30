using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class Socks4ConnectorTest_Connect_ConnectionSucceeded : Socks4ConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private Socket _clientSocket;
        private AsyncSocketListener _proxyServer;
        private List<byte> _bytesReceivedByProxy;
        private bool _disconnected;
        private Socket _actual;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();

            _connectionInfo = CreateConnectionInfo("proxyUser", "proxyPwd");
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(random.Next(50, 200));
            _bytesReceivedByProxy = new List<byte>();
            _clientSocket = SocketFactory.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _actual = null;

            _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.ProxyPort));
            _proxyServer.Disconnected += socket => _disconnected = true;
            _proxyServer.BytesReceived += (bytesReceived, socket) =>
                {
                    _bytesReceivedByProxy.AddRange(bytesReceived);

                    if (_bytesReceivedByProxy.Count == bytesReceived.Length)
                    {
                        // Send SOCKS response
                        socket.Send(new byte[]
                            {
                                // Reply version (null byte)
                                0x00,
                                // Request granted
                                0x5a,
                                // Destination address port
                                0x01,
                                0xf0,
                                // Destination address IP
                                0x01,
                                0x02,
                                0x03,
                                0x04
                            });

                        // Send extra byte to allow us to verify that connector did not consume too much
                        socket.Send(new byte[]
                            {
                                0xfe
                            });
                    }
                };
            _proxyServer.Start();
        }

        protected override void SetupMocks()
        {
            SocketFactoryMock.Setup(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                             .Returns(_clientSocket);
        }

        protected override void TearDown()
        {
            base.TearDown();

            if (_proxyServer != null)
            {
                _proxyServer.Dispose();
            }

            if (_clientSocket != null)
            {
                _clientSocket.Dispose();
            }
        }

        protected override void Act()
        {
            _actual = Connector.Connect(_connectionInfo);
        }

        [TestMethod]
        public void ProxyShouldHaveReceivedExpectedSocksRequest()
        {
            var expectedSocksRequest = new byte[]
                {
                    // SOCKS version
                    0x04,
                    // CONNECT request
                    0x01,
                    // Destination port
                    0x03,
                    0x09,
                    // Destination address (IPv4)
                    0x7f,
                    0x00,
                    0x00,
                    0x01,
                    // Proxy user
                    0x70,
                    0x72,
                    0x6f,
                    0x78,
                    0x79,
                    0x55,
                    0x73,
                    0x65,
                    0x72,
                    // Null terminator
                    0x00
                };

            var errorText = string.Format("Expected:{0}{1}{0}but was:{0}{2}",
                                          Environment.NewLine,
                                          PacketDump.Create(expectedSocksRequest, 2),
                                          PacketDump.Create(_bytesReceivedByProxy, 2));

            Assert.IsTrue(expectedSocksRequest.SequenceEqual(_bytesReceivedByProxy), errorText);
        }

        [TestMethod]
        public void ConnectShouldReturnSocketCreatedUsingSocketFactory()
        {
            Assert.IsNotNull(_actual);
            Assert.AreSame(_clientSocket, _actual);
        }

        [TestMethod]
        public void OnlySocksResponseShouldHaveBeenConsumed()
        {
            var buffer = new byte[2];

            Assert.AreEqual(1, _actual.Receive(buffer));
            Assert.AreEqual(0xfe, buffer[0]);
        }

        [TestMethod]
        public void ClientSocketShouldBeConnected()
        {
            Assert.IsFalse(_disconnected);
            Assert.IsTrue(_actual.Connected);
        }

        [TestMethod]
        public void CreateOnSocketFactoryShouldHaveBeenInvokedOnce()
        {
            SocketFactoryMock.Verify(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                                     Times.Once());
        }
    }
}
