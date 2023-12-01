using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class DirectConnectorTest_Connect_TimeoutConnectingToServer : DirectConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private Exception _actualException;
        private Socket _clientSocket;
        private Stopwatch _stopWatch;

        protected override void SetupData()
        {
            base.SetupData();

            var random = new Random();

            _connectionInfo = CreateConnectionInfo(IPAddress.Loopback.ToString());
            _connectionInfo.Timeout = TimeSpan.FromMilliseconds(random.Next(50, 200));
            _stopWatch = new Stopwatch();
            _actualException = null;

            _clientSocket = SocketFactory.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        protected override void SetupMocks()
        {
            _ = SocketFactoryMock.Setup(p => p.Create(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                                 .Returns(_clientSocket);
        }

        protected override void TearDown()
        {
            base.TearDown();

            _clientSocket?.Dispose();
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
            catch (SocketException ex)
            {
                _actualException = ex;
            }
            finally
            {
                _stopWatch.Stop();
            }
        }

        [TestMethodForPlatform(nameof(OSPlatform.Windows))]
        public void ConnectShouldHaveThrownSshOperationTimeoutExceptionOnWindows()
        {
            Assert.IsNull(_actualException.InnerException);
            Assert.IsInstanceOfType<SshOperationTimeoutException>(_actualException);
            Assert.AreEqual(string.Format(CultureInfo.InvariantCulture, "Connection failed to establish within {0} milliseconds.", _connectionInfo.Timeout.TotalMilliseconds), _actualException.Message);
        }

        [TestMethodForPlatform(nameof(OSPlatform.Linux))]
        public void ConnectShouldHaveThrownSocketExceptionOnLinux()
        {
            Assert.IsNull(_actualException.InnerException);
            Assert.IsInstanceOfType<SocketException>(_actualException);
            Assert.AreEqual("Connection refused", _actualException.Message);
        }

        [TestMethodForPlatform(nameof(OSPlatform.Windows))]
        public void ConnectShouldHaveRespectedTimeoutOnWindows()
        {
            var errorText = string.Format("Elapsed: {0}, Timeout: {1}",
                                          _stopWatch.ElapsedMilliseconds,
                                          _connectionInfo.Timeout.TotalMilliseconds);

            // Compare elapsed time with configured timeout, allowing for a margin of error
            Assert.IsTrue(_stopWatch.ElapsedMilliseconds >= _connectionInfo.Timeout.TotalMilliseconds - 10, errorText);
            Assert.IsTrue(_stopWatch.ElapsedMilliseconds < _connectionInfo.Timeout.TotalMilliseconds + 100, errorText);
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
