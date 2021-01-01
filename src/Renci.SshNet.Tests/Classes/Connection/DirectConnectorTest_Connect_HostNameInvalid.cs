using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Net.Sockets;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class DirectConnectorTest_Connect_HostNameInvalid : DirectConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private SocketException _actualException;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = CreateConnectionInfo("invalid.");
            _actualException = null;
        }

        protected override void Act()
        {
            try
            {
                Connector.Connect(_connectionInfo);
                Assert.Fail();
            }
            catch (SocketException ex)
            {
                _actualException = ex;
            }
        }

        [TestMethod]
        public void ConnectShouldHaveThrownSocketException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.AreEqual(SocketError.HostNotFound, _actualException.SocketErrorCode);
        }
    }
}
