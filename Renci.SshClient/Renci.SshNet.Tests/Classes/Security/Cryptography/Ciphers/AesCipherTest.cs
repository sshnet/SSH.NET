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
    public class AesCipherTest : TestBase
    {
        [TestMethod]
        public void Test_Cipher_AES_128_CBC()
        {
            var input = new byte[] { 0x00, 0x00, 0x00, 0x2c, 0x1a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0x30, 0x9e, 0xe0, 0x9c, 0x12, 0xee, 0x3a, 0x30, 0x03, 0x52, 0x1c, 0x1a, 0xe7, 0x3e, 0x0b, 0x9a, 0xcf, 0x9a, 0x57, 0x42, 0x0b, 0x4f, 0x4a, 0x15, 0xa0, 0xf5 };
            var key = new byte[] { 0xe4, 0x94, 0xf9, 0xb1, 0x00, 0x4f, 0x16, 0x2a, 0x80, 0x11, 0xea, 0x73, 0x0d, 0xb9, 0xbf, 0x64 };
            var iv = new byte[] { 0x74, 0x8b, 0x4f, 0xe6, 0xc1, 0x29, 0xb3, 0x54, 0xec, 0x77, 0x92, 0xf3, 0x15, 0xa0, 0x41, 0xa8 };
            var output = new byte[] { 0x19, 0x7f, 0x80, 0xd8, 0xc9, 0x89, 0xc4, 0xa7, 0xc6, 0xc6, 0x3f, 0x9f, 0x1e, 0x00, 0x1f, 0x72, 0xa7, 0x5e, 0xde, 0x40, 0x88, 0xa2, 0x72, 0xf2, 0xed, 0x3f, 0x81, 0x45, 0xb6, 0xbd, 0x45, 0x87, 0x15, 0xa5, 0x10, 0x92, 0x4a, 0x37, 0x9e, 0xa9, 0x80, 0x1c, 0x14, 0x83, 0xa3, 0x39, 0x45, 0x28 };
            var testCipher = new Renci.SshNet.Security.Cryptography.Ciphers.AesCipher(key, new Renci.SshNet.Security.Cryptography.Ciphers.Modes.CbcCipherMode(iv), null);
            var r = testCipher.Encrypt(input);

            if (!r.SequenceEqual(output))
                Assert.Fail("Invalid encryption");
        }

        [TestMethod]
        public void Test_Cipher_AES_128_CTR()
        {
            var input = new byte[] { 0x00, 0x00, 0x00, 0x2c, 0x1a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0xb0, 0x74, 0x21, 0x87, 0x16, 0xb9, 0x69, 0x48, 0x33, 0xce, 0xb3, 0xe7, 0xdc, 0x3f, 0x50, 0xdc, 0xcc, 0xd5, 0x27, 0xb7, 0xfe, 0x7a, 0x78, 0x22, 0xae, 0xc8 };
            var key = new byte[] { 0x17, 0x78, 0x56, 0xe1, 0x3e, 0xbd, 0x3e, 0x50, 0x1d, 0x79, 0x3f, 0x0f, 0x55, 0x37, 0x45, 0x54 };
            var iv = new byte[] { 0xe6, 0x65, 0x36, 0x0d, 0xdd, 0xd7, 0x50, 0xc3, 0x48, 0xdb, 0x48, 0x07, 0xa1, 0x30, 0xd2, 0x38 };
            var output = new byte[] { 0xca, 0xfb, 0x1c, 0x49, 0xbf, 0x82, 0x2a, 0xbb, 0x1c, 0x52, 0xc7, 0x86, 0x22, 0x8a, 0xe5, 0xa4, 0xf3, 0xda, 0x4e, 0x1c, 0x3a, 0x87, 0x41, 0x1c, 0xd2, 0x6e, 0x76, 0xdc, 0xc2, 0xe9, 0xc2, 0x0e, 0xf5, 0xc7, 0xbd, 0x12, 0x85, 0xfa, 0x0e, 0xda, 0xee, 0x50, 0xd7, 0xfd, 0x81, 0x34, 0x25, 0x6d };
            var testCipher = new Renci.SshNet.Security.Cryptography.Ciphers.AesCipher(key, new Renci.SshNet.Security.Cryptography.Ciphers.Modes.CtrCipherMode(iv), null);

            var r = testCipher.Encrypt(input);

            if (!r.SequenceEqual(output))
                Assert.Fail("Invalid encryption");
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("Cipher")]
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
    }
}