using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;

namespace Renci.SshNet.Tests
{
    [TestClass]
    public class AuthenticationMethodDerivativesTest
    {
        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("KeyboardInteractiveAuthenticationMethod: Pass null as username.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Keyboard_Test_Pass_Null()
        {
            new KeyboardInteractiveAuthenticationMethod(null);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("KeyboardInteractiveAuthenticationMethod: Pass String.Empty as username.")]
        [ExpectedException(typeof(ArgumentException))]
        public void Keyboard_Test_Pass_Whitespace()
        {
            new KeyboardInteractiveAuthenticationMethod(string.Empty);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("NoneAuthenticationMethod: Pass null as username.")]
        [ExpectedException(typeof(ArgumentException))]
        public void None_Test_Pass_Null()
        {
            new NoneAuthenticationMethod(null);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("NoneAuthenticationMethod: Pass String.Empty as username.")]
        [ExpectedException(typeof(ArgumentException))]
        public void None_Test_Pass_Whitespace()
        {
            new NoneAuthenticationMethod(string.Empty);
        }

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

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PrivateKeyAuthenticationMethod: Pass null as username, null as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void PrivateKey_Test_Pass_Null()
        {
            new PrivateKeyAuthenticationMethod(null, null);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PrivateKeyAuthenticationMethod: Pass String.Empty as username, null as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void PrivateKey_Test_Pass_Whitespace()
        {
            new PrivateKeyAuthenticationMethod(string.Empty, null);
        }
    }
}
