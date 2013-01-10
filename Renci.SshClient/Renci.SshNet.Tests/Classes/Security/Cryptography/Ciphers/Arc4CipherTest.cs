using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        [TestMethod()]
        public void Arc4CipherConstructorTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            Arc4Cipher target = new Arc4Cipher(key, true);
            Assert.Inconclusive("TODO: Implement code to verify target");
        }

        /// <summary>
        ///A test for Decrypt
        ///</summary>
        [TestMethod()]
        public void DecryptTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            Arc4Cipher target = new Arc4Cipher(key, true); // TODO: Initialize to an appropriate value
            byte[] input = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Decrypt(input);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for DecryptBlock
        ///</summary>
        [TestMethod()]
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

        /// <summary>
        ///A test for Encrypt
        ///</summary>
        [TestMethod()]
        public void EncryptTest()
        {
            byte[] key = null; // TODO: Initialize to an appropriate value
            Arc4Cipher target = new Arc4Cipher(key, true); // TODO: Initialize to an appropriate value
            byte[] input = null; // TODO: Initialize to an appropriate value
            byte[] expected = null; // TODO: Initialize to an appropriate value
            byte[] actual;
            actual = target.Encrypt(input);
            Assert.AreEqual(expected, actual);
            Assert.Inconclusive("Verify the correctness of this test method.");
        }

        /// <summary>
        ///A test for EncryptBlock
        ///</summary>
        [TestMethod()]
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