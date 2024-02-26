using System;
using System.Diagnostics;
using System.Globalization;
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
    public class Socks4ConnectorTest_Connect_TimeoutReadingDestinationAddress : Socks4ConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private SshOperationTimeoutException _actualException;
        private Socket _clientSocket;
        private AsyncSocketListener _proxyServer;
        private Stopwatch _stopWatch;
        private AsyncSocketListener _server;
        private bool _disconnected;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();

            _connectionInfo = CreateConnectionInfo("proxyUser", "proxyPwd");
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(random.Next(50, 200));
            _stopWatch = new Stopwatch();
            _actualException = null;

            _clientSocket = SocketFactory.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _proxyServer = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.ProxyPort));
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

            _server = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.Port));
            _server.Start();
        }

        protected override void SetupMocks()
        {
            _ = SocketFactoryMock.Setup(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                                 .Returns(_clientSocket);
        }

        protected override void TearDown()
        {
            base.TearDown();

            _server?.Dispose();
            _proxyServer?.Dispose();
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
            Assert.IsInstanceOfType<SshOperationTimeoutException>(_actualException);
            Assert.IsInstanceOfType<SocketException>(_actualException.InnerException);
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
            SocketFactoryMock.Verify(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp),
                                     Times.Once());
        }
    }
}
