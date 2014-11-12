using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements ARCH4 cipher algorithm
    /// </summary>
    [TestClass]
    public class Arc4CipherTest : TestBase
    {
        /// <summary>
        ///A test for Arc4Cipher Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void Arc4CipherConstructorTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            Arc4Cipher target = new Arc4Cipher(key, true);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        [TestMethod]
        public void Decrypt_DischargeFirstBytes_False1()
        {
            const string key = "Key";
            const string expectedPlainText = "Plaintext";
            var encoding = Encoding.ASCII;
            var cipher = new Arc4Cipher(encoding.GetBytes(key), false);
            var cipherText = new byte[] { 0xBB, 0xF3, 0x16, 0xE8, 0xD9, 0x40, 0xAF, 0x0A, 0xD3 };

            var actualPlainText = cipher.Decrypt(cipherText);

            Assert.AreEqual(expectedPlainText, encoding.GetString(actualPlainText));
        }

        [TestMethod]
        public void Decrypt_DischargeFirstBytes_False2()
        {
            const string key = "Wiki";
            const string expectedPlainText = "pedia";
            var encoding = Encoding.ASCII;
            var cipher = new Arc4Cipher(encoding.GetBytes(key), false);
            var cipherText = new byte[] { 0x10, 0X21, 0xBF, 0x04, 0x20 };

            var actualPlainText = cipher.Decrypt(cipherText);

            Assert.AreEqual(expectedPlainText, encoding.GetString(actualPlainText));
        }

        /// <summary>
        ///A test for DecryptBlock
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void DecryptBlockTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            Arc4Cipher target = new Arc4Cipher(key, true); // TODO: Initialize to an appropriate value
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

        [TestMethod]
        public void Encrypt_DischargeFirstBytes_False1()
        {
            const string key = "Key";
            const string plainText = "Plaintext";
            var encoding = Encoding.ASCII;
            var cipher = new Arc4Cipher(encoding.GetBytes(key), false);
            var expectedCipherText = new byte[] { 0xBB, 0xF3, 0x16, 0xE8, 0xD9, 0x40, 0xAF, 0x0A, 0xD3 };

            var actualCipherText = cipher.Encrypt(encoding.GetBytes(plainText));

            Assert.IsNotNull(actualCipherText);
            Assert.AreEqual(expectedCipherText.Length, actualCipherText.Length);
            Assert.AreEqual(expectedCipherText[0], actualCipherText[0]);
            Assert.AreEqual(expectedCipherText[1], actualCipherText[1]);
            Assert.AreEqual(expectedCipherText[2], actualCipherText[2]);
            Assert.AreEqual(expectedCipherText[3], actualCipherText[3]);
            Assert.AreEqual(expectedCipherText[4], actualCipherText[4]);
            Assert.AreEqual(expectedCipherText[5], actualCipherText[5]);
            Assert.AreEqual(expectedCipherText[6], actualCipherText[6]);
            Assert.AreEqual(expectedCipherText[7], actualCipherText[7]);
            Assert.AreEqual(expectedCipherText[8], actualCipherText[8]);
        }

        [TestMethod]
        public void Encrypt_DischargeFirstBytes_False2()
        {
            const string key = "Wiki";
            const string plainText = "pedia";
            var encoding = Encoding.ASCII;
            var cipher = new Arc4Cipher(encoding.GetBytes(key), false);
            var expectedCipherText = new byte[] { 0x10, 0X21, 0xBF, 0x04, 0x20 };

            var actualCipherText = cipher.Encrypt(encoding.GetBytes(plainText));

            Assert.IsNotNull(actualCipherText);
            Assert.AreEqual(expectedCipherText.Length, actualCipherText.Length);
            Assert.AreEqual(expectedCipherText[0], actualCipherText[0]);
            Assert.AreEqual(expectedCipherText[1], actualCipherText[1]);
            Assert.AreEqual(expectedCipherText[2], actualCipherText[2]);
            Assert.AreEqual(expectedCipherText[3], actualCipherText[3]);
            Assert.AreEqual(expectedCipherText[4], actualCipherText[4]);
        }

        /// <summary>
        ///A test for EncryptBlock
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void EncryptBlockTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            Arc4Cipher target = new Arc4Cipher(key, true); // TODO: Initialize to an appropriate value
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

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        [TestCategory("integration")]
        public void Test_Cipher_Arcfour128_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("arcfour128", new CipherInfo(128, (key, iv) => { return new Arc4Cipher(key, true); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        [TestCategory("integration")]
        public void Test_Cipher_Arcfour256_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("arcfour256", new CipherInfo(256, (key, iv) => { return new Arc4Cipher(key, true); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
        }

    }
}