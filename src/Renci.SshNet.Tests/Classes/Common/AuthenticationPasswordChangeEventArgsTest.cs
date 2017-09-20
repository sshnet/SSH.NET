using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    
    
    /// <summary>
    ///This is a test class for AuthenticationPasswordChangeEventArgsTest and is intended
    ///to contain all AuthenticationPasswordChangeEventArgsTest Unit Tests
    ///</summary>
    [TestClass]
    public class AuthenticationPasswordChangeEventArgsTest : TestBase
    {
        /// <summary>
        ///A test for AuthenticationPasswordChangeEventArgs Constructor
        ///</summary>
        [TestMethod]
        public void AuthenticationPasswordChangeEventArgsConstructorTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationPasswordChangeEventArgs target = new AuthenticationPasswordChangeEventArgs(username);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for NewPassword
        ///</summary>
        [TestMethod]
        public void NewPasswordTest()
        {
            string username = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationPasswordChangeEventArgs target = new AuthenticationPasswordChangeEventArgs(username); // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            target.NewPassword = expected;
            actual = target.NewPassword;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
