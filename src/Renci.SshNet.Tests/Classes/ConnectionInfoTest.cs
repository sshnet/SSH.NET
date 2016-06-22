using System.Globalization;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Represents remote connection information class.
    /// </summary>
    [TestClass]
    public class ConnectionInfoTest : TestBase
    {
        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldNotThrowExceptionhenProxyTypesIsNoneAndProxyHostIsNull()
        {
            const string proxyHost = null;

            var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME,
                ProxyTypes.None, proxyHost, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

            Assert.IsNull(connectionInfo.ProxyHost);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentNullExceptionhenProxyTypesIsNotNoneAndProxyHostIsNull()
        {
            try
            {
                new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http, null,
                    int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                    new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("proxyHost", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldNotThrowExceptionWhenProxyTypesIsNotNoneAndProxyHostIsEmpty()
        {
            var proxyHost = string.Empty;

            var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME,
                ProxyTypes.Http, string.Empty, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

            Assert.AreSame(proxyHost, connectionInfo.ProxyHost);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentOutOfRangeExceptionWhenProxyTypesIsNotNoneAndProxyPortIsGreaterThanMaximumValue()
        {
            var maxPort = IPEndPoint.MaxPort;

            try
            {
                new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http, Resources.HOST, ++maxPort, Resources.USERNAME, Resources.PASSWORD, null);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("proxyPort", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentOutOfRangeExceptionWhenProxyTypesIsNotNoneAndProxyPortIsLessThanMinimumValue()
        {
            var minPort = IPEndPoint.MinPort;

            try
            {
                new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http, Resources.HOST, --minPort, Resources.USERNAME, Resources.PASSWORD, null);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("proxyPort", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void Test_ConnectionInfo_ProxyPort_Valid()
        {
            var proxyPort = new Random().Next(IPEndPoint.MinPort, IPEndPoint.MaxPort);

            var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME,
                ProxyTypes.None, Resources.HOST, proxyPort, Resources.USERNAME, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

            Assert.AreEqual(proxyPort, connectionInfo.ProxyPort);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldNotThrowExceptionWhenProxyTypesIsNotNoneAndProxyUsernameIsNull()
        {
            const string proxyUsername = null;

            var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.Http,
                    Resources.PROXY_HOST, int.Parse(Resources.PORT), proxyUsername, Resources.PASSWORD,
                    new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

            Assert.IsNull(connectionInfo.ProxyUsername);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentNullExceptionhenHostIsNull()
        {
            try
            {
                new ConnectionInfo(null, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST,
                    int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("host", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldNotThrowExceptionWhenHostIsEmpty()
        {
            var host = string.Empty;

            var connectionInfo = new ConnectionInfo(host, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None,
                Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

            Assert.AreSame(host, connectionInfo.Host);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldNotThrowExceptionWhenHostIsInvalidDnsName()
        {
            const string host = "in_valid_host.";

            var connectionInfo = new ConnectionInfo(host, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None,
                Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

            Assert.AreSame(host, connectionInfo.Host);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void Test_ConnectionInfo_Host_Valid()
        {
            var host = new Random().Next().ToString(CultureInfo.InvariantCulture);

            var connectionInfo = new ConnectionInfo(host, int.Parse(Resources.PORT), Resources.USERNAME,
                ProxyTypes.None, Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

            Assert.AreSame(host, connectionInfo.Host);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentOutOfRangeExceptionWhenPortIsGreaterThanMaximumValue()
        {
            const int port = IPEndPoint.MaxPort + 1;

            try
            {
                new ConnectionInfo(Resources.HOST, port, Resources.USERNAME, ProxyTypes.None, Resources.HOST,
                    int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("port", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentOutOfRangeExceptionWhenPortIsLessThanMinimumValue()
        {
            const int port = IPEndPoint.MinPort - 1;

            try
            {
                new ConnectionInfo(Resources.HOST, port, Resources.USERNAME, ProxyTypes.None, Resources.HOST,
                    int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("port", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void Test_ConnectionInfo_Port_Valid()
        {
            var port = new Random().Next(IPEndPoint.MinPort, IPEndPoint.MaxPort);

            var connectionInfo = new ConnectionInfo(Resources.HOST, port, Resources.USERNAME, ProxyTypes.None,
                Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));

            Assert.AreEqual(port, connectionInfo.Port);
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentExceptionhenUsernameIsNull()
        {
            const string username = null;

            try
            {
                new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), username, ProxyTypes.Http, Resources.USERNAME,
                    int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                    new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("username", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentExceptionhenUsernameIsEmpty()
        {
            var username = string.Empty;

            try
            {
                new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), username, ProxyTypes.Http, Resources.USERNAME,
                    int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                    new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("username", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentExceptionhenUsernameContainsOnlyWhitespace()
        {
            const string username = " \t\r";

            try
            {
                new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), username, ProxyTypes.Http, Resources.USERNAME,
                    int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                    new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof (ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("username", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentNullExceptionWhenAuthenticationMethodsIsNull()
        {
            try
            {
                new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST,
                    int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("authenticationMethods", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void ConstructorShouldThrowArgumentNullExceptionWhenAuthenticationMethodsIsZeroLength()
        {
            try
            {
                new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None, Resources.HOST,
                    int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD, new AuthenticationMethod[0]);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("authenticationMethods", ex.ParamName);
            }
        }

        [TestMethod]
        [TestCategory("ConnectionInfo")]
        public void AuthenticateShouldThrowArgumentNullExceptionWhenServiceFactoryIsNull()
        {
            var connectionInfo = new ConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, ProxyTypes.None,
                Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD,
                new KeyboardInteractiveAuthenticationMethod(Resources.USERNAME));
            var session = new Mock<ISession>(MockBehavior.Strict).Object;
            IServiceFactory serviceFactory = null;

            try
            {
                connectionInfo.Authenticate(session, serviceFactory);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("serviceFactory", ex.ParamName);
            }
        }
   }
}