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
        private Socket _actual;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = CreateConnectionInfo(IPAddress.Loopback.ToString());
            _connectionInfo.SocketBufferSize = ConnectionInfo.AutoTuneSocketBufferSize;

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

        // The OS may adjust a socket's buffer size on its own once connected (independently of
        // whether our code requests an explicit size), so the only portably-observable proof
        // that AutoTuneSocketBufferSize skips our explicit override is that the result differs
        // from what the legacy hardcoded computation would have explicitly requested.
        [TestMethod]
        public void SendBufferSizeShouldNotMatchLegacyDefault()
        {
            Assert.AreNotEqual(10 * Session.MaximumSshPacketSize, _actual.SendBufferSize);
        }

        [TestMethod]
        public void ReceiveBufferSizeShouldNotMatchLegacyDefault()
        {
            Assert.AreNotEqual(10 * Session.MaximumSshPacketSize, _actual.ReceiveBufferSize);
        }
    }
}
