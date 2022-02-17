using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Common;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class Socks5ConnectorTest_Connect_ConnectionToProxyRefused : Socks5ConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private SocketException _actualException;
        private Socket _clientSocket;
        private Stopwatch _stopWatch;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = CreateConnectionInfo("proxyUser", "proxyPwd");
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(5000);
            _stopWatch = new Stopwatch();
            _actualException = null;

            _clientSocket = SocketFactory.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        protected override void SetupMocks()
        {
            SocketFactoryMock.Setup(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                             .Returns(_clientSocket);
        }

        protected override void TearDown()
        {
            base.TearDown();

            if (_clientSocket != null)
            {
                _clientSocket.Dispose();
            }
        }

        protected override void Act()
        {
            _stopWatch.Start();

            try
            {
                Connector.Connect(_connectionInfo);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                _actualException = ex;
            }
            finally
            {
                _stopWatch.Stop();
            }
        }

        [TestMethod]
        public void ConnectShouldHaveThrownSocketException()
        {
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual(SocketError.ConnectionRefused, _actualException.SocketErrorCode);
        }

        [TestMethod]
        public void ConnectShouldHaveRespectedTimeout()
        {
            var errorText = string.Format("Elapsed: {0}, Timeout: {1}",
                                          _stopWatch.ElapsedMilliseconds,
                                          _connectionInfo.Timeout.TotalMilliseconds);

            // Compare elapsed time with configured timeout, allowing for a margin of error
            Assert.IsTrue(_stopWatch.ElapsedMilliseconds < _connectionInfo.Timeout.TotalMilliseconds + 100, errorText);
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
