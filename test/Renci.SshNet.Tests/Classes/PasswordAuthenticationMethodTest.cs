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
        [ExpectedException(typeof(ArgumentException))]
        public void Password_Test_Pass_Null_Username()
        {
            new PasswordAuthenticationMethod(null, "valid");
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass \"valid\" as username, null as password.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Password_Test_Pass_Null_Password()
        {
            new PasswordAuthenticationMethod("valid", (string)null);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass \"valid\" as username, \"valid\" as password.")]
        public void Password_Test_Pass_Valid_Username_And_Password()
        {
            new PasswordAuthenticationMethod("valid", "valid");
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass String.Empty as username, \"valid\" as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Password_Test_Pass_Whitespace()
        {
            new PasswordAuthenticationMethod(string.Empty, "valid");
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PasswordAuthenticationMethod: Pass \"valid\" as username, String.Empty as password.")]
        public void Password_Test_Pass_Valid()
        {
            new PasswordAuthenticationMethod("valid", string.Empty);
        }
    }
}
