using System;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using Renci.SshNet.Connection;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to connect and interact with SSH server.
    /// </summary>
    [TestClass]
    public partial class SessionTest : TestBase
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<ISocketFactory> _socketFactoryMock;

        protected override void OnInit()
        {
            base.OnInit();

            _serviceFactoryMock = new Mock<IServiceFactory>(MockBehavior.Strict);
            _socketFactoryMock = new Mock<ISocketFactory>(MockBehavior.Strict);
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenConnectionInfoIsNull()
        {
            const ConnectionInfo connectionInfo = null;

            try
            {
                _ = new Session(connectionInfo, _serviceFactoryMock.Object, _socketFactoryMock.Object);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("connectionInfo", ex.ParamName);
            }
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenServiceFactoryIsNull()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            var connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));
            IServiceFactory serviceFactory = null;

            try
            {
                _ = new Session(connectionInfo, serviceFactory, _socketFactoryMock.Object);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("serviceFactory", ex.ParamName);
            }
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenSocketFactoryIsNull()
        {
            var serverEndPoint = new IPEndPoint(IPAddress.Loopback, 8122);
            var connectionInfo = CreateConnectionInfo(serverEndPoint, TimeSpan.FromSeconds(5));
            const ISocketFactory socketFactory = null;

            try
            {
                _ = new Session(connectionInfo, _serviceFactoryMock.Object, socketFactory);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("socketFactory", ex.ParamName);
            }
        }

        private static ConnectionInfo CreateConnectionInfo(IPEndPoint serverEndPoint, TimeSpan timeout)
        {
            return new ConnectionInfo(serverEndPoint.Address.ToString(), serverEndPoint.Port, "eric", new NoneAuthenticationMethod("eric"))
            {
                Timeout = timeout
            };
        }
    }
}
