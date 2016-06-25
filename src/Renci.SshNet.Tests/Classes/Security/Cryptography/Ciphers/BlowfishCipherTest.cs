using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;
using System.Linq;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers
{
    /// <summary>
    ///
    /// </summary>
    [TestClass]
    public class BlowfishCipherTest : TestBase
    {
        [TestMethod]
        public void Test_Cipher_Blowfish_128_CBC()
        {
            var input = new byte[] { 0x00, 0x00, 0x00, 0x2c, 0x1a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0x30, 0x9e, 0xe0, 0x9c, 0x12, 0xee, 0x3a, 0x30, 0x03, 0x52, 0x1c, 0x1a, 0xe7, 0x3e, 0x0b, 0x9a, 0xcf, 0x9a, 0x57, 0x42, 0x0b, 0x4f, 0x4a, 0x15, 0xa0, 0xf5 };
            var key = new byte[] { 0xe4, 0x94, 0xf9, 0xb1, 0x00, 0x4f, 0x16, 0x2a, 0x80, 0x11, 0xea, 0x73, 0x0d, 0xb9, 0xbf, 0x64 };
            var iv = new byte[] { 0x74, 0x8b, 0x4f, 0xe6, 0xc1, 0x29, 0xb3, 0x54, 0xec, 0x77, 0x92, 0xf3, 0x15, 0xa0, 0x41, 0xa8 };
            var output = new byte[] { 0x50, 0x49, 0xe0, 0xce, 0x98, 0x93, 0x8b, 0xec, 0x82, 0x7d, 0x14, 0x1b, 0x3e, 0xdc, 0xca, 0x63, 0xef, 0x36, 0x20, 0x67, 0x58, 0x63, 0x1f, 0x9c, 0xd2, 0x12, 0x6b, 0xca, 0xea, 0xd0, 0x78, 0x8b, 0x61, 0x50, 0x4f, 0xc4, 0x5b, 0x32, 0x91, 0xd6, 0x65, 0xcb, 0x74, 0xe5, 0x6e, 0xf5, 0xde, 0x14 };
            var testCipher = new Renci.SshNet.Security.Cryptography.Ciphers.BlowfishCipher(key, new Renci.SshNet.Security.Cryptography.Ciphers.Modes.CbcCipherMode(iv), null);
            var r = testCipher.Encrypt(input);

            if (!r.SequenceEqual(output))
                Assert.Fail("Invalid encryption");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        [TestCategory("integration")]
        public void Test_Cipher_BlowfishCBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("blowfish-cbc", new CipherInfo(128, (key, iv) => { return new BlowfishCipher(key, new CbcCipherMode(iv), null); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        /// <summary>
        ///A test for BlowfishCipher Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void BlowfishCipherConstructorTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            CipherMode mode = null; // TODO: Initialize to an appropriate value
            CipherPadding padding = null; // TODO: Initialize to an appropriate value
            BlowfishCipher target = new BlowfishCipher(key, mode, padding);
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
            BlowfishCipher target = new BlowfishCipher(key, mode, padding); // TODO: Initialize to an appropriate value
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
            BlowfishCipher target = new BlowfishCipher(key, mode, padding); // TODO: Initialize to an appropriate value
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