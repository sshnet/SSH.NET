using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements RSA cipher algorithm.
    /// </summary>
    [TestClass]
    public class RsaCipherTest : TestBase
    {
        /// <summary>
        ///A test for RsaCipher Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void RsaCipherConstructorTest()
        {
            RsaKey key = null; // TODO: Initialize to an appropriate value
            RsaCipher target = new RsaCipher(key);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Decrypt
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DecryptTest()
        {
            RsaKey key = null; // TODO: Initialize to an appropriate value
            RsaCipher target = new RsaCipher(key); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Decrypt(data);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for Encrypt
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void EncryptTest()
        {
            RsaKey key = null; // TODO: Initialize to an appropriate value
            RsaCipher target = new RsaCipher(key); // TODO: Initialize to an appropriate value
            byte[] data = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Encrypt(data);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}