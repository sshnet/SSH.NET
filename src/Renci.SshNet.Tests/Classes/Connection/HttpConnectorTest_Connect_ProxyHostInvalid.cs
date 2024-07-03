using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Connection;
using System.Net.Sockets;

namespace Renci.SshNet.Tests.Classes.Connection
{
    [TestClass]
    public class HttpConnectorTest_Connect_ProxyHostInvalid : HttpConnectorTestBase
    {
        private ConnectionInfo _connectionInfo;
        private ProxyConnectionInfo _proxyConnectionInfo;
        private IConnector _proxyConnector;
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
            _proxyConnectionInfo = (ProxyConnectionInfo)_connectionInfo.ProxyConnection;
            _proxyConnector = ServiceFactory.CreateConnector(_proxyConnectionInfo, SocketFactoryMock.Object);
            _actualException = null;
        }

        protected override void SetupMocks()
        {
            _ = ServiceFactoryMock.Setup(p => p.CreateConnector(_proxyConnectionInfo, SocketFactoryMock.Object))
                              .Returns(_proxyConnector);
        }

        protected override void TearDown()
        {
            base.TearDown();

            _proxyConnector?.Dispose();
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

        [TestMethod]
        public void ConnectShouldHaveThrownSocketException()
        {
            Assert.IsNotNull(_actualException);
            Assert.IsNull(_actualException.InnerException);
            Assert.IsTrue(_actualException.SocketErrorCode is SocketError.HostNotFound or SocketError.TryAgain);
        }
    }
}
