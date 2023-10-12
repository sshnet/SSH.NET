using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Tests.Common;
using System;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for "none" authentication method
    /// </summary>
    [TestClass]
    public class NoneAuthenticationMethodTest : TestBase
    {
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
        public void Name()
        {
            var username = new Random().Next().ToString(CultureInfo.InvariantCulture);
            var target = new NoneAuthenticationMethod(username);

            Assert.AreEqual("none", target.Name);
        }

        [TestMethod]
        public void Username()
        {
            var username = new Random().Next().ToString(CultureInfo.InvariantCulture);
            var target = new NoneAuthenticationMethod(username);

            Assert.AreSame(username, target.Username);
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DisposeTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            NoneAuthenticationMethod target = new NoneAuthenticationMethod(username); // TODO: Initialize to an appropriate value
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
            NoneAuthenticationMethod target = new NoneAuthenticationMethod(username); // TODO: Initialize to an appropriate value
            Session session = null; // TODO: Initialize to an appropriate value
            AuthenticationResult expected = new AuthenticationResult(); // TODO: Initialize to an appropriate value
            AuthenticationResult actual;
            actual = target.Authenticate(session);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for NoneAuthenticationMethod Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void NoneAuthenticationMethodConstructorTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            NoneAuthenticationMethod target = new NoneAuthenticationMethod(username);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }
    }
}