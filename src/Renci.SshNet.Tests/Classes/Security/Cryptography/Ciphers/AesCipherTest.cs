using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Tests.Common;
using Renci.SshNet.Tests.Properties;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers
{
    /// <summary>
    ///
    /// </summary>
    [TestClass]
    public class AesCipherTest : TestBase
    {
        [TestMethod]
        public void Encrypt_Input_128_CBC()
        {
            var input = new byte[] { 0x00, 0x00, 0x00, 0x2c, 0x1a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0x30, 0x9e, 0xe0, 0x9c, 0x12, 0xee, 0x3a, 0x30, 0x03, 0x52, 0x1c, 0x1a, 0xe7, 0x3e, 0x0b, 0x9a, 0xcf, 0x9a, 0x57, 0x42, 0x0b, 0x4f, 0x4a, 0x15, 0xa0, 0xf5 };
            var key = new byte[] { 0xe4, 0x94, 0xf9, 0xb1, 0x00, 0x4f, 0x16, 0x2a, 0x80, 0x11, 0xea, 0x73, 0x0d, 0xb9, 0xbf, 0x64 };
            var iv = new byte[] { 0x74, 0x8b, 0x4f, 0xe6, 0xc1, 0x29, 0xb3, 0x54, 0xec, 0x77, 0x92, 0xf3, 0x15, 0xa0, 0x41, 0xa8 };
            var expected = new byte[] { 0x19, 0x7f, 0x80, 0xd8, 0xc9, 0x89, 0xc4, 0xa7, 0xc6, 0xc6, 0x3f, 0x9f, 0x1e, 0x00, 0x1f, 0x72, 0xa7, 0x5e, 0xde, 0x40, 0x88, 0xa2, 0x72, 0xf2, 0xed, 0x3f, 0x81, 0x45, 0xb6, 0xbd, 0x45, 0x87, 0x15, 0xa5, 0x10, 0x92, 0x4a, 0x37, 0x9e, 0xa9, 0x80, 0x1c, 0x14, 0x83, 0xa3, 0x39, 0x45, 0x28 };
            var testCipher = new AesCipher(key, new CbcCipherMode(iv), null);

            var actual = testCipher.Encrypt(input);

            Assert.IsTrue(actual.IsEqualTo(expected));
        }

        [TestMethod]
        public void Encrypt_InputAndOffsetAndLength_128_CBC()
        {
            // 2 dummy bytes before and 3 after the input from test case above
            var input = new byte[] { 0x05, 0x07, 0x00, 0x00, 0x00, 0x2c, 0x1a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0x30, 0x9e, 0xe0, 0x9c, 0x12, 0xee, 0x3a, 0x30, 0x03, 0x52, 0x1c, 0x1a, 0xe7, 0x3e, 0x0b, 0x9a, 0xcf, 0x9a, 0x57, 0x42, 0x0b, 0x4f, 0x4a, 0x15, 0xa0, 0xf5, 0x0f, 0x0d, 0x03 };
            var key = new byte[] { 0xe4, 0x94, 0xf9, 0xb1, 0x00, 0x4f, 0x16, 0x2a, 0x80, 0x11, 0xea, 0x73, 0x0d, 0xb9, 0xbf, 0x64 };
            var iv = new byte[] { 0x74, 0x8b, 0x4f, 0xe6, 0xc1, 0x29, 0xb3, 0x54, 0xec, 0x77, 0x92, 0xf3, 0x15, 0xa0, 0x41, 0xa8 };
            var expected = new byte[] { 0x19, 0x7f, 0x80, 0xd8, 0xc9, 0x89, 0xc4, 0xa7, 0xc6, 0xc6, 0x3f, 0x9f, 0x1e, 0x00, 0x1f, 0x72, 0xa7, 0x5e, 0xde, 0x40, 0x88, 0xa2, 0x72, 0xf2, 0xed, 0x3f, 0x81, 0x45, 0xb6, 0xbd, 0x45, 0x87, 0x15, 0xa5, 0x10, 0x92, 0x4a, 0x37, 0x9e, 0xa9, 0x80, 0x1c, 0x14, 0x83, 0xa3, 0x39, 0x45, 0x28 };
            var testCipher = new AesCipher(key, new CbcCipherMode(iv), null);

            var actual = testCipher.Encrypt(input, 2, input.Length - 5);

            Assert.IsTrue(actual.IsEqualTo(expected));
        }

        [TestMethod]
        public void Encrypt_Input_128_CTR()
        {
            var input = new byte[] { 0x00, 0x00, 0x00, 0x2c, 0x1a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0xb0, 0x74, 0x21, 0x87, 0x16, 0xb9, 0x69, 0x48, 0x33, 0xce, 0xb3, 0xe7, 0xdc, 0x3f, 0x50, 0xdc, 0xcc, 0xd5, 0x27, 0xb7, 0xfe, 0x7a, 0x78, 0x22, 0xae, 0xc8 };
            var key = new byte[] { 0x17, 0x78, 0x56, 0xe1, 0x3e, 0xbd, 0x3e, 0x50, 0x1d, 0x79, 0x3f, 0x0f, 0x55, 0x37, 0x45, 0x54 };
            var iv = new byte[] { 0xe6, 0x65, 0x36, 0x0d, 0xdd, 0xd7, 0x50, 0xc3, 0x48, 0xdb, 0x48, 0x07, 0xa1, 0x30, 0xd2, 0x38 };
            var expected = new byte[] { 0xca, 0xfb, 0x1c, 0x49, 0xbf, 0x82, 0x2a, 0xbb, 0x1c, 0x52, 0xc7, 0x86, 0x22, 0x8a, 0xe5, 0xa4, 0xf3, 0xda, 0x4e, 0x1c, 0x3a, 0x87, 0x41, 0x1c, 0xd2, 0x6e, 0x76, 0xdc, 0xc2, 0xe9, 0xc2, 0x0e, 0xf5, 0xc7, 0xbd, 0x12, 0x85, 0xfa, 0x0e, 0xda, 0xee, 0x50, 0xd7, 0xfd, 0x81, 0x34, 0x25, 0x6d };
            var testCipher = new AesCipher(key, new CtrCipherMode(iv), null);

            var actual = testCipher.Encrypt(input);

            Assert.IsTrue(actual.IsEqualTo(expected));
        }

        [TestMethod]
        public void Decrypt_Input_128_CTR()
        {
            var key = new byte[] { 0x17, 0x78, 0x56, 0xe1, 0x3e, 0xbd, 0x3e, 0x50, 0x1d, 0x79, 0x3f, 0x0f, 0x55, 0x37, 0x45, 0x54 };
            var iv = new byte[] { 0xe6, 0x65, 0x36, 0x0d, 0xdd, 0xd7, 0x50, 0xc3, 0x48, 0xdb, 0x48, 0x07, 0xa1, 0x30, 0xd2, 0x38 };
            var input = new byte[] { 0xca, 0xfb, 0x1c, 0x49, 0xbf, 0x82, 0x2a, 0xbb, 0x1c, 0x52, 0xc7, 0x86, 0x22, 0x8a, 0xe5, 0xa4, 0xf3, 0xda, 0x4e, 0x1c, 0x3a, 0x87, 0x41, 0x1c, 0xd2, 0x6e, 0x76, 0xdc, 0xc2, 0xe9, 0xc2, 0x0e, 0xf5, 0xc7, 0xbd, 0x12, 0x85, 0xfa, 0x0e, 0xda, 0xee, 0x50, 0xd7, 0xfd, 0x81, 0x34, 0x25, 0x6d };
            var expected = new byte[] { 0x00, 0x00, 0x00, 0x2c, 0x1a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0xb0, 0x74, 0x21, 0x87, 0x16, 0xb9, 0x69, 0x48, 0x33, 0xce, 0xb3, 0xe7, 0xdc, 0x3f, 0x50, 0xdc, 0xcc, 0xd5, 0x27, 0xb7, 0xfe, 0x7a, 0x78, 0x22, 0xae, 0xc8 };
            var testCipher = new AesCipher(key, new CtrCipherMode(iv), null);

            var actual = testCipher.Decrypt(input);

            Assert.IsTrue(expected.IsEqualTo(actual));
        }

        [TestMethod]
        public void Decrypt_InputAndOffsetAndLength_128_CTR()
        {
            var key = new byte[] { 0x17, 0x78, 0x56, 0xe1, 0x3e, 0xbd, 0x3e, 0x50, 0x1d, 0x79, 0x3f, 0x0f, 0x55, 0x37, 0x45, 0x54 };
            var iv = new byte[] { 0xe6, 0x65, 0x36, 0x0d, 0xdd, 0xd7, 0x50, 0xc3, 0x48, 0xdb, 0x48, 0x07, 0xa1, 0x30, 0xd2, 0x38 };
            var input = new byte[] { 0x0a, 0xca, 0xfb, 0x1c, 0x49, 0xbf, 0x82, 0x2a, 0xbb, 0x1c, 0x52, 0xc7, 0x86, 0x22, 0x8a, 0xe5, 0xa4, 0xf3, 0xda, 0x4e, 0x1c, 0x3a, 0x87, 0x41, 0x1c, 0xd2, 0x6e, 0x76, 0xdc, 0xc2, 0xe9, 0xc2, 0x0e, 0xf5, 0xc7, 0xbd, 0x12, 0x85, 0xfa, 0x0e, 0xda, 0xee, 0x50, 0xd7, 0xfd, 0x81, 0x34, 0x25, 0x6d, 0x0a, 0x05 };
            var expected = new byte[] { 0x00, 0x00, 0x00, 0x2c, 0x1a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0xb0, 0x74, 0x21, 0x87, 0x16, 0xb9, 0x69, 0x48, 0x33, 0xce, 0xb3, 0xe7, 0xdc, 0x3f, 0x50, 0xdc, 0xcc, 0xd5, 0x27, 0xb7, 0xfe, 0x7a, 0x78, 0x22, 0xae, 0xc8 };
            var testCipher = new AesCipher(key, new CtrCipherMode(iv), null);

            var actual = testCipher.Decrypt(input, 1, input.Length - 3);

            Assert.IsTrue(expected.IsEqualTo(actual));
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
        [TestCategory("integration")]
        public void Test_Cipher_AEes128CBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes128-cbc", new CipherInfo(128, (key, iv) => { return new AesCipher(key, new CbcCipherMode(iv), null); }));

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
        public void Test_Cipher_Aes192CBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes192-cbc", new CipherInfo(192, (key, iv) => { return new AesCipher(key, new CbcCipherMode(iv), null); }));

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
        public void Test_Cipher_Aes256CBC_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes256-cbc", new CipherInfo(256, (key, iv) => { return new AesCipher(key, new CbcCipherMode(iv), null); }));

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
        public void Test_Cipher_Aes128CTR_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes128-ctr", new CipherInfo(128, (key, iv) => { return new AesCipher(key, new CtrCipherMode(iv), null); }));

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
        public void Test_Cipher_Aes192CTR_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes192-ctr", new CipherInfo(192, (key, iv) => { return new AesCipher(key, new CtrCipherMode(iv), null); }));

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
        public void Test_Cipher_Aes256CTR_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("aes256-ctr", new CipherInfo(256, (key, iv) => { return new AesCipher(key, new CtrCipherMode(iv), null); }));

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
        public void Test_Cipher_Arcfour_Connection()
        {
            var connectionInfo = new PasswordConnectionInfo(Resources.HOST, int.Parse(Resources.PORT), Resources.USERNAME, Resources.PASSWORD);
            connectionInfo.Encryptions.Clear();
            connectionInfo.Encryptions.Add("arcfour", new CipherInfo(128, (key, iv) => { return new Arc4Cipher(key, false); }));

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                client.Disconnect();
            }
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
            AesCipher target = new AesCipher(key, mode, padding); // TODO: Initialize to an appropriate value
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
        ///A test for AesCipher Constructor
        ///</summary>
        [TestMethod]
        [Ignore] // placeholder for actual test
        public void AesCipherConstructorTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            CipherMode mode = null; // TODO: Initialize to an appropriate value
            CipherPadding padding = null; // TODO: Initialize to an appropriate value
            AesCipher target = new AesCipher(key, mode, padding);
            Assert.Inconclusive("TODO: Implement code to verify target");
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
            AesCipher target = new AesCipher(key, mode, padding); // TODO: Initialize to an appropriate value
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