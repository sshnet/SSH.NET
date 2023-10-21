using System;
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
            if (_temporaryFile is not null)
            {
                File.Delete(_temporaryFile);
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string)"/> ctor.
        /// </summary>
        [WorkItem(703)]
        [TestMethod]
        public void ConstructorWithFileNameShouldThrowArgumentNullExceptionWhenFileNameIsEmpty()
        {
            var fileName = string.Empty;
            PrivateKeyFile privateKeyFile = null;

            try
            {
                privateKeyFile = new PrivateKeyFile(fileName);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("fileName", ex.ParamName);
            }
            finally
            {
                privateKeyFile?.Dispose();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string)"/> ctor.
        /// </summary>
        [WorkItem(703)]
        [TestMethod]
        public void ConstructorWithFileNameShouldThrowArgumentNullExceptionWhenFileNameIsNull()
        {
            const string fileName = null;
            PrivateKeyFile privateKeyFile = null;

            try
            {
                privateKeyFile = new PrivateKeyFile(fileName);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("fileName", ex.ParamName);
            }
            finally
            {
                privateKeyFile?.Dispose();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        /// </summary>
        [WorkItem(703)]
        [TestMethod]
        public void ConstructorWithFileNameAndPassphraseShouldThrowArgumentNullExceptionWhenFileNameIsEmpty()
        {
            var fileName = string.Empty;
            PrivateKeyFile privateKeyFile = null;

            try
            {
                privateKeyFile = new PrivateKeyFile(fileName, "12345");
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("fileName", ex.ParamName);
            }
            finally
            {
                privateKeyFile?.Dispose();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        /// </summary>
        [WorkItem(703)]
        [TestMethod]
        public void ConstructorWithFileNameAndPassphraseShouldThrowArgumentNullExceptionWhenFileNameIsNull()
        {
            const string fileName = null;
            PrivateKeyFile privateKeyFile = null;

            try
            {
                privateKeyFile = new PrivateKeyFile(fileName, "12345");
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("fileName", ex.ParamName);
            }
            finally
            {
                privateKeyFile?.Dispose();
            }
        }

        [WorkItem(703)]
        [TestMethod]
        public void ConstructorWithPrivateKeyShouldThrowArgumentNullExceptionWhenPrivateKeyIsNull()
        {
            const Stream privateKey = null;
            PrivateKeyFile privateKeyFile = null;

            try
            {
                privateKeyFile = new PrivateKeyFile(privateKey);
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("privateKey", ex.ParamName);
            }
            finally
            {
                privateKeyFile?.Dispose();
            }
        }

        [WorkItem(703)]
        [TestMethod]
        public void ConstructorWithPrivateKeyAndPassphraseShouldThrowArgumentNullExceptionWhenPrivateKeyIsNull()
        {
            const Stream privateKey = null;
            PrivateKeyFile privateKeyFile = null;

            try
            {
                privateKeyFile = new PrivateKeyFile(privateKey, "12345");
                Assert.Fail();
            }
            catch (ArgumentNullException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("privateKey", ex.ParamName);
            }
            finally
            {
                privateKeyFile?.Dispose();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA()
        {
            using (var stream = GetData("Key.RSA.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_DSA()
        {
            using (var stream = GetData("Key.SSH2.DSA.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_RSA()
        {
            using (var stream = GetData("Key.SSH2.RSA.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_Encrypted_DSA_DES_CBC()
        {
            using (var stream = GetData("Key.SSH2.DSA.Encrypted.Des.CBC.12345.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_Encrypted_RSA_DES_CBC()
        {
            using (var stream = GetData("Key.SSH2.RSA.Encrypted.Des.CBC.12345.txt"))
            {
                using (var privateKeyFile = new PrivateKeyFile(stream, "12345"))
                {
                    TestRsaKeyFile(privateKeyFile);
                }
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_Encrypted_ShouldThrowSshExceptionWhenPassphraseIsWrong()
        {
            using (var stream = GetData("Key.SSH2.RSA.Encrypted.Des.CBC.12345.txt"))
            {
                PrivateKeyFile privateKeyFile = null;

                try
                {
                    privateKeyFile = new PrivateKeyFile(stream, "34567");
                    Assert.Fail();
                }
                catch (SshException ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(SshException));
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Invalid passphrase.", ex.Message);
                }
                finally
                {
                    privateKeyFile?.Dispose();
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
                PrivateKeyFile privateKeyFile = null;

                try
                {
                    privateKeyFile = new PrivateKeyFile(stream, passPhrase: null);
                    Assert.Fail();
                }
                catch (SshPassPhraseNullOrEmptyException ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(SshPassPhraseNullOrEmptyException));
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Private key is encrypted but passphrase is empty.", ex.Message);
                }
                finally
                {
                    privateKeyFile?.Dispose();
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
                PrivateKeyFile privateKeyFile = null;

                try
                {
                    privateKeyFile = new PrivateKeyFile(stream, string.Empty);
                    Assert.Fail();
                }
                catch (SshPassPhraseNullOrEmptyException ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(SshPassPhraseNullOrEmptyException));
                    Assert.IsNull(ex.InnerException);
                    Assert.AreEqual("Private key is encrypted but passphrase is empty.", ex.Message);
                }
                finally
                {
                    privateKeyFile?.Dispose();
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
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Des.Ede3.CBC.12345.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_128_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_192_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.192.CBC.12345.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_256_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.256.CBC.12345.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CFB()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Des.Ede3.CFB.1234567890.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "1234567890");
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA()
        {
            using (var stream = GetData("Key.ECDSA.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA384()
        {
            using (var stream = GetData("Key.ECDSA384.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA521()
        {
            using (var stream = GetData("Key.ECDSA521.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA_Encrypted()
        {
            using (var stream = GetData("Key.ECDSA.Encrypted.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA384_Encrypted()
        {
            using (var stream = GetData("Key.ECDSA384.Encrypted.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_ECDSA521_Encrypted()
        {
            using (var stream = GetData("Key.ECDSA521.Encrypted.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                privateKeyFile.Dispose();
            }
        }

        /// <summary>
        /// A test for Dispose
        /// </summary>
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
        /// A test for <see cref="PrivateKeyFile(Stream, string)"/> ctor.
        /// </summary>
        [TestMethod]
        public void ConstructorWithStreamAndPassphrase()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        /// </summary>
        [TestMethod]
        public void ConstructorWithFileNameAndPassphrase()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            var privateKeyFile = new PrivateKeyFile(_temporaryFile, "12345");
            TestRsaKeyFile(privateKeyFile);
            privateKeyFile.Dispose();
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        /// </summary>
        [TestMethod]
        public void ConstructorWithFileNameAndPassphraseShouldThrowSshPassPhraseNullOrEmptyExceptionWhenNeededPassphraseIsEmpty()
        {
            var passphrase = string.Empty;

            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            PrivateKeyFile privateKeyFile = null;

            try
            {
                privateKeyFile = new PrivateKeyFile(_temporaryFile, passphrase);
                Assert.Fail();
            }
            catch (SshPassPhraseNullOrEmptyException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Private key is encrypted but passphrase is empty.", ex.Message);
            }
            finally
            {
                privateKeyFile?.Dispose();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        /// </summary>
        [TestMethod]
        public void ConstructorWithFileNameAndPassphraseShouldThrowSshPassPhraseNullOrEmptyExceptionWhenNeededPassphraseIsNull()
        {
            string passphrase = null;

            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            PrivateKeyFile privateKeyFile = null;

            try
            {
                privateKeyFile = new PrivateKeyFile(_temporaryFile, passphrase);
                Assert.Fail();
            }
            catch (SshPassPhraseNullOrEmptyException ex)
            {
                Assert.IsNull(ex.InnerException);
                Assert.AreEqual("Private key is encrypted but passphrase is empty.", ex.Message);
            }
            finally
            {
                privateKeyFile?.Dispose();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string)"/> ctor.
        /// </summary>
        [TestMethod]
        public void ConstructorWithFileName()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            var privateKeyFile = new PrivateKeyFile(_temporaryFile);
            TestRsaKeyFile(privateKeyFile);
            privateKeyFile.Dispose();
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(Stream)"/> ctor.
        /// </summary>
        [TestMethod]
        public void ConstructorWithStream()
        {
            using (var stream = GetData("Key.RSA.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
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
                privateKeyFile.Dispose();
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
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("bhalbright")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ED25519()
        {
            using (var stream = GetData("Key.OPENSSH.ED25519.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("bhalbright")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ED25519_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.ED25519.Encrypted.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "password");
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_RSA()
        {
            using (var stream = GetData("Key.OPENSSH.RSA.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_RSA_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.RSA.Encrypted.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                TestRsaKeyFile(privateKeyFile);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA.Encrypted.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA384()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA384.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA384_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA384.Encrypted.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA521()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA521.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream);
                privateKeyFile.Dispose();
            }
        }

        [TestMethod]
        [Owner("darinkes")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_OPENSSH_ECDSA521_ENCRYPTED()
        {
            using (var stream = GetData("Key.OPENSSH.ECDSA521.Encrypted.txt"))
            {
                var privateKeyFile = new PrivateKeyFile(stream, "12345");
                privateKeyFile.Dispose();
            }
        }

        private static void SaveStreamToFile(Stream stream, string fileName)
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

        private static string GetTempFileName()
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

            Assert.AreEqual("rsa-sha2-512", algorithms[0].Name);
            Assert.AreEqual("rsa-sha2-256", algorithms[1].Name);
            Assert.AreEqual("ssh-rsa", algorithms[2].Name);
        }
    }
}
