using Renci.SshNet.Messages.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Messages.Authentication
{
    /// <summary>
    ///This is a test class for FailureMessageTest and is intended
    ///to contain all FailureMessageTest Unit Tests
    ///</summary>
    [TestClass]
    public class FailureMessageTest : TestBase
    {
        /// <summary>
        ///A test for FailureMessage Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void FailureMessageConstructorTest()
        {
            FailureMessage target = new FailureMessage();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for AllowedAuthentications
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder
        public void AllowedAuthenticationsTest()
        {
            FailureMessage target = new FailureMessage(); // TODO: Initialize to an appropriate value
            string[] expected = null; // TODO: Initialize to an appropriate value
            string[] actual;
            target.AllowedAuthentications = expected;
            actual = target.AllowedAuthentications;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
