using System;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes
{
    /// <summary>
    /// old private key information/
    /// </summary>
    [TestClass]
    public class PrivateKeyFileTest : TestBase
    {
        private string _temporaryFile;

        [TestInitialize]
        public void SetUp()
        {
            _temporaryFile = GetTempFileName();
        }

        [TestCleanup]
        public void TearDown()
        {
            if (_temporaryFile != null)
            {
                File.Delete(_temporaryFile);
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string)"/> ctor.
        ///</summary>
        [TestMethod]
        public void ConstructorWithFileNameShouldThrowArgumentNullExceptionWhenFileNameIsNull()
        {
            string fileName = null;
            try
            {
                _ = new PrivateKeyFile(fileName);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("fileName", ex.ParamName);
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        ///</summary>
        [TestMethod]
        public void ConstructorWithFileNameAndPassphraseShouldThrowArgumentNullExceptionWhenFileNameIsNull()
        {
            string fileName = null;
            try
            {
                _ = new PrivateKeyFile(fileName, "12345");
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("fileName", ex.ParamName);
            }
        }

        [TestMethod]
        public void ConstructorWithPrivateKeyShouldThrowArgumentNullExceptionWhenPrivateKeyIsNull()
        {
            Stream privateKey = null;
            try
            {
                _ = new PrivateKeyFile(privateKey);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("privateKey", ex.ParamName);
            }
        }

        [TestMethod]
        public void ConstructorWithPrivateKeyAndPassphraseShouldThrowArgumentNullExceptionWhenPrivateKeyIsNull()
        {
            Stream privateKey = null;
            try
            {
                _ = new PrivateKeyFile(privateKey, "12345");
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("privateKey", ex.ParamName);
            }
        }

        [TestMethod]
        public void ConstructorWithKeyShouldThrowArgumentNullExceptionWhenKeyIsNull()
        {
            Key key = null;
            try
            {
                _ = new PrivateKeyFile(key);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("key", ex.ParamName);
            }
        }

        [TestMethod]
        public void Test_PrivateKey_SSH2_Encrypted_ShouldThrowSshExceptionWhenPassphraseIsWrong()
        {
            using (var stream = GetData("Key.SSH2.RSA.Encrypted.Des.CBC.12345.txt"))
            {
                try
                {
                    _ = new PrivateKeyFile(stream, "34567");
                    Assert.Fail();
                }
                catch (SshException ex)
                {
                    Assert.IsInstanceOfType<SshException>(ex);
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Invalid passphrase.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void Test_PrivateKey_SSH2_Encrypted_ShouldThrowSshPassPhraseNullOrEmptyExceptionWhenPassphraseIsNull()
        {
            using (var stream = GetData("Key.SSH2.RSA.Encrypted.Des.CBC.12345.txt"))
            {
                try
                {
                    _ = new PrivateKeyFile(stream, null);
                    Assert.Fail();
                }
                catch (SshPassPhraseNullOrEmptyException ex)
                {
                    Assert.IsInstanceOfType<SshPassPhraseNullOrEmptyException>(ex);
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Private key is encrypted but passphrase is empty.", ex.Message);
                }
            }
        }

        [TestMethod]
        public void Test_PrivateKey_SSH2_Encrypted_ShouldThrowSshPassPhraseNullOrEmptyExceptionWhenPassphraseIsEmpty()
        {
            using (var stream = GetData("Key.SSH2.RSA.Encrypted.Des.CBC.12345.txt"))
            {
                try
                {
                    _ = new PrivateKeyFile(stream, string.Empty);
                    Assert.Fail();
                }
                catch (SshPassPhraseNullOrEmptyException ex)
                {
                    Assert.IsInstanceOfType<SshPassPhraseNullOrEmptyException>(ex);
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Private key is encrypted but passphrase is empty.", ex.Message);
                }
            }
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod]
        public void DisposeTest()
        {
            using (var privateKeyStream = GetData("Key.RSA.txt"))
            {
                var target = new PrivateKeyFile(privateKeyStream);
                target.Dispose();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        ///</summary>
        [TestMethod]
        public void ConstructorWithFileNameAndPassphrase()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            using (var fs = File.Open(_temporaryFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var privateKeyFile = new PrivateKeyFile(_temporaryFile, "12345");
                TestRsaKeyFile(privateKeyFile);
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        ///</summary>
        [TestMethod]
        public void ConstructorWithFileNameAndPassphraseShouldThrowSshPassPhraseNullOrEmptyExceptionWhenNeededPassphraseIsEmpty()
        {
            var passphrase = string.Empty;

            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            try
            {
                _ = new PrivateKeyFile(_temporaryFile, passphrase);
                Assert.Fail();
            }
            catch (SshPassPhraseNullOrEmptyException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Private key is encrypted but passphrase is empty.", ex.Message);
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        ///</summary>
        [TestMethod]
        public void ConstructorWithFileNameAndPassphraseShouldThrowSshPassPhraseNullOrEmptyExceptionWhenNeededPassphraseIsNull()
        {
            string passphrase = null;

            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            try
            {
                _ = new PrivateKeyFile(_temporaryFile, passphrase);
                Assert.Fail();
            }
            catch (SshPassPhraseNullOrEmptyException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Private key is encrypted but passphrase is empty.", ex.Message);
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string)"/> ctor.
        ///</summary>
        [TestMethod]
        public void ConstructorWithFileName()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            var privateKeyFile = new PrivateKeyFile(_temporaryFile, "12345");
            TestRsaKeyFile(privateKeyFile);
        }

        [TestMethod]
        public void ConstructorWithFileNameShouldBeAbleToReadFileThatIsSharedForReadAccess()
        {
            using (var stream = GetData("Key.RSA.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            using (var fs = File.Open(_temporaryFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var privateKeyFile = new PrivateKeyFile(_temporaryFile);
                TestRsaKeyFile(privateKeyFile);
            }
        }

        [TestMethod]
        public void ConstructorWithFileNameAndPassPhraseShouldBeAbleToReadFileThatIsSharedForReadAccess()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            using (var fs = File.Open(_temporaryFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var privateKeyFile = new PrivateKeyFile(_temporaryFile, "12345");
                TestRsaKeyFile(privateKeyFile);
            }
        }

        [TestMethod]
        [DataRow("Key.DSA.txt", null, typeof(DsaKey))]
        [DataRow("Key.ECDSA.Encrypted.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.ECDSA.txt", null, typeof(EcdsaKey))]
        [DataRow("Key.ECDSA384.Encrypted.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.ECDSA384.txt", null, typeof(EcdsaKey))]
        [DataRow("Key.ECDSA521.Encrypted.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.ECDSA521.txt", null, typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ECDSA.Encrypted.Aes.128.CTR.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ECDSA.Encrypted.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ECDSA.txt", null, typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ECDSA384.Encrypted.Aes.256.GCM.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ECDSA384.Encrypted.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ECDSA384.txt", null, typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ECDSA521.Encrypted.Aes.192.CBC.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ECDSA521.Encrypted.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ECDSA521.txt", null, typeof(EcdsaKey))]
        [DataRow("Key.OPENSSH.ED25519.Encrypted.3Des.CBC.txt", "12345", typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.ED25519.Encrypted.Aes.128.CBC.txt", "12345", typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.ED25519.Encrypted.Aes.128.GCM.txt", "12345", typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.ED25519.Encrypted.Aes.256.CBC.txt", "12345", typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.ED25519.Encrypted.Aes.256.CTR.txt", "12345", typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.ED25519.Encrypted.ChaCha20.Poly1305.txt", "12345", typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.ED25519.Encrypted.txt", "12345", typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.ED25519.txt", null, typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.RSA.Encrypted.Aes.192.CTR.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.OPENSSH.RSA.Encrypted.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.OPENSSH.RSA.txt", null, typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Aes.128.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Aes.192.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Aes.256.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Des.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Des.Ede3.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Des.Ede3.CFB.1234567890.txt", "1234567890", typeof(RsaKey))]
        [DataRow("Key.RSA.txt", null, typeof(RsaKey))]
        [DataRow("Key.SSH2.DSA.Encrypted.Des.CBC.12345.txt", "12345", typeof(DsaKey))]
        [DataRow("Key.SSH2.DSA.txt", null, typeof(DsaKey))]
        [DataRow("Key.SSH2.RSA.Encrypted.Des.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.SSH2.RSA.txt", null, typeof(RsaKey))]
        public void Test_PrivateKey(string name, string passPhrase, Type expectedKeyType)
        {
            using (var stream = GetData(name))
            {
                var pkFile = new PrivateKeyFile(stream, passPhrase);

                Assert.IsInstanceOfType(pkFile.Key, expectedKeyType);

                if (expectedKeyType == typeof(RsaKey))
                {
                    TestRsaKeyFile(pkFile);
                }
            }
        }

        [TestMethod]
        [Owner("scott-xu")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_PKCS8_RSA()
        {
            using (var stream = GetData("Key.PKCS8.RSA.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream));
            }
        }

        [TestMethod]
        [Owner("scott-xu")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_PKCS8_RSA_Encrypted()
        {
            using (var stream = GetData("Key.PKCS8.RSA.Encrypted.Aes.256.CBC.12345.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream, "12345"));
            }
        }

        [TestMethod]
        [Owner("scott-xu")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_PKCS8_DSA()
        {
            using (var stream = GetData("Key.PKCS8.DSA.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("scott-xu")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_PKCS8_DSA_Encrypted()
        {
            using (var stream = GetData("Key.PKCS8.DSA.Encrypted.Aes.256.CBC.12345.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("scott-xu")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_PKCS8_ECDSA()
        {
            using (var stream = GetData("Key.PKCS8.ECDSA.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("scott-xu")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_PKCS8_ECDSA_Encrypted()
        {
            using (var stream = GetData("Key.PKCS8.ECDSA.Encrypted.Aes.256.CBC.12345.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("scott-xu")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_PKCS8_ED25519()
        {
            using (var stream = GetData("Key.PKCS8.ED25519.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("scott-xu")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_PKCS8_ED25519_Encrypted()
        {
            using (var stream = GetData("Key.PKCS8.ED25519.Encrypted.Aes.256.CBC.12345.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        private void SaveStreamToFile(Stream stream, string fileName)
        {
            var buffer = new byte[4000];

            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                while (bytesRead > 0)
                {
                    fs.Write(buffer, 0, bytesRead);
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                }
            }
        }

        private string GetTempFileName()
        {
            var tempFile = Path.GetTempFileName();
            File.Delete(tempFile);
            return tempFile;
        }

        private static void TestRsaKeyFile(PrivateKeyFile rsaPrivateKeyFile)
        {
            Assert.IsNotNull(rsaPrivateKeyFile.HostKeyAlgorithms);
            Assert.AreEqual(3, rsaPrivateKeyFile.HostKeyAlgorithms.Count);

            var algorithms = rsaPrivateKeyFile.HostKeyAlgorithms.ToList();

            // ssh-rsa should be attempted first during authentication by default.
            // See https://github.com/sshnet/SSH.NET/issues/1233#issuecomment-1871196405
            Assert.AreEqual("ssh-rsa", algorithms[0].Name);
            Assert.AreEqual("rsa-sha2-512", algorithms[1].Name);
            Assert.AreEqual("rsa-sha2-256", algorithms[2].Name);
        }
    }
}
