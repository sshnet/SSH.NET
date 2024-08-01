using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Connection;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class Socks4ConnectorTest_Connect_ConnectionRejectedByProxy : Socks4ConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ProxyConnectionInfo _proxyConnectionInfo;
        private AsyncSocketListener _proxyServer;
        private Socket _clientSocket;
        private IConnector _proxyConnector;
        private List<byte> _bytesReceivedByProxy;
        private bool _disconnected;
        private ProxyException _actualException;

        protected override void SetupData()
        {
            base.SetupData();

            _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, 0));
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

            _connectionInfo = CreateConnectionInfo("proxyUser", "proxyPwd", 777, ((IPEndPoint)_proxyServer.ListenerEndPoint).Port);
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(100);
            _proxyConnectionInfo = (ProxyConnectionInfo)_connectionInfo.ProxyConnection;
            _bytesReceivedByProxy = new List<byte>();
            _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);
            _proxyConnector = ServiceFactory.CreateConnector(_proxyConnectionInfo, SocketFactoryMock.Object);
            _actualException = null;
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
            _clientSocket?.Dispose();
            _proxyConnector?.Dispose();
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
                _ = _clientSocket.Receive(new byte[0]);
                Assert.Fail();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        [TestMethod]
        public void CreateOnSocketFactoryShouldHaveBeenInvokedOnce()
        {
            SocketFactoryMock.Verify(p => p.Create(SocketType.Stream, ProtocolType.Tcp),
                                     Times.Once());
        }
    }
}
