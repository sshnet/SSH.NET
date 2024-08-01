﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Connection;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class Socks5ConnectorTest_Connect_UserNamePasswordAuthentication_ConnectionSucceeded : Socks5ConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ProxyConnectionInfo _proxyConnectionInfo;
        private AsyncSocketListener _proxyServer;
        private Socket _clientSocket;
        private IConnector _proxyConnector;
        private List<byte> _bytesReceivedByProxy;
        private Socket _actual;

        protected override void SetupData()
        {
            base.SetupData();

            _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, 0));
            _proxyServer.BytesReceived += (bytesReceived, socket) =>
                {
                    _bytesReceivedByProxy.AddRange(bytesReceived);

                    if (_bytesReceivedByProxy.Count == 4)
                    {
                        // We received the greeting

                        _ = socket.Send(new byte[]
                            {
                                    // SOCKS version
                                    0x05,
                                    // Require username/password authentication
                                    0x02
                            });
                    }
                    else if (_bytesReceivedByProxy.Count == 4 + (1 + 1 + _proxyConnectionInfo.Username.Length + 1 + _proxyConnectionInfo.Password.Length))
                    {
                        // We received the username/password authentication request

                        _ = socket.Send(new byte[]
                            {
                                    // Authentication version
                                    0x01,
                                    // Authentication successful
                                    0x00
                            });
                    }
                    else if (_bytesReceivedByProxy.Count == 4 + (1 + 1 + _proxyConnectionInfo.Username.Length + 1 + _proxyConnectionInfo.Password.Length) + (1 + 1 + 1 + 1 + 4 + 2))
                    {
                        // We received the connection request

                        _ = socket.Send(new byte[]
                            {
                                    // SOCKS version
                                    0x05,
                                    // Connection successful
                                    0x00,
                                    // Reserved byte
                                    0x00,
                            });

                        // Send server bound address
                        _ = socket.Send(new byte[]
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
                        _ = socket.Send(new byte[]
                            {
                                0xfe
                            });
                    }
                };
            _proxyServer.Start();

            _connectionInfo = CreateConnectionInfo(GenerateRandomString(0, 255), GenerateRandomString(0, 255), ((IPEndPoint)_proxyServer.ListenerEndPoint).Port);
            _proxyConnectionInfo = (ProxyConnectionInfo)_connectionInfo.ProxyConnection;
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(100);
            _bytesReceivedByProxy = new List<byte>();

            _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);
            _proxyConnector = ServiceFactory.CreateConnector(_proxyConnectionInfo, SocketFactoryMock.Object);
        }

        protected override void SetupMocks()
        {
            _ = SocketFactoryMock.Setup(p => p.Create(SocketType.Stream, ProtocolType.Tcp))
                                 .Returns(_clientSocket);
            _ = ServiceFactoryMock.Setup(p => p.CreateConnector(_proxyConnectionInfo, SocketFactoryMock.Object))
                                  .Returns(_proxyConnector);
        }

        protected override void TearDown()
        {
            base.TearDown();

            _proxyServer?.Dispose();

            if (_clientSocket != null)
            {
                _clientSocket.Shutdown(SocketShutdown.Both);
                _clientSocket.Dispose();
            }

            _proxyConnector?.Dispose();
        }

        protected override void Act()
        {
            _actual = Connector.Connect(_connectionInfo);

            // Give some time to process all messages
            Thread.Sleep(200);
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
            expectedSocksRequest.Add((byte)_proxyConnectionInfo.Username.Length);
            // Username
            expectedSocksRequest.AddRange(Encoding.ASCII.GetBytes(_proxyConnectionInfo.Username));
            // Length of the password
            expectedSocksRequest.Add((byte)_proxyConnectionInfo.Password.Length);
            // Password
            expectedSocksRequest.AddRange(Encoding.ASCII.GetBytes(_proxyConnectionInfo.Password));

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
            expectedSocksRequest.Add(0x04);
            expectedSocksRequest.Add(0x05);

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
            SocketFactoryMock.Verify(p => p.Create(SocketType.Stream, ProtocolType.Tcp),
                                     Times.Once());
        }
    }
}
