using System;
using System.Globalization;
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
        [WorkItem(703)]
        [TestMethod]
        public void Test_ConnectionInfo_Host_Is_Null()
        {
            PasswordConnectionInfo connectionInfo = null;

            try
            {
                connectionInfo = new PasswordConnectionInfo(host: null, Resources.USERNAME, Resources.PASSWORD);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("host", ex.ParamName);
            }
            finally
            {
                connectionInfo?.Dispose();
            }
        }

        [TestMethod]
        public void Test_ConnectionInfo_UsernameIsEmpty()
        {
            PasswordConnectionInfo connectionInfo = null;

            try
            {
                connectionInfo = new PasswordConnectionInfo(Resources.HOST, username: string.Empty, Resources.PASSWORD);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals("Cannot be null or only whitespace.", ex);
                Assert.AreEqual("username", ex.ParamName);
            }
            finally
            {
                connectionInfo?.Dispose();
            }
        }

        [TestMethod]
        public void Test_ConnectionInfo_UsernameIsNull()
        {
            PasswordConnectionInfo connectionInfo = null;

            try
            {
                connectionInfo = new PasswordConnectionInfo(Resources.HOST, username: null, Resources.PASSWORD);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals("Cannot be null or only whitespace.", ex);
                Assert.AreEqual("username", ex.ParamName);
            }
            finally
            {
                connectionInfo?.Dispose();
            }
        }

        [TestMethod]
        public void Test_ConnectionInfo_UsernameIsWhitespace()
        {
            PasswordConnectionInfo connectionInfo = null;

            try
            {
                connectionInfo = new PasswordConnectionInfo(Resources.HOST, username: "  ", Resources.PASSWORD);
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals("Cannot be null or only whitespace.", ex);
                Assert.AreEqual("username", ex.ParamName);
            }
            finally
            {
                connectionInfo?.Dispose();
            }
        }

        [WorkItem(703)]
        [TestMethod]
        public void Test_ConnectionInfo_Password_Is_Null()
        {
            PasswordConnectionInfo connectionInfo = null;

            try
            {
                connectionInfo = new PasswordConnectionInfo(Resources.HOST, Resources.USERNAME, (string) null);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.AreEqual(typeof(ArgumentNullException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("s", ex.ParamName);
            }
            finally
            {
                connectionInfo?.Dispose();
            }
        }

        [WorkItem(703)]
        [TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        public void Test_ConnectionInfo_SmallPortNumber()
        {
            PasswordConnectionInfo connectionInfo = null;

            try
            {
                connectionInfo = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MinPort - 1, Resources.USERNAME, Resources.PASSWORD);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals(string.Format(CultureInfo.InvariantCulture, "Specified value cannot be less than {0}.", IPEndPoint.MinPort), ex);
                Assert.AreEqual("port", ex.ParamName);
            }
            finally
            {
                connectionInfo?.Dispose();
            }
        }

        [WorkItem(703)]
        [TestMethod]
        [TestCategory("PasswordConnectionInfo")]
        public void Test_ConnectionInfo_BigPortNumber()
        {
            PasswordConnectionInfo connectionInfo = null;

            try
            {
                connectionInfo = new PasswordConnectionInfo(Resources.HOST, IPEndPoint.MaxPort + 1, Resources.USERNAME, Resources.PASSWORD);
                Assert.Fail();
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                ArgumentExceptionAssert.MessageEquals(string.Format(CultureInfo.InvariantCulture, "Specified value cannot be greater than {0}.", IPEndPoint.MaxPort), ex);
                Assert.AreEqual("port", ex.ParamName);
            }
            finally
            {
                connectionInfo?.Dispose();
            }
        }
    }
}
