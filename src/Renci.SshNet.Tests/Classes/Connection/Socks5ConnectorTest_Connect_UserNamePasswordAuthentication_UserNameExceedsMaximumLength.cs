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
    public class Socks5ConnectorTest_Connect_UserNamePasswordAuthentication_UserNameExceedsMaximumLength : Socks5ConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private AsyncSocketListener _proxyServer;
        private Socket _clientSocket;
        private List<byte> _bytesReceivedByProxy;
        private bool _disconnected;
        private ProxyException _actualException;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = CreateConnectionInfo(new string('a', 256), new string('b', 255));
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(100);
            _bytesReceivedByProxy = new List<byte>();
            _actualException = null;

            _clientSocket = SocketFactory.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.ProxyPort));
            _proxyServer.Disconnected += socket => _disconnected = true;
            _proxyServer.BytesReceived += (bytesReceived, socket) =>
                {
                    _bytesReceivedByProxy.AddRange(bytesReceived);

                    // Wait until we received the greeting
                    if (_bytesReceivedByProxy.Count == 4)
                    {
                        socket.Send(new byte[]
                            {
                                // SOCKS version
                                0x05,
                                // Username/password authentication
                                0x02
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
            try
            {
                Connector.Connect(_connectionInfo);
                Assert.Fail();
            }
            catch (ProxyException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void ConnectShouldHaveThrownProxyException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("Proxy username is too long.", _actualException.Message);
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

            var errorText = string.Format("Expected:{0}{1}{0}but was:{0}{2}",
                                          Environment.NewLine,
                                          PacketDump.Create(expectedSocksRequest, 2),
                                          PacketDump.Create(_bytesReceivedByProxy, 2));

            Assert.IsTrue(expectedSocksRequest.SequenceEqual(_bytesReceivedByProxy), errorText);
        }

        [TestMethod]
        public void ConnectionToProxyShouldHaveBeenShutDown()
        {
            Assert.IsTrue(_disconnected);
        }

        [TestMethod]
        public void ClientSocketShouldHaveBeenDisposed()
        {
            try
            {
                _clientSocket.Receive(new byte[0]);
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void CreateOnSocketFactoryShouldHaveBeenInvokedOnce()
        {
            SocketFactoryMock.Verify(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                                     Times.Once());
        }
    }
}
