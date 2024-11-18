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
        [ExpectedException(typeof(ArgumentNullException))]
        public void Password_Test_Pass_Null_Username()
        {
            new PasswordAuthenticationMethod(null, "valid");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Password_Test_Pass_Null_Password()
        {
            new PasswordAuthenticationMethod("valid", (string)null);
        }

        [TestMethod]
        public void Password_Test_Pass_Valid_Username_And_Password()
        {
            new PasswordAuthenticationMethod("valid", "valid");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Password_Test_Pass_Whitespace()
        {
            new PasswordAuthenticationMethod(string.Empty, "valid");
        }

        [TestMethod]
        public void Password_Test_Pass_Valid()
        {
            new PasswordAuthenticationMethod("valid", string.Empty);
        }
    }
}
