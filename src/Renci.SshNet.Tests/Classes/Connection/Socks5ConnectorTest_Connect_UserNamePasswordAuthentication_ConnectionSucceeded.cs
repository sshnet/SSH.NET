using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class Socks5ConnectorTest_Connect_UserNamePasswordAuthentication_ConnectionSucceeded : Socks5ConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private AsyncSocketListener _proxyServer;
        private Socket _clientSocket;
        private List<byte> _bytesReceivedByProxy;
        private Socket _actual;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = CreateConnectionInfo(GenerateRandomString(0, 255), GenerateRandomString(0, 255));
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
                                    // Require username/password authentication
                                    0x02
                            });
                    }
                    else if (_bytesReceivedByProxy.Count == 4 + (1 + 1 + _connectionInfo.ProxyUsername.Length + 1 + _connectionInfo.ProxyPassword.Length))
                    {
                        // We received the username/password authentication request

                        socket.Send(new byte[]
                            {
                                    // Authentication version
                                    0x01,
                                    // Authentication successful
                                    0x00
                            });
                    }
                    else if (_bytesReceivedByProxy.Count == 4 + (1 + 1 + _connectionInfo.ProxyUsername.Length + 1 + _connectionInfo.ProxyPassword.Length) + (1 + 1 + 1 + 1 + 4 + 2))
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
                                    // IPv4
                                    0x01,
                                    // IP address
                                    0x01,
                                    0x02,
                                    0x12,
                                    0x41,
                                    // Port
                                    0x01,
                                    0x02,
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
            var expectedSocksRequest = new List<byte>();

            //
            // Client greeting
            //

            // SOCKS version
            expectedSocksRequest.Add(0x05);
            // Number of authentication methods supported
            expectedSocksRequest.Add(0x02);
            // No authentication
            expectedSocksRequest.Add(0x00);
            // Username/password
            expectedSocksRequest.Add(0x02);

            //
            // Username/password authentication request
            //

            // Version of the negotiation
            expectedSocksRequest.Add(0x01);
            // Length of the username
            expectedSocksRequest.Add((byte)_connectionInfo.ProxyUsername.Length);
            // Username
            expectedSocksRequest.AddRange(Encoding.ASCII.GetBytes(_connectionInfo.ProxyUsername));
            // Length of the password
            expectedSocksRequest.Add((byte)_connectionInfo.ProxyPassword.Length);
            // Password
            expectedSocksRequest.AddRange(Encoding.ASCII.GetBytes(_connectionInfo.ProxyPassword));

            //
            // Client connection request
            //

            // SOCKS version
            expectedSocksRequest.Add(0x05);
            // Establish a TCP/IP stream connection
            expectedSocksRequest.Add(0x01);
            // Reserved
            expectedSocksRequest.Add(0x00);
            // Destination address type (IPv4)
            expectedSocksRequest.Add(0x01);
            // Destination address (IPv4)
            expectedSocksRequest.Add(0x7f);
            expectedSocksRequest.Add(0x00);
            expectedSocksRequest.Add(0x00);
            expectedSocksRequest.Add(0x01);
            // Destination port
            expectedSocksRequest.Add(0x03);
            expectedSocksRequest.Add(0x09);

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
            Assert.AreEqual(0xfe, buffer[0]);
        }

        [TestMethod]
        public void CreateOnSocketFactoryShouldHaveBeenInvokedOnce()
        {
            SocketFactoryMock.Verify(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                                     Times.Once());
        }
    }
}
