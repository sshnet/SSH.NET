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
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass null as username, \"valid\" as password.")]
        public void Password_Test_Pass_Null_Username()
        {
            PasswordAuthenticationMethod pam = null;

            try
            {
                pam = new PasswordAuthenticationMethod(null, "valid");
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("X", ex.ParamName);
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
                Assert.AreEqual("X", ex.ParamName);
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
        [Description("PasswordAuthenticationMethod: Pass String.Empty as username, \"valid\" as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Password_Test_Pass_Whitespace()
        {
            PasswordAuthenticationMethod pam = null;

            try
            {
                pam = new PasswordAuthenticationMethod(string.Empty, "valid");
                Assert.Fail();
            }
            catch (ArgumentException ex)
            {
                Assert.AreEqual(typeof(ArgumentException), ex.GetType());
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("X", ex.ParamName);
            }
            finally
            {
                pam?.Dispose();
            }
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
