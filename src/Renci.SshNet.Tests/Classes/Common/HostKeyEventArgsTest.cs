using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Common
{
    /// <summary>
    ///This is a test class for HostKeyEventArgsTest and is intended
    ///to contain all HostKeyEventArgsTest Unit Tests
    ///</summary>
    [TestClass]
    [Ignore] // placeholder for actual test
    public class HostKeyEventArgsTest : TestBase
    {
        /// <summary>
        ///A test for HostKeyEventArgs Constructor
        ///</summary>
        [TestMethod]
        public void HostKeyEventArgsConstructorTest()
        {
            KeyHostAlgorithm host = null; // TODO: Initialize to an appropriate value
            HostKeyEventArgs target = new HostKeyEventArgs(host);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for CanTrust
        ///</summary>
        [TestMethod]
        public void CanTrustTest()
        {
            KeyHostAlgorithm host = null; // TODO: Initialize to an appropriate value
            HostKeyEventArgs target = new HostKeyEventArgs(host); // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            target.CanTrust = expected;
            actual = target.CanTrust;
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
