using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to perform keyboard interactive authentication.
    /// </summary>
    [TestClass]
    public partial class KeyboardInteractiveAuthenticationMethodTest : TestBase
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

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void NameTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            KeyboardInteractiveAuthenticationMethod target = new KeyboardInteractiveAuthenticationMethod(username); // TODO: Initialize to an appropriate value
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
            KeyboardInteractiveAuthenticationMethod target = new KeyboardInteractiveAuthenticationMethod(username); // TODO: Initialize to an appropriate value
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
            KeyboardInteractiveAuthenticationMethod target = new KeyboardInteractiveAuthenticationMethod(username); // TODO: Initialize to an appropriate value
            Session session = null; // TODO: Initialize to an appropriate value
            AuthenticationResult expected = new AuthenticationResult(); // TODO: Initialize to an appropriate value
            AuthenticationResult actual;
            actual = target.Authenticate(session);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for KeyboardInteractiveAuthenticationMethod Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyboardInteractiveAuthenticationMethodConstructorTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            KeyboardInteractiveAuthenticationMethod target = new KeyboardInteractiveAuthenticationMethod(username);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}