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
    internal class NoneAuthenticationMethodTest : TestBase
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
    }
}
