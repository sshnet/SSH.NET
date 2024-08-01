using System;
using System.Diagnostics;
using System.Globalization;
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
    public class Socks4ConnectorTest_Connect_TimeoutReadingDestinationAddress : Socks4ConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ProxyConnectionInfo _proxyConnectionInfo;
        private SshOperationTimeoutException _actualException;
        private Socket _clientSocket;
        private IConnector _proxyConnector;
        private AsyncSocketListener _proxyServer;
        private Stopwatch _stopWatch;
        private AsyncSocketListener _server;
        private bool _disconnected;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();

            _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, 0));
            _proxyServer.Disconnected += socket => _disconnected = true;
            _proxyServer.Connected += socket =>
            {
                _ = socket.Send(new byte[]
                    {
                            // Reply version (null byte)
                            0x00,
                            // Request granted
                            0x5a,
                            // Incomplete destination address
                            0x01
                    });
            };
            _proxyServer.Start();

            _server = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, 0));
            _server.Start();

            _connectionInfo = CreateConnectionInfo("proxyUser", "proxyPwd", ((IPEndPoint)_server.ListenerEndPoint).Port, ((IPEndPoint)_proxyServer.ListenerEndPoint).Port);
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(random.Next(50, 200));
            _proxyConnectionInfo = (ProxyConnectionInfo)_connectionInfo.ProxyConnection;
            _stopWatch = new Stopwatch();
            _actualException = null;

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

            _server?.Dispose();
            _proxyServer?.Dispose();
            _proxyConnector?.Dispose();
        }

        protected override void Act()
        {
            _stopWatch.Start();

            try
            {
                _ = Connector.Connect(_connectionInfo);
                Assert.Fail();
            }
            catch (SshOperationTimeoutException ex)
            {
                _actualException = ex;
            }
            finally
            {
                _stopWatch.Stop();
            }

            // Give some time to process all messages
            Thread.Sleep(200);
        }

        [TestMethod]
        public void ConnectShouldHaveThrownSshOperationTimeoutException()
        {
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, "Socket read operation has timed out after {0:F0} milliseconds.", _connectionInfo.Timeout.TotalMilliseconds), _actualException.Message);
        }

        [TestMethod]
        public void ConnectShouldHaveRespectedTimeout()
        {
            var errorText = string.Format("Elapsed: {0}, Timeout: {1}",
                                          _stopWatch.ElapsedMilliseconds,
                                          _connectionInfo.Timeout.TotalMilliseconds);

            // Compare elapsed time with configured timeout, allowing for a margin of error
            Assert.IsTrue(_stopWatch.ElapsedMilliseconds >= _connectionInfo.Timeout.TotalMilliseconds - 10, errorText);
            Assert.IsTrue(_stopWatch.ElapsedMilliseconds < _connectionInfo.Timeout.TotalMilliseconds + 100, errorText);
        }

        [TestMethod]
        public void ClientSocketShouldNotBeConnected()
        {
            Assert.IsTrue(_disconnected);
            Assert.IsFalse(_clientSocket.Connected);
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
