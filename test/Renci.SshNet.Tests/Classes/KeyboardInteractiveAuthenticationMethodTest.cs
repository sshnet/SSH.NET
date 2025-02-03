using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// Provides functionality to perform keyboard interactive authentication.
    /// </summary>
    [TestClass]
    public partial class KeyboardInteractiveAuthenticationMethodTest : TestBase
    {
        [TestMethod]
        public void Keyboard_Test_Pass_Null()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new KeyboardInteractiveAuthenticationMethod(null));
        }

        [TestMethod]
        public void Keyboard_Test_Pass_Whitespace()
        {
            Assert.ThrowsException<ArgumentException>(() => new KeyboardInteractiveAuthenticationMethod(string.Empty));
        }
    }
}
