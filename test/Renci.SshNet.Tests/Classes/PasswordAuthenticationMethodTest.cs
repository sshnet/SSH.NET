using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to perform password authentication.
    /// </summary>
    [TestClass]
    public partial class PasswordAuthenticationMethodTest : TestBase
    {
        [TestMethod]
        public void Password_Test_UsernameIsEmpty()
        {
            PasswordAuthenticationMethod pam = null;

            try
            {
                pam = new PasswordAuthenticationMethod(username: string.Empty, password: "valid");
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
                pam?.Dispose();
            }
        }

        [TestMethod]
        public void Password_Test_UsernameIsNull()
        {
            PasswordAuthenticationMethod pam = null;

            try
            {
                pam = new PasswordAuthenticationMethod(username: null, password: "valid");
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
                pam?.Dispose();
            }
        }

        [TestMethod]
        public void Password_Test_UsernameIsWhitespace()
        {
            PasswordAuthenticationMethod pam = null;

            try
            {
                pam = new PasswordAuthenticationMethod(username: "   ", password: "valid");
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
                pam?.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass \"valid\" as username, null as password.")]
        public void Password_Test_Pass_Null_Password()
        {
            PasswordAuthenticationMethod pam = null;

            try
            {
                pam = new PasswordAuthenticationMethod("valid", password: (string) null);
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
                pam?.Dispose();
            }
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass \"valid\" as username, \"valid\" as password.")]
        public void Password_Test_Pass_Valid_Username_And_Password()
        {
            var pam = new PasswordAuthenticationMethod("valid", "valid");
            pam.Dispose();
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass \"valid\" as username, String.Empty as password.")]
        public void Password_Test_Pass_Valid()
        {
            var pam = new PasswordAuthenticationMethod("valid", string.Empty);
            pam.Dispose();
        }
    }
}
