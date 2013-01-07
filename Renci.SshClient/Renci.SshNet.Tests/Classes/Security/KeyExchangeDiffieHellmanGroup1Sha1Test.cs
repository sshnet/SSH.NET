using Renci.SshNet.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for KeyExchangeDiffieHellmanGroup1Sha1Test and is intended
    ///to contain all KeyExchangeDiffieHellmanGroup1Sha1Test Unit Tests
    ///</summary>
    [TestClass()]
    public class KeyExchangeDiffieHellmanGroup1Sha1Test : TestBase
    {
        /// <summary>
        ///A test for KeyExchangeDiffieHellmanGroup1Sha1 Constructor
        ///</summary>
        [TestMethod()]
        public void KeyExchangeDiffieHellmanGroup1Sha1ConstructorTest()
        {
            KeyExchangeDiffieHellmanGroup1Sha1 target = new KeyExchangeDiffieHellmanGroup1Sha1();
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for GroupPrime
        ///</summary>
        [TestMethod()]
        public void GroupPrimeTest()
        {
            KeyExchangeDiffieHellmanGroup1Sha1 target = new KeyExchangeDiffieHellmanGroup1Sha1(); // TODO: Initialize to an appropriate value
            BigInteger actual;
            actual = target.GroupPrime;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            KeyExchangeDiffieHellmanGroup1Sha1 target = new KeyExchangeDiffieHellmanGroup1Sha1(); // TODO: Initialize to an appropriate value
            string actual;
            actual = target.Name;
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
