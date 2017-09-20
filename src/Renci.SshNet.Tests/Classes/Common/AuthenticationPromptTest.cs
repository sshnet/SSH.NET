using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for AuthenticationPromptTest and is intended
    ///to contain all AuthenticationPromptTest Unit Tests
    ///</summary>
    [TestClass]
    public class AuthenticationPromptTest : TestBase
    {
        /// <summary>
        ///A test for AuthenticationPrompt Constructor
        ///</summary>
        [TestMethod]
        public void AuthenticationPromptConstructorTest()
        {
            int id = 0; // TODO: Initialize to an appropriate value
            bool isEchoed = false; // TODO: Initialize to an appropriate value
            string request = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationPrompt target = new AuthenticationPrompt(id, isEchoed, request);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Response
        ///</summary>
        [TestMethod]
        public void ResponseTest()
        {
            int id = 0; // TODO: Initialize to an appropriate value
            bool isEchoed = false; // TODO: Initialize to an appropriate value
            string request = string.Empty; // TODO: Initialize to an appropriate value
            AuthenticationPrompt target = new AuthenticationPrompt(id, isEchoed, request); // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            target.Response = expected;
            actual = target.Response;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
