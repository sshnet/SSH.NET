using System.Net.Sockets;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class DirectConnectorTest_Connect_HostNameInvalid : DirectConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private Socket _clientSocket;
        private SocketException _actualException;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = CreateConnectionInfo("invalid.");
            _actualException = null;
            _clientSocket = SocketFactory.Create(SocketType.Stream, ProtocolType.Tcp);
        }

        protected override void Act()
        {
            try
            {
                _ = Connector.Connect(_connectionInfo);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                _actualException = ex;
            }
        }

        protected override void SetupMocks()
        {
            _ = SocketFactoryMock.Setup(p => p.Create(SocketType.Stream, ProtocolType.Tcp))
                                 .Returns(_clientSocket);
        }

        [TestMethod]
        public void ConnectShouldHaveThrownSocketException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.IsTrue(_actualException.SocketErrorCode is SocketError.HostNotFound or SocketError.TryAgain or SocketError.NoData);
        }
    }
}
