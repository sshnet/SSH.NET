using System.Net;
using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class DirectConnectorTest_Connect_SocketBufferSizeAutoTune : DirectConnectorTestBase
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
            _connectionInfo.SocketBufferSize = ConnectionInfo.AutoTuneSocketBufferSize;

            // A freshly-created, never-connected socket represents the OS default we expect
            // our connect logic to leave untouched when AutoTuneSocketBufferSize is requested.
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
        public void SendBufferSizeShouldNotBeOverridden()
        {
            Assert.AreEqual(_controlSocket.SendBufferSize, _actual.SendBufferSize);
        }

        [TestMethod]
        public void ReceiveBufferSizeShouldNotBeOverridden()
        {
            Assert.AreEqual(_controlSocket.ReceiveBufferSize, _actual.ReceiveBufferSize);
        }

        [TestMethod]
        public void SendBufferSizeShouldNotMatchLegacyDefault()
        {
            Assert.AreNotEqual(10 * Session.MaximumSshPacketSize, _actual.SendBufferSize);
        }
    }
}
