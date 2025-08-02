using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
#if NETFRAMEWORK
        private static readonly DateTimeOffset UnixEpoch = new(1970, 01, 01, 00, 00, 00, TimeSpan.Zero);
#else
        private static readonly DateTimeOffset UnixEpoch = DateTimeOffset.UnixEpoch;
#endif

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
        [DataRow("Key.ECDSA.Encrypted.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.ECDSA.PKCS8.Encrypted.Aes.256.CBC.12345.txt", "12345", typeof(EcdsaKey))]
        [DataRow("Key.ECDSA.PKCS8.txt", null, typeof(EcdsaKey))]
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
        [DataRow("Key.OPENSSH.ED25519.PKCS8.Encrypted.Aes.256.CBC.12345.txt", "12345", typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.ED25519.PKCS8.txt", null, typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.ED25519.txt", null, typeof(ED25519Key))]
        [DataRow("Key.OPENSSH.RSA.Encrypted.Aes.192.CTR.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.OPENSSH.RSA.Encrypted.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.OPENSSH.RSA.txt", null, typeof(RsaKey))]
        [DataRow("Key.PuTTY2.Ed25519.Encrypted.12345.ppk", "12345", typeof(ED25519Key))]
        [DataRow("Key.PuTTY2.Ed25519.ppk", null, typeof(ED25519Key))]
        [DataRow("Key.PuTTY2.RSA.Encrypted.12345.ppk", "12345", typeof(RsaKey))]
        [DataRow("Key.PuTTY2.RSA.ppk", null, typeof(RsaKey))]
        [DataRow("Key.PuTTY3.ECDSA.Encrypted.Argon2id.12345.ppk", "12345", typeof(EcdsaKey))]
        [DataRow("Key.PuTTY3.ECDSA.ppk", null, typeof(EcdsaKey))]
        [DataRow("Key.PuTTY3.Ed25519.Encrypted.Argon2i.12345.ppk", "12345", typeof(ED25519Key))]
        [DataRow("Key.PuTTY3.Ed25519.Encrypted.Argon2d.12345.ppk", "12345", typeof(ED25519Key))]
        [DataRow("Key.PuTTY3.Ed25519.Encrypted.Argon2id.12345.ppk", "12345", typeof(ED25519Key))]
        [DataRow("Key.PuTTY3.Ed25519.ppk", null, typeof(ED25519Key))]
        [DataRow("Key.PuTTY3.RSA.Encrypted.Argon2id.12345.ppk", "12345", typeof(RsaKey))]
        [DataRow("Key.PuTTY3.RSA.ppk", null, typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Aes.128.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Aes.192.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Aes.256.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Des.Ede3.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.Encrypted.Des.Ede3.CFB.1234567890.txt", "1234567890", typeof(RsaKey))]
        [DataRow("Key.RSA.PKCS8.Encrypted.Aes.256.CBC.12345.txt", "12345", typeof(RsaKey))]
        [DataRow("Key.RSA.PKCS8.txt", null, typeof(RsaKey))]
        [DataRow("Key.RSA.txt", null, typeof(RsaKey))]
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
        public void Test_Certificate_OPENSSH_RSA()
        {
            PrivateKeyFile pkFile;

            using (var privateKeyStream = GetData("Key.OPENSSH.RSA.txt"))
            using (var certificateStream = GetData("Key.OPENSSH.RSA-cert.pub"))
            {
                pkFile = new PrivateKeyFile(privateKeyStream, passPhrase: null, certificateStream);
            }

            Certificate cert = pkFile.Certificate;

            // ssh-keygen -L -f Key.OPENSSH.RSA-cert.pub

            Assert.AreEqual("ssh-rsa-cert-v01@openssh.com", cert.Name);

            Assert.IsInstanceOfType<RsaKey>(cert.Key);
            CollectionAssert.AreEqual(((RsaKey)pkFile.Key).Public, ((RsaKey)cert.Key).Public);
            Assert.AreEqual(0UL, cert.Serial);
            Assert.AreEqual(Certificate.CertificateType.User, cert.Type);
            Assert.AreEqual("rsa-cert-rsa", cert.KeyId);
            CollectionAssert.AreEqual(new string[] { "sshnet" }, cert.ValidPrincipals.ToList());
            Assert.AreEqual(0, cert.CriticalOptions.Count);
            Assert.IsTrue(cert.ValidAfter.EqualsExact(new DateTimeOffset(2024, 07, 17, 20, 50, 34, TimeSpan.Zero)));
            Assert.AreEqual(ulong.MaxValue, cert.ValidBeforeUnixSeconds);
            Assert.AreEqual(DateTimeOffset.MaxValue, cert.ValidBefore);
            CollectionAssert.AreEqual(new Dictionary<string, string>
            {
                ["permit-X11-forwarding"] = "",
                ["permit-agent-forwarding"] = "",
                ["permit-port-forwarding"] = "",
                ["permit-pty"] = "",
                ["permit-user-rc"] = "",
            }, new Dictionary<string, string>(cert.Extensions));
            Assert.AreEqual("NqLEgdYti0XjUkYjGyQv2Ddy1O5v2NZDZFRtlfESLIA", cert.CertificateAuthorityKeyFingerPrint);

            Assert.AreEqual(6, pkFile.HostKeyAlgorithms.Count);

            var algorithms = pkFile.HostKeyAlgorithms.ToList();

            Assert.AreEqual("rsa-sha2-512-cert-v01@openssh.com", algorithms[0].Name);
            Assert.AreEqual("rsa-sha2-256-cert-v01@openssh.com", algorithms[1].Name);
            Assert.AreEqual("ssh-rsa-cert-v01@openssh.com", algorithms[2].Name);
            Assert.AreEqual("ssh-rsa", algorithms[3].Name);
            Assert.AreEqual("rsa-sha2-512", algorithms[4].Name);
            Assert.AreEqual("rsa-sha2-256", algorithms[5].Name);
        }

        [TestMethod]
        public void Test_CertificateKeyMismatch()
        {
            using (var privateKey = GetData("Key.OPENSSH.RSA.txt"))
            using (var certificate = GetData("Key.OPENSSH.ECDSA521-cert.pub"))
            {
                Assert.ThrowsExactly<ArgumentException>(() => new PrivateKeyFile(privateKey, passPhrase: null, certificate));
            }
        }

        [TestMethod]
        public void Test_Certificate_OPENSSH_ECDSA()
        {
            PrivateKeyFile pkFile;

            using (var privateKeyStream = GetData("Key.OPENSSH.ECDSA521.txt"))
            using (var certificateStream = GetData("Key.OPENSSH.ECDSA521-cert.pub"))
            {
                pkFile = new PrivateKeyFile(privateKeyStream, passPhrase: null, certificateStream);
            }

            Certificate cert = pkFile.Certificate;

            // ssh-keygen -L -f Key.OPENSSH.ECDSA521-cert.pub

            Assert.AreEqual("ecdsa-sha2-nistp521-cert-v01@openssh.com", cert.Name);

            Assert.IsInstanceOfType<EcdsaKey>(cert.Key);
            CollectionAssert.AreEqual(((EcdsaKey)pkFile.Key).Public, ((EcdsaKey)cert.Key).Public);
            Assert.AreEqual(0UL, cert.Serial);
            Assert.AreEqual(Certificate.CertificateType.User, cert.Type);
            Assert.AreEqual("ecdsa521certEcdsa", cert.KeyId);
            CollectionAssert.AreEqual(new string[] { "sshnet" }, cert.ValidPrincipals.ToList());
            Assert.AreEqual(0, cert.CriticalOptions.Count);
            Assert.AreEqual(0UL, cert.ValidAfterUnixSeconds);
            Assert.IsTrue(cert.ValidAfter.EqualsExact(UnixEpoch));
            Assert.AreEqual(ulong.MaxValue, cert.ValidBeforeUnixSeconds);
            Assert.AreEqual(DateTimeOffset.MaxValue, cert.ValidBefore);
            CollectionAssert.AreEqual(new Dictionary<string, string>
            {
                ["permit-X11-forwarding"] = "",
                ["permit-agent-forwarding"] = "",
                ["permit-port-forwarding"] = "",
                ["permit-pty"] = "",
                ["permit-user-rc"] = "",
            }, new Dictionary<string, string>(cert.Extensions));
            Assert.AreEqual("r/t6I+bZQzN5BhSuntFSHDHlrnNHVM2lAo6gbvynG/4", cert.CertificateAuthorityKeyFingerPrint);

            Assert.AreEqual(2, pkFile.HostKeyAlgorithms.Count);

            var algorithms = pkFile.HostKeyAlgorithms.ToList();

            Assert.AreEqual("ecdsa-sha2-nistp521-cert-v01@openssh.com", algorithms[0].Name);
            Assert.AreEqual("ecdsa-sha2-nistp521", algorithms[1].Name);
        }

        [TestMethod]
        public void Test_LowercaseSalt()
        {
            // Occurs occasionally in keys generated from older BouncyCastle versions

            string pk = """
            -----BEGIN RSA PRIVATE KEY-----
            Proc-Type: 4,ENCRYPTED
            DEK-Info: AES-256-CBC,063de67ae11456c89bce9d4a21be3dfb

            6mS1GhCjAg5mEwMFcKRJwg1uxCeY3ekJNCQewIN9NSI5A8prBOQ+JSyWAsn6c3Gw
            OeRyur+5dxMFdt5Hz1CBi9EePvhVyMry7U5U86BWB0HgtDAD02b324sfc6Wk+kj5
            PZvuKyXDiqdwy0rsbBUT+bLtXjCI4Ws1k/KbbF0OqGhFJJvErNU5x8zMD9mqp92R
            D8ZZ/F8Sks3V/JeUisAF86sgMfVCELJobn5Zq/IaUyzQwC6IEL+Sy5fSBB5NHiex
            NDIJg2RW79uLbufCpuoMPS/GKydf4dq0L5MwvKeqtUgf9Wddc+ZAE4+q1Xz/T8iN
            3IMqsQfVbYjVK7uTaVGKH+Ew77Qryj01Vg+zyzdf4UwOV3XXQKLVCjNxpMCVtoq7
            S45M3Ad7598vb7ooa/BFCIcEM8TkuzPnuttLqjzXEzUcA5kqm3kV14IKtlexBfNT
            tarbidlZcOinvJaoIT3baP4rVnEWDKcxpc+UzNU5RRty6l0zpRmw/9RQ5+FKreh8
            eXDHD8TT8ArdaREFM8J2OGpkmIK5sLhhYi9gnTopmKIHn8OAXusmQosEOzS6kGxk
            aFtZezXSCBGgXp5RsrBGGx3oXWHGuWbEFXAq+M7PKXMQe5rLRv6sQdfTFSB5hgNK
            82P8UzV1wWtAX4JYAhRh2zA8agY2arbNvbjRyjSbp9HNVBgSbVQ60JInesOqLxEg
            XURuCYp4F8AeHzyO805MTNpcX7PZT2kOxp9sKKABJ9BJ0RoSWa0LJqXzGCHvrExE
            g7XY/ZfDFZlPLbQnrOgVlYh7pzyfyKB74/oXHkonAisRfsgnQ87yT2DmcHNP6Cek
            eae2nrpx2yn9Bf8rYdpmJgNxduO8IZvpn84xEyPqK+FbQsdOefBvsg5TgfzETkh/
            SJjzbqCTDa3XHEUCInixo/wT7FxT8KR9vk43FGPNVRUvPB2GNxe9ZwLYIir64hcQ
            CpdA3ipVx4/jVzWQH8KXG9UP9TDAKXEvbndLnr2taPnUdAnznwHN2EkfzS/PrFG4
            /j3l1+VY2AyRybbCTI2iuwJPnKdxOR5oWW6I2Ksfq93Oy+NQz/zasjyNpCZBZWds
            5gBmwiNk2Xzq7ikEVtVk3osOQRw/u9GbretfaT9jtClALL3DFbOzL4WxA+0NJqpd
            NB2MohOJa1BJjdfh6x6EVhugH85Y9uYyz/MQj7piljAJY96190n3Q86b/7phfwuD
            A/ixS42nqpyOPO+EjiWFerFVTJ3iBj7GXXOZGwCrZfpTbqE7OdTDnE3Vr4MO/Etq
            kSDmJ/+4SFFh80YwYVERDNFdDxCYxx5AnxaBFwbqjzatTV/btgGVabIf6zm2L6aY
            BJ5wnBZnRnsRaIMehDQmTjioMcyHBSMqId+LYQp+KFpBXqXQTEjJPnq+t4o2FF/N
            8yoKR8BX6HXSO5qUndI8emwec1JeveiRai6SDnEz1EFfetYXImR290mlqt0aRjQk
            t/HXRv+fmDQk5hJbCPICydcVSRyrbzxKkppVceEf9NwkBT1MBsOZIFJ3s3A9I72n
            XPIab5czlgSLYA/U9nEg2XU21hKD2kRH1OF0WSlpNhN2SJFViVqlC3v36MgHoWNh
            -----END RSA PRIVATE KEY-----
            """;

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(pk)))
            {
                var pkFile = new PrivateKeyFile(stream, "12345");

                TestRsaKeyFile(pkFile);
            }
        }

        [TestMethod]
        public void PuTTYv2_InvalidMac_ThrowsSshException()
        {
            string pk = """
            PuTTY-User-Key-File-2: ssh-rsa
            Encryption: none
            Comment: Key.OPENSSH.RSA
            Public-Lines: 6
            AAAAB3NzaC1yc2EAAAADAQABAAABAQDtbs6KCLsePWaxraXweKYs/NqBWYT8Kx4w
            oJHE8xO1ZO+hl0y3uF+S2FYDuHbRruhJJ4fa3sWp46lU0YVi9FXcFVawpkkxFx0m
            JMJkCMffytiT3Re9neYqso3/d9xCyHg6I+dapPodKqDXiiJXxQ+1TCcTrmyRZLG/
            G34QuVWkKobm8TY78Y0MpATsXNi3q9CKEwVIAEGqO9q7SaNfTTYpiIIyvq+CXxdi
            QMDifn4nJBJDHOed+sv3dmhqq6NE/ZtPlSFeBvOvwcXC6pAa9REQJlNMjwGK//q0
            4if3HaERo3q/EMu1dz30TZ3o1bpx2uLBoYUniOBVYMTmZTTTpd09
            Private-Lines: 14
            AAABAQDpeCr6CmnM632eu2zPkCN/W0eVJ6yftdpi4JFWA9veY5lK4RbcFR1NrRKv
            Z+TWfNIGlSt+qc3eJ3IraDdsPWxsFEOBQpH4Bo1wI3dOnF/GDJV4mFAu8SQR2i/N
            BFR/CtdF/GYTeOREZ9Vu/HKWsbynfnFyZfJ16XjqvaLx2PyAhje0qnREy9nhmU1u
            FYc93k7HIdYv17eBs5LIjKNCBMpl7OHMStL9f8on9dirPIECo2pnZGDWQqIdGUdL
            ooQja3IXBh+H5Fvov3FyHVKo61CFNaKubFLbl2kYPaOBqVd7KLDw+a6pOJYKpSZQ
            zHox0Xe0WyKuvngrhAD2Sox5pEu1AAAAgQD+dPDqesFjwMJ9SXwWbqkLY3H5yXje
            DZGEAXcm59L1buVHcqkkC2vIZQM0ToQPqib65bGYDPYfAsi08ropvJYpGR6HMDtd
            8wU3VWkPHNpSb39rl0yFzWR7HkuyE5HwYjtYUgeM/EQ5Dq9+Zhn3W8iSBQMBWReF
            7PFp0BfrxxGnawAAAIEA7t9vXgsFRX/YNMzR32bt9adFrRK3LEb+e36vlKD7aL/J
            8VBe9aDlnuSkhpxrTCAiN9ZAbT4VG73zprqja4CQY4I2z0JotMUgBOS90LhCkTY5
            WhN/1mnSgcM4SQ7WrrmJNYn5K3QFaeu18kOabsrhoFWkATT268QPYNSG8ni+P/cA
            AACBALFEE9FIau5dLoE3eGPfPWx+nltH6Jdtf5uwec5CUHqTWnVD07NfPLr7+Ip1
            vJ9jt0Qmp11h2XwidQLEfzBBFtgukA7b6ilx2831kJQmElcQdewo1ESmvHzWiAJP
            fM4JjTcDudzQZXsq1IT4L5t8bewAoKc12OUcDSS/P2tFjpoM
            Private-MAC: 7f487d19cb5d03257c9b9a2aaaaaaaaaaaaaaaaa
            """;

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(pk)))
            {
                var ex = Assert.ThrowsExactly<SshException>(() => new PrivateKeyFile(stream));

                Assert.AreEqual("MAC verification failed for PuTTY key file", ex.Message);
            }
        }

        [TestMethod]
        public void PuTTYv3_InvalidMac_ThrowsSshException()
        {
            string pk = """
            PuTTY-User-Key-File-3: ssh-rsa
            Encryption: none
            Comment: Key.OPENSSH.RSA
            Public-Lines: 6
            AAAAB3NzaC1yc2EAAAADAQABAAABAQDtbs6KCLsePWaxraXweKYs/NqBWYT8Kx4w
            oJHE8xO1ZO+hl0y3uF+S2FYDuHbRruhJJ4fa3sWp46lU0YVi9FXcFVawpkkxFx0m
            JMJkCMffytiT3Re9neYqso3/d9xCyHg6I+dapPodKqDXiiJXxQ+1TCcTrmyRZLG/
            G34QuVWkKobm8TY78Y0MpATsXNi3q9CKEwVIAEGqO9q7SaNfTTYpiIIyvq+CXxdi
            QMDifn4nJBJDHOed+sv3dmhqq6NE/ZtPlSFeBvOvwcXC6pAa9REQJlNMjwGK//q0
            4if3HaERo3q/EMu1dz30TZ3o1bpx2uLBoYUniOBVYMTmZTTTpd09
            Private-Lines: 14
            AAABAQDpeCr6CmnM632eu2zPkCN/W0eVJ6yftdpi4JFWA9veY5lK4RbcFR1NrRKv
            Z+TWfNIGlSt+qc3eJ3IraDdsPWxsFEOBQpH4Bo1wI3dOnF/GDJV4mFAu8SQR2i/N
            BFR/CtdF/GYTeOREZ9Vu/HKWsbynfnFyZfJ16XjqvaLx2PyAhje0qnREy9nhmU1u
            FYc93k7HIdYv17eBs5LIjKNCBMpl7OHMStL9f8on9dirPIECo2pnZGDWQqIdGUdL
            ooQja3IXBh+H5Fvov3FyHVKo61CFNaKubFLbl2kYPaOBqVd7KLDw+a6pOJYKpSZQ
            zHox0Xe0WyKuvngrhAD2Sox5pEu1AAAAgQD+dPDqesFjwMJ9SXwWbqkLY3H5yXje
            DZGEAXcm59L1buVHcqkkC2vIZQM0ToQPqib65bGYDPYfAsi08ropvJYpGR6HMDtd
            8wU3VWkPHNpSb39rl0yFzWR7HkuyE5HwYjtYUgeM/EQ5Dq9+Zhn3W8iSBQMBWReF
            7PFp0BfrxxGnawAAAIEA7t9vXgsFRX/YNMzR32bt9adFrRK3LEb+e36vlKD7aL/J
            8VBe9aDlnuSkhpxrTCAiN9ZAbT4VG73zprqja4CQY4I2z0JotMUgBOS90LhCkTY5
            WhN/1mnSgcM4SQ7WrrmJNYn5K3QFaeu18kOabsrhoFWkATT268QPYNSG8ni+P/cA
            AACBALFEE9FIau5dLoE3eGPfPWx+nltH6Jdtf5uwec5CUHqTWnVD07NfPLr7+Ip1
            vJ9jt0Qmp11h2XwidQLEfzBBFtgukA7b6ilx2831kJQmElcQdewo1ESmvHzWiAJP
            fM4JjTcDudzQZXsq1IT4L5t8bewAoKc12OUcDSS/P2tFjpoM
            Private-MAC: ef76b1cf66a4a28d6fe08c70012c4bfa61771502e496d227dddddddddddddddd
            """;

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(pk)))
            {
                var ex = Assert.ThrowsExactly<SshException>(() => new PrivateKeyFile(stream));

                Assert.AreEqual("MAC verification failed for PuTTY key file", ex.Message);
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
            Assert.IsInstanceOfType<RsaKey>(rsaPrivateKeyFile.Key);
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
