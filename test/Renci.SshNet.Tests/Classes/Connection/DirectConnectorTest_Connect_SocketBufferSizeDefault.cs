using System.Net;
using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class DirectConnectorTest_Connect_SocketBufferSizeDefault : DirectConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private AsyncSocketListener _server;
        private Socket _clientSocket;
        private Socket _controlSocket;
        private Socket _actual;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = CreateConnectionInfo(IPAddress.Loopback.ToString());

            // A freshly-created, never-touched socket represents the platform's minimal
            // default, used as a lower bound below. The OS may clamp or otherwise adjust an
            // explicitly requested buffer size (e.g. against net.core.wmem_max/rmem_max on
            // Linux), so the exact value our code requests is not portably observable via
            // Socket.SendBufferSize/ReceiveBufferSize after connecting — only that it is
            // clearly larger than doing nothing.
            _controlSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);

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
            _controlSocket?.Dispose();
        }

        protected override void Act()
        {
            _actual = Connector.Connect(_connectionInfo);
        }

        [TestMethod]
        public void SocketBufferSizeShouldBeNull()
        {
            Assert.IsNull(_connectionInfo.SocketBufferSize);
        }

        [TestMethod]
        public void SendBufferSizeShouldBeLargerThanUntouchedDefault()
        {
            Assert.IsGreaterThan(_controlSocket.SendBufferSize, _actual.SendBufferSize);
        }

        [TestMethod]
        public void ReceiveBufferSizeShouldBeLargerThanUntouchedDefault()
        {
            Assert.IsGreaterThan(_controlSocket.ReceiveBufferSize, _actual.ReceiveBufferSize);
        }
    }
}
