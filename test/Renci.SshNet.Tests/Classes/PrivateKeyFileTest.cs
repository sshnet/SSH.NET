﻿using System;
using System.IO;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Common;
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
        [WorkItem(703), TestMethod]
        public void ConstructorWithFileNameShouldThrowArgumentNullExceptionWhenFileNameIsEmpty()
        {
            var fileName = string.Empty;
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
        /// A test for <see cref="PrivateKeyFile(string)"/> ctor.
        ///</summary>
        [WorkItem(703), TestMethod]
        public void ConstructorWithFileNameShouldThrowArgumentNullExceptionWhenFileNameIsNull()
        {
            var fileName = string.Empty;
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
        [WorkItem(703), TestMethod]
        public void ConstructorWithFileNameAndPassphraseShouldThrowArgumentNullExceptionWhenFileNameIsEmpty()
        {
            var fileName = string.Empty;
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

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        ///</summary>
        [WorkItem(703), TestMethod]
        public void ConstructorWithFileNameAndPassphraseShouldThrowArgumentNullExceptionWhenFileNameIsNull()
        {
            var fileName = string.Empty;
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

        [WorkItem(703), TestMethod]
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

        [WorkItem(703), TestMethod]
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
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA()
        {
            using (var stream = GetData("Key.RSA.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream));
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_DSA()
        {
            using (var stream = GetData("Key.SSH2.DSA.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_RSA()
        {
            using (var stream = GetData("Key.SSH2.RSA.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream));
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_Encrypted_DSA_DES_CBC()
        {
            using (var stream = GetData("Key.SSH2.DSA.Encrypted.Des.CBC.12345.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_Encrypted_RSA_DES_CBC()
        {
            using (var stream = GetData("Key.SSH2.RSA.Encrypted.Des.CBC.12345.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream, "12345"));
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
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
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
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
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
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

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Des.CBC.12345.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream, "12345"));
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Des.Ede3.CBC.12345.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_128_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream, "12345"));
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_192_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.192.CBC.12345.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream, "12345"));
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_256_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.256.CBC.12345.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream, "12345"));
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CFB()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Des.Ede3.CFB.1234567890.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream, "1234567890"));
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA()
        {
            using (var stream = GetData("Key.ECDSA.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA384()
        {
            using (var stream = GetData("Key.ECDSA384.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA521()
        {
            using (var stream = GetData("Key.ECDSA521.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA_Encrypted()
        {
            using (var stream = GetData("Key.ECDSA.Encrypted.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA384_Encrypted()
        {
            using (var stream = GetData("Key.ECDSA384.Encrypted.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA521_Encrypted()
        {
            using (var stream = GetData("Key.ECDSA521.Encrypted.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        /// <summary>
        ///A test for Dispose
        ///</summary>
        [TestMethod()]
        public void DisposeTest()
        {
            using (var privateKeyStream = GetData("Key.RSA.txt"))
            {
                var target = new PrivateKeyFile(privateKeyStream);
                target.Dispose();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(Stream, string)"/> ctor.
        ///</summary>
        [TestMethod()]
        public void ConstructorWithStreamAndPassphrase()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                TestRsaKeyFile(privateKeyFile);
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        ///</summary>
        [TestMethod()]
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

                fs.Close();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        ///</summary>
        [TestMethod()]
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
        [TestMethod()]
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
        [TestMethod()]
        public void ConstructorWithFileName()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            var privateKeyFile = new PrivateKeyFile(_temporaryFile, "12345");
            TestRsaKeyFile(privateKeyFile);
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(Stream)"/> ctor.
        ///</summary>
        [TestMethod()]
        public void ConstructorWithStream()
        {
            using (var stream = GetData("Key.RSA.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                TestRsaKeyFile(privateKeyFile);
            }
        }

        [TestMethod]
        [TestCategory("PrivateKey")]
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

                fs.Close();
            }
        }

        [TestMethod]
        [TestCategory("PrivateKey")]
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

                fs.Close();
            }
        }

        [TestMethod()]
        [Owner("bhalbright")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ED25519()
        {
            using (var stream = GetData("Key.OPENSSH.ED25519.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod()]
        [Owner("bhalbright")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ED25519_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.ED25519.Encrypted.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod()]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_RSA()
        {
            using (var stream = GetData("Key.OPENSSH.RSA.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream));
            }
        }

        [TestMethod()]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_RSA_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.RSA.Encrypted.txt"))
            {
                TestRsaKeyFile(new PrivateKeyFile(stream, "12345"));
            }
        }

        [TestMethod()]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod()]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA.Encrypted.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod()]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA384()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA384.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod()]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA384_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA384.Encrypted.txt"))
            {
                _ = new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod()]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA521()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA521.txt"))
            {
                _ = new PrivateKeyFile(stream);
            }
        }

        [TestMethod()]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA521_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA521.Encrypted.txt"))
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
