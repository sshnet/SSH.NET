using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
{
    /// <summary>
    ///This is a test class for CipherTest and is intended
    ///to contain all CipherTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CipherTest : TestBase
    {
        internal virtual Cipher CreateCipher()
        {
            // TODO: Instantiate an appropriate concrete class.
            Cipher target = null;
            return target;
        }

        /// <summary>
        ///A test for Decrypt
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DecryptTest()
        {
            Cipher target = CreateCipher(); // TODO: Initialize to an appropriate value
            byte[] input = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Decrypt(input);
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
            Cipher target = CreateCipher(); // TODO: Initialize to an appropriate value
            byte[] input = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Encrypt(input);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
