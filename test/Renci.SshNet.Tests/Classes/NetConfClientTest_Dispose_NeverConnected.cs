using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class NetConfClientTest_Dispose_NeverConnected : NetConfClientTestBase
    {
        private NetConfClient _netConfClient;
        private ConnectionInfo _connectionInfo;
        private int _operationTimeout;

        protected override void SetupData()
        {
            _connectionInfo = new ConnectionInfo("host", "user", new NoneAuthenticationMethod("userauth"));
            _operationTimeout = new Random().Next(1000, 10000);
            _netConfClient = new NetConfClient(_connectionInfo, false, ServiceFactoryMock.Object)
                {
                    OperationTimeout = TimeSpan.FromMilliseconds(_operationTimeout)
                };
        }

        protected override void Act()
        {
            _netConfClient.Dispose();
        }

        [TestMethod]
        public void CreateSocketFactoryOnServiceFactoryShouldNeverBeInvoked()
        {
            ServiceFactoryMock.Verify(p => p.CreateSocketFactory(), Times.Never);
        }

        [TestMethod]
        public void CreateSessionOnServiceFactoryShouldNeverBeInvoked()
        {
            ServiceFactoryMock.Verify(p => p.CreateSession(_connectionInfo, SocketFactoryMock.Object),
                                      Times.Never);
        }

        [TestMethod]
        public void IsConnectedThrowsObjectDisposedException()
        {
            try
            {
                _ = _netConfClient.IsConnected;
                Assert.Fail();
            }
            catch (ObjectDisposedException ex)
            {
                Assert.AreEqual(typeof(ObjectDisposedException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual(typeof(NetConfClient).FullName, ex.ObjectName);
            }
        }
    }
}
