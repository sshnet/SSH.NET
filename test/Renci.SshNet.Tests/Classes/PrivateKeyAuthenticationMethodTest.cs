using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to perform private key authentication.
    /// </summary>
    [TestClass]
    public class PrivateKeyAuthenticationMethodTest : TestBase
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrivateKey_Test_Pass_Null()
        {
            new PrivateKeyAuthenticationMethod(null, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrivateKey_Test_Pass_PrivateKey_Null()
        {
            new PrivateKeyAuthenticationMethod("username", null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void PrivateKey_Test_Pass_Whitespace()
        {
            new PrivateKeyAuthenticationMethod(string.Empty, null);
        }
    }
}
