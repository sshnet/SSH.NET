using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Connection;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class HttpConnectorTest_Connect_ProxyResponseDoesNotContainHttpStatusLine : HttpConnectorTestBase
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

            _connectionInfo = new ConnectionInfo(IPAddress.Loopback.ToString(),
                                                 777,
                                                 "user",
                                                 ProxyTypes.Http,
                                                 IPAddress.Loopback.ToString(),
                                                 8122,
                                                 "proxyUser",
                                                 "proxyPwd",
                                                 new KeyboardInteractiveAuthenticationMethod("user"));
            _proxyConnectionInfo = (ProxyConnectionInfo)_connectionInfo.ProxyConnection;
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(100);
            _bytesReceivedByProxy = new List<byte>();
            _actualException = null;

            _clientSocket = SocketFactory.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _proxyConnector = ServiceFactory.CreateConnector(_proxyConnectionInfo, SocketFactory);

            _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _proxyConnectionInfo.Port));
            _proxyServer.Disconnected += socket => _disconnected = true;
            _proxyServer.BytesReceived += (bytesReceived, socket) =>
                {
                    if (_bytesReceivedByProxy.Count == 0)
                    {
                        socket.Send(Encoding.ASCII.GetBytes("Whatever\r\n"));
                        socket.Shutdown(SocketShutdown.Send);
                    }

                    _bytesReceivedByProxy.AddRange(bytesReceived);
                };
            _proxyServer.Start();
        }

        protected override void SetupMocks()
        {
            SocketFactoryMock.Setup(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                             .Returns(_clientSocket);
            ServiceFactoryMock.Setup(p => p.CreateConnector(_proxyConnectionInfo, SocketFactory))
                              .Returns(_proxyConnector);
        }

        protected override void TearDown()
        {
            base.TearDown();

            if (_proxyServer != null)
            {
                _proxyServer.Dispose();
            }

            if (_proxyConnector != null)
            {
                _proxyConnector.Dispose();
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
            Assert.AreEqual("HTTP response does not contain status line.", _actualException.Message);
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
