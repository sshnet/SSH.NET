using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers
{
    /// <summary>
    ///This is a test class for CipherModeTest and is intended
    ///to contain all CipherModeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CipherModeTest : TestBase
    {
        internal virtual CipherMode CreateCipherMode()
        {
            // TODO: Instantiate an appropriate concrete class.
            CipherMode target = null;
            return target;
        }

        /// <summary>
        ///A test for DecryptBlock
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DecryptBlockTest()
        {
            CipherMode target = CreateCipherMode(); // TODO: Initialize to an appropriate value
            byte[] inputBuffer = null; // TODO: Initialize to an appropriate value
            int inputOffset = 0; // TODO: Initialize to an appropriate value
            int inputCount = 0; // TODO: Initialize to an appropriate value
            byte[] outputBuffer = null; // TODO: Initialize to an appropriate value
            int outputOffset = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.DecryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
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
            CipherMode target = CreateCipherMode(); // TODO: Initialize to an appropriate value
            byte[] inputBuffer = null; // TODO: Initialize to an appropriate value
            int inputOffset = 0; // TODO: Initialize to an appropriate value
            int inputCount = 0; // TODO: Initialize to an appropriate value
            byte[] outputBuffer = null; // TODO: Initialize to an appropriate value
            int outputOffset = 0; // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            int actual;
            actual = target.EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
