using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Connection;
using System;

namespace Renci.SshNet.Tests.Classes
{
    [TestClass]
    public class ServiceFactoryTest_CreateConnector
    {
        private ServiceFactory _serviceFactory;
        private Mock<IConnectionInfo> _connectionInfoMock;

        [TestInitialize]
        public void Setup()
        {
            _serviceFactory = new ServiceFactory();
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
        }

        [TestMethod]
        public void ConnectionInfoIsNull()
        {
            const IConnectionInfo connectionInfo = null;

            try
            {
                _serviceFactory.CreateConnector(connectionInfo);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("connectionInfo", ex.ParamName);
            }
        }

        [TestMethod]
        public void ProxyType_Http()
        {
            _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.Http);

            var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(HttpConnector), actual.GetType());

            _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
        }

        [TestMethod]
        public void ProxyType_None()
        {
            _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.None);

            var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(DirectConnector), actual.GetType());

            _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
        }

        [TestMethod]
        public void ProxyType_Socks4()
        {
            _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.Socks4);

            var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(Socks4Connector), actual.GetType());

            _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
        }

        [TestMethod]
        public void ProxyType_Socks5()
        {
            _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.Socks5);

            var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(Socks5Connector), actual.GetType());

            _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
        }
    }
}
