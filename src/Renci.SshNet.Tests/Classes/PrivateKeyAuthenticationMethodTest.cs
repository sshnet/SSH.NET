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
        [TestCategory("PrivateKeyAuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PrivateKeyAuthenticationMethod: Pass null as username, null as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void PrivateKey_Test_Pass_Null()
        {
            new PrivateKeyAuthenticationMethod(null, null);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [TestCategory("PrivateKeyAuthenticationMethod")]
        [Owner("olegkap")]
        [Description("PrivateKeyAuthenticationMethod: Pass valid username, null as password.")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PrivateKey_Test_Pass_PrivateKey_Null()
        {
            new PrivateKeyAuthenticationMethod("username", null);
        }

        [TestMethod]
        [TestCategory("AuthenticationMethod")]
        [TestCategory("PrivateKeyAuthenticationMethod")]
        [Owner("Kenneth_aa")]
        [Description("PrivateKeyAuthenticationMethod: Pass String.Empty as username, null as password.")]
        [ExpectedException(typeof(ArgumentException))]
        public void PrivateKey_Test_Pass_Whitespace()
        {
            new PrivateKeyAuthenticationMethod(string.Empty, null);
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void NameTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyAuthenticationMethod target = new PrivateKeyAuthenticationMethod(username, keyFiles); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Name;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DisposeTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyAuthenticationMethod target = new PrivateKeyAuthenticationMethod(username, keyFiles); // TODO: Initialize to an appropriate value
            target.Dispose();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Authenticate
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AuthenticateTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyAuthenticationMethod target = new PrivateKeyAuthenticationMethod(username, keyFiles); // TODO: Initialize to an appropriate value
            Session session = null; // TODO: Initialize to an appropriate value
            AuthenticationResult expected = new AuthenticationResult(); // TODO: Initialize to an appropriate value
            AuthenticationResult actual;
            actual = target.Authenticate(session);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for PrivateKeyAuthenticationMethod Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void PrivateKeyAuthenticationMethodConstructorTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            PrivateKeyFile[] keyFiles = null; // TODO: Initialize to an appropriate value
            PrivateKeyAuthenticationMethod target = new PrivateKeyAuthenticationMethod(username, keyFiles);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}