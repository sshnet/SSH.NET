using System.Net;
using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class DirectConnectorTest_Connect_SocketBufferSizeExplicit : DirectConnectorTestBase
    {
        private const int ExplicitBufferSize = 1024 * 1024;

        private ConnectionInfo _connectionInfo;
        private AsyncSocketListener _server;
        private Socket _clientSocket;
        private Socket _actual;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = CreateConnectionInfo(IPAddress.Loopback.ToString());
            _connectionInfo.SocketBufferSize = ExplicitBufferSize;

            _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);

            _server = new AsyncSocketListener(new IPEndPoint(IPAddress.Loopback, _connectionInfo.Port));
            _server.Start();
        }

        protected override void SetupMocks()
        {
            _ = SocketFactoryMock.Setup(p => p.Create(SocketType.Stream, ProtocolType.Tcp))
                                 .Returns(_clientSocket);
        }

        protected override void TearDown()
        {
            base.TearDown();

            _server?.Dispose();
            _clientSocket?.Dispose();
        }

        protected override void Act()
        {
            _actual = Connector.Connect(_connectionInfo);
        }

        [TestMethod]
        public void SendBufferSizeShouldMatchExplicitValue()
        {
            Assert.AreEqual(ExplicitBufferSize, _actual.SendBufferSize);
        }

        [TestMethod]
        public void ReceiveBufferSizeShouldMatchExplicitValue()
        {
            Assert.AreEqual(ExplicitBufferSize, _actual.ReceiveBufferSize);
        }
    }
}
