using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System.Linq;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements 3DES cipher algorithm.
    /// </summary>
    [TestClass]
    public class TripleDesCipherTest : TestBase
    {
        [TestMethod]
        public void Test_Cipher_3DES_CBC()
        {
            var input = new byte[] { 0x00, 0x00, 0x00, 0x1c, 0x0a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0x72, 0x4e, 0x06, 0x08, 0x28, 0x2d, 0xaa, 0xe2, 0xb3, 0xd9 };
            var key = new byte[] { 0x78, 0xf6, 0xc6, 0xbb, 0x57, 0x03, 0x69, 0xca, 0xba, 0x31, 0x18, 0x2f, 0x2f, 0x4c, 0x35, 0x34, 0x64, 0x06, 0x85, 0x30, 0xbe, 0x78, 0x60, 0xb3 };
            var iv = new byte[] { 0xc0, 0x75, 0xf2, 0x26, 0x0a, 0x2a, 0x42, 0x96 };
            var output = new byte[] { 0x28, 0x77, 0x2f, 0x07, 0x3e, 0xc2, 0x27, 0xa6, 0xdb, 0x36, 0x4d, 0xc6, 0x7a, 0x26, 0x7a, 0x38, 0xe6, 0x54, 0x0b, 0xab, 0x07, 0x87, 0xf0, 0xa4, 0x73, 0x1f, 0xde, 0xe6, 0x81, 0x1d, 0x4b, 0x4b };
            var testCipher = new TripleDesCipher(key, new CbcCipherMode(iv), null);
            var r = testCipher.Encrypt(input);

            if (!r.SequenceEqual(output))
                Assert.Fail("Invalid encryption");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        [TestCategory("integration")]
        public void Test_Cipher_TripleDESCBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("3des-cbc", new CipherInfo(192, (key, iv) => { return new TripleDesCipher(key, new CbcCipherMode(iv), null); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }
        /// <summary>
        ///A test for TripleDesCipher Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void TripleDesCipherConstructorTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            CipherMode mode = null; // TODO: Initialize to an appropriate value
            CipherPadding padding = null; // TODO: Initialize to an appropriate value
            TripleDesCipher target = new TripleDesCipher(key, mode, padding);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for DecryptBlock
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DecryptBlockTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            CipherMode mode = null; // TODO: Initialize to an appropriate value
            CipherPadding padding = null; // TODO: Initialize to an appropriate value
            TripleDesCipher target = new TripleDesCipher(key, mode, padding); // TODO: Initialize to an appropriate value
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
            byte[] key = null; // TODO: Initialize to an appropriate value
            CipherMode mode = null; // TODO: Initialize to an appropriate value
            CipherPadding padding = null; // TODO: Initialize to an appropriate value
            TripleDesCipher target = new TripleDesCipher(key, mode, padding); // TODO: Initialize to an appropriate value
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