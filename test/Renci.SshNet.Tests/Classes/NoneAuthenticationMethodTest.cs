using System;
using System.Globalization;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality for "none" authentication method
    /// </summary>
    [TestClass]
    public class NoneAuthenticationMethodTest : TestBase
    {
        [TestMethod]
        public void None_Test_Pass_Null()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new NoneAuthenticationMethod(null));
        }

        [TestMethod]
        public void None_Test_Pass_Whitespace()
        {
            Assert.ThrowsExactly<ArgumentException>(() => new NoneAuthenticationMethod(string.Empty));
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
