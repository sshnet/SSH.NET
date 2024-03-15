using System;
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides connection information when password authentication method is used
    /// </summary>
    [TestClass]
    internal class PasswordConnectionInfoTest : TestBase
    {
        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        public void Test_ConnectionInfo_Host_Is_Null()
        {
            try
            {
                _ = new PasswordConnectionInfo(null, Resources.USERNAME, Resources.PASSWORD);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("host", ex.ParamName);
            }

        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Username_Is_Null()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, null, Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test_ConnectionInfo_Password_Is_Null()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, (string)null);
        }

        [TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [Description("Test passing whitespace to username parameter.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_ConnectionInfo_Username_Is_Whitespace()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, " ", Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_SmallPortNumber()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MinPort - 1, Resources.USERNAME, Resources.PASSWORD);
        }

        [WorkItem(703), TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Test_ConnectionInfo_BigPortNumber()
        {
            _ = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MaxPort + 1, Resources.USERNAME, Resources.PASSWORD);
        }
    }
}
