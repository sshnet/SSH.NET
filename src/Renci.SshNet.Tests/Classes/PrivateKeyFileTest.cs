﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Tests.Common;
using System;
using System.IO;

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
                File.Delete(_temporaryFile);
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
                new PrivateKeyFile(fileName);
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
                new PrivateKeyFile(fileName);
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
                new PrivateKeyFile(fileName, "12345");
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
                new PrivateKeyFile(fileName, "12345");
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
                new PrivateKeyFile(privateKey);
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
                new PrivateKeyFile(privateKey, "12345");
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
                new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_DSA()
        {
            using (var stream = GetData("Key.SSH2.DSA.txt"))
            {
                new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_RSA()
        {
            using (var stream = GetData("Key.SSH2.RSA.txt"))
            {
                new PrivateKeyFile(stream);
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_Encrypted_DSA_DES_CBC()
        {
            using (var stream = GetData("Key.SSH2.DSA.Encrypted.Des.CBC.12345.txt"))
            {
                new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("drieseng")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_SSH2_Encrypted_RSA_DES_CBC()
        {
            using (var stream = GetData("Key.SSH2.RSA.Encrypted.Des.CBC.12345.txt"))
            {
                new PrivateKeyFile(stream, "12345");
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
                    new PrivateKeyFile(stream, "34567");
                    Assert.Fail();
                }
                catch (SshException ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(SshException));
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
                    new PrivateKeyFile(stream, null);
                    Assert.Fail();
                }
                catch (SshPassPhraseNullOrEmptyException ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(SshPassPhraseNullOrEmptyException));
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
                    new PrivateKeyFile(stream, string.Empty);
                    Assert.Fail();
                }
                catch (SshPassPhraseNullOrEmptyException ex)
                {
                    Assert.IsInstanceOfType(ex, typeof(SshPassPhraseNullOrEmptyException));
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
                new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Des.Ede3.CBC.12345.txt"))
            {
                new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_128_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_192_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.192.CBC.12345.txt"))
            {
                new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_AES_256_CBC()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Aes.256.CBC.12345.txt"))
            {
                new PrivateKeyFile(stream, "12345");
            }
        }

        [TestMethod]
        [Owner("olegkap")]
        [TestCategory("PrivateKey")]
        public void Test_PrivateKey_RSA_DES_EDE3_CFB()
        {
            using (var stream = GetData("Key.RSA.Encrypted.Des.Ede3.CFB.1234567890.txt"))
            {
                new PrivateKeyFile(stream, "1234567890");
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
                Assert.IsNotNull(privateKeyFile.HostKey);
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
                Assert.IsNotNull(privateKeyFile.HostKey);

                fs.Close();
            }
        }

        /// <summary>
        /// A test for <see cref="PrivateKeyFile(string, string)"/> ctor.
        ///</summary>
        [TestMethod()]
        public void ConstructorWithFileNameAndPassphraseShouldThrowSshPassPhraseNullOrEmptyExceptionWhenPrivateKeyIsEncryptedAndPassphraseIsEmpty()
        {
            var passphrase = string.Empty;

            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            try
            {
                new PrivateKeyFile(_temporaryFile, passphrase);
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
        public void ConstructorWithFileNameAndPassphraseShouldThrowSshPassPhraseNullOrEmptyExceptionWhenPrivateKeyIsEncryptedAndPassphraseIsNull()
        {
            string passphrase = null;

            using (var stream = GetData("Key.RSA.Encrypted.Aes.128.CBC.12345.txt"))
            {
                SaveStreamToFile(stream, _temporaryFile);
            }

            try
            {
                new PrivateKeyFile(_temporaryFile, passphrase);
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
            Assert.IsNotNull(privateKeyFile.HostKey);
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
                Assert.IsNotNull(privateKeyFile.HostKey);
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
                Assert.IsNotNull(privateKeyFile.HostKey);

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
                Assert.IsNotNull(privateKeyFile.HostKey);

                fs.Close();
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
    }
}