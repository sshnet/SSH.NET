using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class Socks5Connector_Connect_NoAuthentication_Succeed : Socks5ConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private AsyncSocketListener _proxyServer;
        private Socket _clientSocket;
        private List<byte> _bytesReceivedByProxy;
        private Socket _actual;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = CreateConnectionInfo(new string('a', 255), new string('b', 255));
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(100);
            _bytesReceivedByProxy = new List<byte>();

            _clientSocket = SocketFactory.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.ProxyPort));
            _proxyServer.BytesReceived += (bytesReceived, socket) =>
            {
                _bytesReceivedByProxy.AddRange(bytesReceived);

                if (_bytesReceivedByProxy.Count == 4)
                {
                    // We received the greeting

                    socket.Send(new byte[]
                        {
                                    // SOCKS version
                                    0x05,
                                    // Require no authentication
                                    0x00
                        });
                }
                else if (_bytesReceivedByProxy.Count == 4 + (1 + 1 + 1 + 1 + 4 + 2))
                {
                    // We received the connection request

                    socket.Send(new byte[]
                        {
                                    // SOCKS version
                                    0x05,
                                    // Connection successful
                                    0x00,
                                    // Reserved byte
                                    0x00,
                        });

                    // Send server bound address
                    socket.Send(new byte[]
                        {
                                    // IPv6
                                    0x04,
                                    // IP address
                                    0x01,
                                    0x02,
                                    0x12,
                                    0x41,
                                    0x31,
                                    0x02,
                                    0x42,
                                    0x41,
                                    0x71,
                                    0x02,
                                    0x32,
                                    0x81,
                                    0x01,
                                    0x52,
                                    0x12,
                                    0x91,
                                    // Port
                                    0x0f,
                                    0x1b,
                        });

                    // Send extra byte to allow us to verify that connector did not consume too much
                    socket.Send(new byte[]
                        {
                                0xff
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
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Dispose();
            }
        }

        protected override void Act()
        {
            _actual = Connector.Connect(_connectionInfo);
        }

        [TestMethod]
        public void ConnectShouldHaveReturnedSocketCreatedUsingSocketFactory()
        {
            Assert.IsNotNull(_actual);
            Assert.AreSame(_clientSocket, _actual);
        }

        [TestMethod]
        public void ClientSocketShouldBeConnected()
        {
            Assert.IsTrue(_clientSocket.Connected);
        }

        [TestMethod]
        public void ProxyShouldHaveReceivedExpectedSocksRequest()
        {
            var expectedSocksRequest = new byte[]
                {
                    //
                    // Client greeting
                    //

                    // SOCKS version
                    0x05,
                    // Number of authentication methods supported
                    0x02,
                    // No authentication
                    0x00,
                    // Username/password
                    0x02,

                    //
                    // Client connection request
                    //

                    // SOCKS version
                    0x05,
                    // Establish a TCP/IP stream connection
                    0x01,
                    // Reserved
                    0x00,
                    // Destination address type (IPv4)
                    0x01,
                    // Destination address (IPv4)
                    0x7f,
                    0x00,
                    0x00,
                    0x01,
                    // Destination port
                    0x03,
                    0x09

                };

            var errorText = string.Format("Expected:{0}{1}{0}but was:{0}{2}",
                                          Environment.NewLine,
                                          PacketDump.Create(expectedSocksRequest, 2),
                                          PacketDump.Create(_bytesReceivedByProxy, 2));

            Assert.IsTrue(expectedSocksRequest.SequenceEqual(_bytesReceivedByProxy), errorText);
        }

        [TestMethod]
        public void OnlySocksResponseShouldHaveBeenConsumed()
        {
            var buffer = new byte[1];

            var bytesRead = _clientSocket.Receive(buffer);
            Assert.AreEqual(1, bytesRead);
            Assert.AreEqual(0xff, buffer[0]);
        }

        [TestMethod]
        public void CreateOnSocketFactoryShouldHaveBeenInvokedOnce()
        {
            SocketFactoryMock.Verify(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                                     Times.Once());
        }
    }
}
