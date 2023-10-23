using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for "none" authentication method
    /// </summary>
    [TestClass]
    public class NoneAuthenticationMethodTest : TestBase
    {
        [TestMethod]
        public void None_Test_UsernameIsEmpty()
        {
            NoneAuthenticationMethod nam = null;

            try
            {
                nam = new NoneAuthenticationMethod(string.Empty);
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
                nam?.Dispose();
            }
        }

        [TestMethod]
        public void None_Test_UsernameIsNull()
        {
            NoneAuthenticationMethod nam = null;

            try
            {
                nam = new NoneAuthenticationMethod(username: null);
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
                nam?.Dispose();
            }
        }

        [TestMethod]
        public void None_Test_UsernameIsWhitespace()
        {
            NoneAuthenticationMethod nam = null;

            try
            {
                nam = new NoneAuthenticationMethod("   ");
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
                nam?.Dispose();
            }
        }

        [TestMethod]
        public void Name()
        {
            var username = new Random().Next().ToString(CultureInfo.InvariantCulture);

            using (var target = new NoneAuthenticationMethod(username))
            {
                Assert.AreEqual("none", target.Name);
            }
        }

        [TestMethod]
        public void Username()
        {
            var username = new Random().Next().ToString(CultureInfo.InvariantCulture);

            using (var target = new NoneAuthenticationMethod(username))
            {
                Assert.AreSame(username, target.Username);
            }
        }
    }
}
