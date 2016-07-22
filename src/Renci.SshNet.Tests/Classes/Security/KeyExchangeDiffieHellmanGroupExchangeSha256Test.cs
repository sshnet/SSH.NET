using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Messages.Transport;
using Renci.SshNet.Security;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security
{
    /// <summary>
    ///This is a test class for KeyExchangeDiffieHellmanGroupExchangeSha256Test and is intended
    ///to contain all KeyExchangeDiffieHellmanGroupExchangeSha256Test Unit Tests
    ///</summary>
    [TestClass]
    public class KeyExchangeDiffieHellmanGroupExchangeSha256Test : TestBase
    {
        /// <summary>
        ///A test for KeyExchangeDiffieHellmanGroupExchangeSha256 Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void KeyExchangeDiffieHellmanGroupExchangeSha256ConstructorTest()
        {
            KeyExchangeDiffieHellmanGroupExchangeSha256 target = new KeyExchangeDiffieHellmanGroupExchangeSha256();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Finish
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void FinishTest()
        {
            KeyExchangeDiffieHellmanGroupExchangeSha256 target = new KeyExchangeDiffieHellmanGroupExchangeSha256(); // TODO: Initialize to an appropriate value
            target.Finish();
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Start
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void StartTest()
        {
            KeyExchangeDiffieHellmanGroupExchangeSha256 target = new KeyExchangeDiffieHellmanGroupExchangeSha256(); // TODO: Initialize to an appropriate value
            Session session = null; // TODO: Initialize to an appropriate value
            KeyExchangeInitMessage message = null; // TODO: Initialize to an appropriate value
            target.Start(session, message);
            Assert.Inconclusive("A method that does not return a value cannot be verified.");
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void NameTest()
        {
            KeyExchangeDiffieHellmanGroupExchangeSha256 target = new KeyExchangeDiffieHellmanGroupExchangeSha256(); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Name;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
