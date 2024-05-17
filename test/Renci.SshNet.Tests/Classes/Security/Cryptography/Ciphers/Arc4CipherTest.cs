using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements ARCH4 cipher algorithm
    /// </summary>
    [TestClass]
    public class Arc4CipherTest : TestBase
    {
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

        [TestMethod]
        public void Decrypt_InputAndOffsetAndLength()
        {
            const string key = "Key";
            const string expectedPlainText = "Plaintext";
            var encoding = Encoding.ASCII;
            var cipher = new Arc4Cipher(encoding.GetBytes(key), false);
            var cipherText = new byte[] { 0x0A, 0x0f, 0xBB, 0xF3, 0x16, 0xE8, 0xD9, 0x40, 0xAF, 0x0A, 0xD3, 0x0d, 0x05 };

            var actualPlainText = cipher.Decrypt(cipherText, 2, cipherText.Length - 4);

            Assert.AreEqual(expectedPlainText, encoding.GetString(actualPlainText));
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
            Assert.IsTrue(expectedCipherText.IsEqualTo(actualCipherText));
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
            Assert.IsTrue(expectedCipherText.IsEqualTo(actualCipherText));
        }

        [TestMethod]
        public void Encrypt_InputAndOffsetAndLength()
        {
            const string key = "Wiki";
            const string plainText = "NOpediaNO";
            var encoding = Encoding.ASCII;
            var cipher = new Arc4Cipher(encoding.GetBytes(key), false);
            var plainTextBytes = encoding.GetBytes(plainText);
            var expectedCipherText = new byte[] { 0x10, 0X21, 0xBF, 0x04, 0x20 };

            var actualCipherText = cipher.Encrypt(plainTextBytes, 2, plainTextBytes.Length - 4);

            Assert.IsNotNull(actualCipherText);
            Assert.IsTrue(expectedCipherText.IsEqualTo(actualCipherText));

            Assert.IsTrue(plainTextBytes.IsEqualTo(encoding.GetBytes(plainText)));
        }
    }
}
