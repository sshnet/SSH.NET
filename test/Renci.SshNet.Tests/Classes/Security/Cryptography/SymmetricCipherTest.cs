using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests
{
    /// <summary>
    ///This is a test class for SymmetricCipherTest and is intended
    ///to contain all SymmetricCipherTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SymmetricCipherTest : TestBase
    {
        internal virtual SymmetricCipher CreateSymmetricCipher()
        {
            // TODO: Instantiate an appropriate concrete class.
            SymmetricCipher target = null;
            return target;
        }

        /// <summary>
        ///A test for DecryptBlock
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DecryptBlockTest()
        {
            var target = CreateSymmetricCipher(); // TODO: Initialize to an appropriate value
            byte[] inputBuffer = null; // TODO: Initialize to an appropriate value
            var inputOffset = 0; // TODO: Initialize to an appropriate value
            var inputCount = 0; // TODO: Initialize to an appropriate value
            byte[] outputBuffer = null; // TODO: Initialize to an appropriate value
            var outputOffset = 0; // TODO: Initialize to an appropriate value
            var expected = 0; // TODO: Initialize to an appropriate value
            var actual = target.DecryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for EncryptBlock
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void EncryptBlockTest()
        {
            var target = CreateSymmetricCipher(); // TODO: Initialize to an appropriate value
            byte[] inputBuffer = null; // TODO: Initialize to an appropriate value
            var inputOffset = 0; // TODO: Initialize to an appropriate value
            var inputCount = 0; // TODO: Initialize to an appropriate value
            byte[] outputBuffer = null; // TODO: Initialize to an appropriate value
            var outputOffset = 0; // TODO: Initialize to an appropriate value
            var expected = 0; // TODO: Initialize to an appropriate value
            var actual = target.EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
