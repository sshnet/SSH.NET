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
        public void Password_Test_Pass_Null_Username()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new PasswordAuthenticationMethod(null, "valid"));
        }

        [TestMethod]
        public void Password_Test_Pass_Null_Password()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new PasswordAuthenticationMethod("valid", (string)null));
        }

        [TestMethod]
        public void Password_Test_Pass_Valid_Username_And_Password()
        {
            new PasswordAuthenticationMethod("valid", "valid");
        }

        [TestMethod]
        public void Password_Test_Pass_Whitespace()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new PasswordAuthenticationMethod(string.Empty, "valid"));
        }

        [TestMethod]
        public void Password_Test_Pass_Valid()
        {
            new PasswordAuthenticationMethod("valid", string.Empty);
        }
    }
}
