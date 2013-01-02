using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to perform private key authentication.
    /// </summary>
    [TestClass]
    public class PrivateKeyAuthenticationMethodTest : TestBase
    {
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