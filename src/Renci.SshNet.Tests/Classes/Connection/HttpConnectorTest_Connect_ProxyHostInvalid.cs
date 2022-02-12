using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Sockets;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class HttpConnectorTest_Connect_ProxyHostInvalid : HttpConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private SocketException _actualException;

        protected override void SetupData()
        {
            base.SetupData();

            _connectionInfo = new ConnectionInfo("localhost",
                                                 40,
                                                 "user",
                                                 ProxyTypes.Http,
                                                 "invalid.",
                                                 80,
                                                 "proxyUser",
                                                 "proxyPwd",
                                                 new KeyboardInteractiveAuthenticationMethod("user"));
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
