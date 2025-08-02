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
    public class PasswordConnectionInfoTest : TestBase
    {
        [TestMethod]
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

        [TestMethod]
        public void Test_ConnectionInfo_Username_Is_Null()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new PasswordConnectionInfo(Resources.HOST, null, Resources.PASSWORD));
        }

        [TestMethod]
        public void Test_ConnectionInfo_Password_Is_Null()
        {
            Assert.ThrowsExactly<ArgumentNullException>(
                () => new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, (string)null));
        }

        [TestMethod]
        public void Test_ConnectionInfo_Username_Is_Whitespace()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new PasswordConnectionInfo(Resources.HOST, " ", Resources.PASSWORD));
        }

        [TestMethod]
        public void Test_ConnectionInfo_SmallPortNumber()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MinPort - 1, Resources.USERNAME, Resources.PASSWORD));
        }

        [TestMethod]
        public void Test_ConnectionInfo_BigPortNumber()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MaxPort + 1, Resources.USERNAME, Resources.PASSWORD));
        }
    }
}
