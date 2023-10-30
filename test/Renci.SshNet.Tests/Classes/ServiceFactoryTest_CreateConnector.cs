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
        private Mock<ISocketFactory> _socketFactoryMock;

        [TestInitialize]
        public void Setup()
        {
            _serviceFactory = new ServiceFactory();
            _connectionInfoMock = new Mock<IConnectionInfo>(MockBehavior.Strict);
            _socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        }

        [TestMethod]
        public void ConnectionInfoIsNull()
        {
            const IConnectionInfo connectionInfo = null;

            try
            {
                _serviceFactory.CreateConnector(connectionInfo, _socketFactoryMock.Object);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("connectionInfo", ex.ParamName);
            }
        }

        [TestMethod]
        public void SocketFactoryIsNull()
        {
            const ISocketFactory socketFactory = null;

            try
            {
                _serviceFactory.CreateConnector(_connectionInfoMock.Object, socketFactory);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("socketFactory", ex.ParamName);
            }
        }

        [TestMethod]
        public void ProxyType_Http()
        {
            _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.Http);

            var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(HttpConnector), actual.GetType());

            var httpConnector = (HttpConnector) actual;
            Assert.AreSame(_socketFactoryMock.Object, httpConnector.SocketFactory);

            _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
        }

        [TestMethod]
        public void ProxyType_None()
        {
            _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.None);

            var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(DirectConnector), actual.GetType());

            var directConnector = (DirectConnector) actual;
            Assert.AreSame(_socketFactoryMock.Object, directConnector.SocketFactory);

            _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
        }

        [TestMethod]
        public void ProxyType_Socks4()
        {
            _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.Socks4);

            var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(Socks4Connector), actual.GetType());

            var socks4Connector = (Socks4Connector) actual;
            Assert.AreSame(_socketFactoryMock.Object, socks4Connector.SocketFactory);

            _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
        }

        [TestMethod]
        public void ProxyType_Socks5()
        {
            _connectionInfoMock.Setup(p => p.ProxyType).Returns(ProxyTypes.Socks5);

            var actual = _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);

            Assert.IsNotNull(actual);
            Assert.AreEqual(typeof(Socks5Connector), actual.GetType());

            var socks5Connector = (Socks5Connector) actual;
            Assert.AreSame(_socketFactoryMock.Object, socks5Connector.SocketFactory);

            _connectionInfoMock.Verify(p => p.ProxyType, Times.Once);
        }

        [TestMethod]
        public void ProxyType_Undefined()
        {
            _connectionInfoMock.Setup(p => p.ProxyType).Returns((ProxyTypes) 666);

            try
            {
                _serviceFactory.CreateConnector(_connectionInfoMock.Object, _socketFactoryMock.Object);
                Assert.Fail();
            }
            catch (NotSupportedException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("ProxyTypes '666' is not supported.", ex.Message);
            }

            _connectionInfoMock.Verify(p => p.ProxyType, Times.Exactly(2));
        }
    }
}
