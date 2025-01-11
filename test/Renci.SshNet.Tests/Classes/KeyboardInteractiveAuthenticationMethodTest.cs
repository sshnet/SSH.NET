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
        [ExpectedException(typeof(ArgumentNullException))]
        public void Keyboard_Test_Pass_Null()
        {
            new KeyboardInteractiveAuthenticationMethod(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Keyboard_Test_Pass_Whitespace()
        {
            new KeyboardInteractiveAuthenticationMethod(string.Empty);
        }
    }
}
