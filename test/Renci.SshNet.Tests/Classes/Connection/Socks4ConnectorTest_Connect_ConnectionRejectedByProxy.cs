using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class Socks4ConnectorTest_Connect_ConnectionRejectedByProxy : Socks4ConnectorTestBase
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

            _connectionInfo = CreateConnectionInfo("proxyUser", "proxyPwd");
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(100);
            _bytesReceivedByProxy = new List<byte>();
            _clientSocket = SocketFactory.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _actualException = null;

            _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.ProxyPort));
            _proxyServer.Disconnected += socket => _disconnected = true;
            _proxyServer.BytesReceived += (bytesReceived, socket) =>
                {
                    if (_bytesReceivedByProxy.Count == 0)
                    {
                        _ = socket.Send(new byte[]
                            {
                                // Reply version (null byte)
                                0x00,
                                // Connection refused
                                 0x5b
                            });
                    }

                    _bytesReceivedByProxy.AddRange(bytesReceived);
                };
            _proxyServer.Start();
        }

        protected override void SetupMocks()
        {
            _ = SocketFactoryMock.Setup(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                                 .Returns(_clientSocket);
        }

        protected override void TearDown()
        {
            base.TearDown();

            _proxyServer?.Dispose();
            _clientSocket?.Dispose();
        }

        protected override void Act()
        {
            try
            {
                _ = Connector.Connect(_connectionInfo);
                Assert.Fail();
            }
            catch (ProxyException ex)
            {
                _actualException = ex;
            }

            // Give some time to process all messages
            Thread.Sleep(200);
        }

        [TestMethod]
        public void ConnectShouldHaveThrownProxyException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual("SOCKS4: Connection rejected.", _actualException.Message);
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
                _ = _clientSocket.Receive(Array.Empty<byte>());
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
