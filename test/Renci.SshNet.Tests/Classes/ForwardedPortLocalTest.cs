using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for local port forwarding
    /// </summary>
    [TestClass]
    public partial class ForwardedPortLocalTest : TestBase
    {
        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenBoundHostIsNull()
        {
            ForwardedPortLocal target = null;

            try
            {
                target = new ForwardedPortLocal(null, 8080, Resources.HOST, 80);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(typeof(ArgumentNullException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("boundHost", ex.ParamName);
            }
            finally
            {
                target?.Dispose();
            }
        }

        [TestMethod]
        public void ConstructorShouldNotThrowExceptionWhenBoundHostIsEmpty()
        {
            var boundHost = string.Empty;

            using (var forwardedPort = new ForwardedPortLocal(boundHost, 8080, Resources.HOST, 80))
            {
                Assert.AreSame(boundHost, forwardedPort.BoundHost);
            }
        }

        [TestMethod]
        public void ConstructorShouldNotThrowExceptionWhenBoundHostIsInvalidDnsName()
        {
            const string boundHost = "in_valid_host.";

            using (var forwardedPort = new ForwardedPortLocal(boundHost, 8080, Resources.HOST, 80))
            {
                Assert.AreSame(boundHost, forwardedPort.BoundHost);
            }
        }

        [TestMethod]
        public void ConstructorShouldThrowArgumentNullExceptionWhenHostIsNull()
        {
            ForwardedPortLocal target = null;

            try
            {
                target = new ForwardedPortLocal(Resources.HOST, 8080, null, 80);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(typeof(ArgumentNullException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("host", ex.ParamName);
            }
            finally
            {
                target?.Dispose();
            }
        }

        [TestMethod]
        public void ConstructorShouldNotThrowExceptionWhenHostIsEmpty()
        {
            var host = string.Empty;

            using (var forwardedPort = new ForwardedPortLocal(Resources.HOST, 8080, string.Empty, 80))
            {
                Assert.AreSame(host, forwardedPort.Host);
            }
        }

        [TestMethod]
        public void ConstructorShouldNotThrowExceptionWhenHostIsInvalidDnsName()
        {
            const string host = "in_valid_host.";

            using (var forwardedPort = new ForwardedPortLocal(Resources.HOST, 8080, host, 80))
            {
                Assert.AreSame(host, forwardedPort.Host);
            }
        }
    }
}
