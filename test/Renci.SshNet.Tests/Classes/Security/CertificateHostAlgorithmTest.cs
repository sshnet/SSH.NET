using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security
{
    [TestClass]
    public class CertificateHostAlgorithmTest : TestBase
    {
        private static IReadOnlyDictionary<string, Func<byte[], KeyHostAlgorithm>> DefaultKeyAlgs
        {
            get
            {
                return new Dictionary<string, Func<byte[], KeyHostAlgorithm>>(
                    new PasswordConnectionInfo("x", "y", "z").HostKeyAlgorithms);
            }
        }

        [TestMethod]
        public void SshRsa_SignAndVerify()
        {
            byte[] data = Encoding.UTF8.GetBytes("hello world");

            (RsaKey key, Certificate certificate) = GetRsaKey();

            CertificateHostAlgorithm algorithm = new("ssh-rsa-cert-v01@openssh.com", key, certificate);

            byte[] expectedEncodedSignatureBytes = new byte[]
            {
                0, 0, 0, 7, // byte count of "ssh-rsa"
                (byte)'s', (byte)'s', (byte)'h', (byte)'-', (byte)'r', (byte)'s', (byte)'a', // ssh-rsa
                0, 0, 1, 0, // byte count of signature (=256)

                // ssh-keygen -e -f Key.OPENSSH.RSA.txt -m PEM -p
                // echo -n 'hello world' | openssl dgst -sha1 -sign Key.OPENSSH.RSA.txt -out test.signed
                0x2d, 0x54, 0x2e, 0x6a, 0x5f, 0x7c, 0x29, 0x7d, 0x2d, 0x81, 0xf6, 0x34, 0x45, 0x7a, 0x3f, 0xd0,
                0xa5, 0x06, 0x55, 0x9c, 0xab, 0x8c, 0x28, 0x76, 0x27, 0xc0, 0x8a, 0x32, 0x23, 0xa4, 0x62, 0xd1,
                0x8c, 0x72, 0x05, 0x52, 0x47, 0x4d, 0xd0, 0xde, 0x86, 0xdd, 0xfc, 0x38, 0x54, 0x47, 0x4e, 0x17,
                0xef, 0x6b, 0x9a, 0x2e, 0x4d, 0x55, 0xf3, 0x2a, 0x11, 0xa7, 0x3a, 0x8b, 0x37, 0xbb, 0x61, 0x2d,
                0xb8, 0x4c, 0x1f, 0xa1, 0x0f, 0xb4, 0xbe, 0x06, 0xea, 0xc1, 0x4e, 0x17, 0x3c, 0x53, 0x01, 0x1b,
                0x41, 0x3b, 0x3c, 0x86, 0xb7, 0x55, 0x4d, 0xe6, 0xcb, 0x9d, 0x0e, 0x6f, 0x18, 0x10, 0x63, 0x3c,
                0xcd, 0x02, 0x32, 0x9f, 0xbe, 0x58, 0x22, 0xa1, 0x24, 0x61, 0xf3, 0x1e, 0xa8, 0xbd, 0xf7, 0x0e,
                0x9a, 0xeb, 0x42, 0x5c, 0xf5, 0xdb, 0x3b, 0x65, 0x22, 0xb1, 0x54, 0x7f, 0xe0, 0x62, 0xae, 0xb3,
                0xab, 0x7b, 0xfe, 0x4b, 0x80, 0x7a, 0xd1, 0x5e, 0xd2, 0x0a, 0xa3, 0x4d, 0x1a, 0xf5, 0xa8, 0xbf,
                0x87, 0xfc, 0x91, 0x57, 0xf1, 0xc2, 0x58, 0xea, 0x7a, 0xbc, 0xdf, 0x86, 0xb4, 0x24, 0x32, 0x10,
                0x72, 0x2e, 0x91, 0x15, 0xa7, 0x39, 0xb5, 0x22, 0x7a, 0xe1, 0x88, 0xbd, 0x23, 0xa6, 0x05, 0xe2,
                0x20, 0x22, 0x46, 0x68, 0x56, 0x34, 0x2e, 0x08, 0x35, 0xa7, 0x4b, 0x4f, 0x54, 0xcb, 0xf9, 0x53,
                0xd1, 0x41, 0xf6, 0xac, 0x23, 0xf8, 0x0e, 0x90, 0x1e, 0xea, 0x4c, 0xdb, 0xa3, 0xb6, 0xdb, 0x5f,
                0xf9, 0xc4, 0xf3, 0x08, 0x12, 0x32, 0xa8, 0xa2, 0xa1, 0x8c, 0x1d, 0x5f, 0xf7, 0x18, 0x79, 0x4c,
                0xd4, 0x28, 0xc6, 0xe9, 0x55, 0xbc, 0x80, 0xc2, 0x08, 0x1f, 0x8f, 0x8d, 0x35, 0x0b, 0xa9, 0x49,
                0x80, 0xba, 0x32, 0xba, 0xe0, 0xf6, 0x2f, 0x7f, 0xf2, 0xb7, 0xaf, 0xfa, 0xfd, 0xc8, 0x7a, 0x66,
            };

            CollectionAssert.AreEqual(expectedEncodedSignatureBytes, algorithm.Sign(data));

            algorithm = new CertificateHostAlgorithm(
                "ssh-rsa-cert-v01@openssh.com",
                certificate,
                DefaultKeyAlgs);

            Assert.IsTrue(algorithm.VerifySignature(data, expectedEncodedSignatureBytes));
        }

        [TestMethod]
        public void RsaSha256_SignAndVerify()
        {
            byte[] data = Encoding.UTF8.GetBytes("hello world");

            (RsaKey key, Certificate certificate) = GetRsaKey();

            CertificateHostAlgorithm algorithm = new(
                "rsa-sha2-256-cert-v01@openssh.com",
                key,
                certificate,
                new RsaDigitalSignature(key, HashAlgorithmName.SHA256));

            byte[] expectedEncodedSignatureBytes = new byte[]
            {
                0, 0, 0, 12, // byte count of "rsa-sha2-256"
                (byte)'r', (byte)'s', (byte)'a', (byte)'-', (byte)'s', (byte)'h', (byte)'a', (byte)'2',
                (byte)'-', (byte)'2', (byte)'5', (byte)'6',
                0, 0, 1, 0, // byte count of signature (=256)
                
                // ssh-keygen -e -f Key.OPENSSH.RSA.txt -m PEM -p
                // echo -n 'hello world' | openssl dgst -sha256 -sign Key.OPENSSH.RSA.txt -out test.signed
                0x18, 0xf4, 0x3e, 0xa9, 0xdf, 0x89, 0x92, 0x6b, 0xc1, 0x6a, 0x35, 0x72, 0x42, 0x56, 0xf7, 0x50,
                0x32, 0x33, 0xff, 0xc4, 0x91, 0x3d, 0x49, 0x12, 0x37, 0x52, 0x98, 0x37, 0xb8, 0xeb, 0xeb, 0xaa,
                0xe5, 0x4e, 0xd4, 0x99, 0x74, 0xfd, 0xea, 0xd6, 0x8f, 0x34, 0xa0, 0x3a, 0x0e, 0xfd, 0xcb, 0xae,
                0x04, 0x20, 0x01, 0x1c, 0x67, 0x98, 0x94, 0x6c, 0xdb, 0x26, 0x9a, 0x0c, 0x5b, 0xcf, 0x9a, 0x06,
                0xa5, 0x90, 0xfb, 0x62, 0xe8, 0x56, 0x91, 0xdf, 0x63, 0x1f, 0xc3, 0xb1, 0xd3, 0x4f, 0x18, 0x2b,
                0x2e, 0xfa, 0xb4, 0x61, 0x1d, 0x54, 0xdd, 0x63, 0x14, 0x17, 0x31, 0x8e, 0x86, 0xe3, 0xc2, 0xb1,
                0x30, 0x42, 0x1e, 0x5a, 0x43, 0x87, 0x54, 0x64, 0xd5, 0xbb, 0xcb, 0x37, 0x7b, 0xa6, 0x97, 0x75,
                0xca, 0x3b, 0x0d, 0xb2, 0x24, 0x34, 0x0b, 0xfc, 0xde, 0x67, 0xbf, 0xdf, 0x2a, 0x8b, 0xc6, 0xac,
                0x51, 0x0d, 0x98, 0x54, 0xed, 0x57, 0x5e, 0xa9, 0xbe, 0x0f, 0x0c, 0x0f, 0x30, 0x23, 0x96, 0x83,
                0x65, 0x74, 0x87, 0x91, 0x99, 0x21, 0x88, 0x80, 0x6d, 0xe4, 0xec, 0xcb, 0x51, 0xe5, 0xe5, 0x3a,
                0x2b, 0x34, 0x9b, 0x10, 0x70, 0xef, 0x57, 0x40, 0x59, 0x45, 0x94, 0x58, 0xd0, 0x65, 0x84, 0x23,
                0x5e, 0xcd, 0x49, 0xea, 0x18, 0x51, 0x29, 0xdd, 0x84, 0x05, 0x24, 0xe4, 0x65, 0x0c, 0x38, 0x8e,
                0x42, 0x33, 0xdf, 0xcb, 0x3c, 0xa0, 0x0d, 0xe2, 0x2d, 0x13, 0xbd, 0xea, 0x51, 0x06, 0xdd, 0x61,
                0x87, 0x05, 0xbe, 0xef, 0xaa, 0x77, 0xe4, 0xef, 0x25, 0x6b, 0xbf, 0x24, 0xd7, 0xe4, 0xba, 0x25,
                0x28, 0x49, 0x26, 0xc2, 0x31, 0xca, 0xbb, 0x1a, 0x2c, 0x19, 0xa3, 0x7b, 0x62, 0x12, 0x59, 0x75,
                0x12, 0x03, 0x38, 0xc9, 0x69, 0x93, 0xe6, 0xec, 0xc8, 0x13, 0x25, 0x48, 0xd2, 0x6c, 0x67, 0x10,
            };

            CollectionAssert.AreEqual(expectedEncodedSignatureBytes, algorithm.Sign(data));

            algorithm = new CertificateHostAlgorithm(
                "rsa-sha2-256-cert-v01@openssh.com",
                certificate,
                new RsaDigitalSignature((RsaKey)certificate.Key, HashAlgorithmName.SHA256),
                DefaultKeyAlgs);

            Assert.IsTrue(algorithm.VerifySignature(data, expectedEncodedSignatureBytes));
        }

        [TestMethod]
        public void RsaSha512_SignAndVerify()
        {
            byte[] data = Encoding.UTF8.GetBytes("hello world");

            (RsaKey key, Certificate certificate) = GetRsaKey();

            CertificateHostAlgorithm algorithm = new(
                "rsa-sha2-512-cert-v01@openssh.com",
                key,
                certificate,
                new RsaDigitalSignature(key, HashAlgorithmName.SHA512));

            byte[] expectedEncodedSignatureBytes = new byte[]
            {
                0, 0, 0, 12, // byte count of "rsa-sha2-512"
                (byte)'r', (byte)'s', (byte)'a', (byte)'-', (byte)'s', (byte)'h', (byte)'a', (byte)'2',
                (byte)'-', (byte)'5', (byte)'1', (byte)'2',
                0, 0, 1, 0, // byte count of signature (=256)
                
                // ssh-keygen -e -f Key.OPENSSH.RSA.txt -m PEM -p
                // echo -n 'hello world' | openssl dgst -sha512 -sign Key.OPENSSH.RSA.txt -out test.signed
                0x1d, 0x64, 0xc6, 0x82, 0xb0, 0xc4, 0x2b, 0xe1, 0x71, 0x13, 0x1f, 0x62, 0xab, 0x8f, 0xf8, 0x72,
                0x43, 0xe8, 0x95, 0x4c, 0x8d, 0xa6, 0xf7, 0xcd, 0x62, 0xc9, 0x6f, 0xe5, 0xbf, 0x23, 0x1b, 0xc7,
                0xa0, 0x93, 0xc6, 0xc0, 0xa2, 0x06, 0x2d, 0x07, 0x16, 0x59, 0xbc, 0x0d, 0xe5, 0x00, 0x39, 0x56,
                0xa7, 0xde, 0x4b, 0x17, 0xf4, 0x02, 0xf6, 0x5d, 0x8f, 0xc5, 0x76, 0xe2, 0xb7, 0xae, 0xe5, 0xa2,
                0x7f, 0xd8, 0x34, 0x04, 0x2c, 0xbc, 0xdf, 0x84, 0x51, 0x69, 0x83, 0xda, 0x7a, 0x74, 0x19, 0xe9,
                0x6e, 0x02, 0xf8, 0x51, 0x20, 0xa2, 0x67, 0x43, 0xbb, 0xde, 0x7a, 0xa7, 0x12, 0xe7, 0x89, 0x7c,
                0x50, 0xf3, 0xd5, 0x07, 0xc9, 0x70, 0x22, 0xed, 0x2e, 0x45, 0x1e, 0x49, 0x23, 0x94, 0x69, 0xae,
                0x8f, 0x5d, 0x3b, 0x34, 0xdb, 0xc8, 0x49, 0x26, 0x09, 0x81, 0x7d, 0xad, 0x77, 0xb5, 0x6d, 0xad,
                0x0c, 0x9f, 0x66, 0x29, 0x56, 0xff, 0xea, 0xa7, 0x6f, 0x7f, 0xcd, 0xc0, 0x15, 0x05, 0xdc, 0xee,
                0xfb, 0xac, 0xfd, 0x59, 0x19, 0x30, 0x32, 0x6e, 0x16, 0xe0, 0x4e, 0x74, 0x6a, 0x13, 0xa7, 0x9f,
                0x5b, 0x71, 0x75, 0x13, 0xcf, 0xa5, 0xf3, 0x07, 0x8f, 0xfb, 0xa2, 0xa2, 0x92, 0xc2, 0x41, 0xc4,
                0xbc, 0x14, 0x75, 0x22, 0xe3, 0x4b, 0xb7, 0xc0, 0x54, 0xc3, 0x25, 0x87, 0xbb, 0x52, 0xde, 0x70,
                0x69, 0xc6, 0x68, 0x66, 0x3a, 0x88, 0xf6, 0x3b, 0x8e, 0x44, 0x00, 0x25, 0x17, 0xc9, 0x44, 0x7c,
                0xcc, 0x0c, 0x63, 0xab, 0xa3, 0x2c, 0xaa, 0x4c, 0x34, 0xda, 0xe0, 0x96, 0x71, 0x83, 0xe5, 0x7a,
                0xec, 0x56, 0xbe, 0x85, 0x27, 0x7c, 0xe7, 0x79, 0xfd, 0xb8, 0x77, 0x41, 0x05, 0x25, 0x30, 0x57,
                0x24, 0x45, 0xa9, 0x12, 0x9e, 0xdc, 0x9e, 0x23, 0x43, 0x13, 0x67, 0x38, 0x59, 0xae, 0x4b, 0x76,
            };

            CollectionAssert.AreEqual(expectedEncodedSignatureBytes, algorithm.Sign(data));

            algorithm = new CertificateHostAlgorithm(
                "rsa-sha2-512-cert-v01@openssh.com",
                certificate,
                new RsaDigitalSignature((RsaKey)certificate.Key, HashAlgorithmName.SHA512),
                DefaultKeyAlgs);

            Assert.IsTrue(algorithm.VerifySignature(data, expectedEncodedSignatureBytes));
        }

        [TestMethod]
        public void CertificateBadCASignature_VerifySignatureReturnsFalse()
        {
            // ssh-keygen -s Key.OPENSSH.ED25519.txt -I test Key.OPENSSH.ECDSA.txt
            string goodCertString = "ecdsa-sha2-nistp256-cert-v01@openssh.com " +
                "AAAAKGVjZHNhLXNoYTItbmlzdHAyNTYtY2VydC12MDFAb3BlbnNzaC5jb20AA" +
                "AAg1vQFCYTYufJiCBFJBWc63sOGwnJ3BHQn4ig499dtB0AAAAAIbmlzdHAyNT" +
                "YAAABBBI/dlNvfssW9KYrB67TcDmz9zBzDf7eMvUupAroP3b3FjUnYnpL3Utc" +
                "4GkF/PiX7w2DuxaG70/+EX/CYHZBHKCsAAAAAAAAAAAAAAAEAAAAEdGVzdAAA" +
                "AAAAAAAAAAAAAP//////////AAAAAAAAAIIAAAAVcGVybWl0LVgxMS1mb3J3Y" +
                "XJkaW5nAAAAAAAAABdwZXJtaXQtYWdlbnQtZm9yd2FyZGluZwAAAAAAAAAWcG" +
                "VybWl0LXBvcnQtZm9yd2FyZGluZwAAAAAAAAAKcGVybWl0LXB0eQAAAAAAAAA" +
                "OcGVybWl0LXVzZXItcmMAAAAAAAAAAAAAADMAAAALc3NoLWVkMjU1MTkAAAAg" +
                "DQlmcNCvFBlw0At9lgbss8BbUxgQa9VbmeN7s6UwYyIAAABTAAAAC3NzaC1lZ" +
                "DI1NTE5AAAAQA8+LXQ++nb1/gNEtURKt5Yo/geUc/+3+Bv3EPGno5JhxvekjJ" +
                "PD7/nXcyxnY3zALlPQTxb19EVx5lz58BS96gg=";

            char[] chars = goodCertString.ToCharArray();
            chars[^10] = 'a';
            string badCertString = new string(chars);

            Assert.IsTrue(VerifySignature(goodCertString));
            Assert.IsFalse(VerifySignature(badCertString));

            static bool VerifySignature(string certString)
            {
                PrivateKeyFile pk;

                using (Stream keyStream = GetData("Key.OPENSSH.ECDSA.txt"))
                using (MemoryStream certStream = new MemoryStream(Encoding.UTF8.GetBytes(certString)))
                {
                    pk = new PrivateKeyFile(keyStream, null, certStream);
                }

                Assert.IsNotNull(pk.Certificate);

                byte[] data = Encoding.UTF8.GetBytes("hello world");

                CertificateHostAlgorithm certificateAlgorithm = new(
                    "ecdsa-sha2-nistp256-cert-v01@openssh.com",
                    pk.Certificate,
                    DefaultKeyAlgs);

                KeyHostAlgorithm keyHostAlgorithm = new KeyHostAlgorithm("ecdsa-sha2-nistp256", pk.Key);

                byte[] signature = keyHostAlgorithm.Sign(data);

                Assert.IsTrue(keyHostAlgorithm.VerifySignature(data, signature));

                return certificateAlgorithm.VerifySignature(data, signature);
            }
        }

        [TestMethod]
        public void CertificateValidityPeriodExpired_VerifySignatureReturnsFalse()
        {
            // ssh-keygen -s Key.OPENSSH.ED25519.txt -I nolongervalid -V always:20240101 Key.OPENSSH.ECDSA.txt
            string certString = "ecdsa-sha2-nistp256-cert-v01@openssh.com " +
                "AAAAKGVjZHNhLXNoYTItbmlzdHAyNTYtY2VydC12MDFAb3BlbnNzaC5jb" +
                "20AAAAg5BUo6CqGzTDc0UgNcLUqna2bH3C69NZCzd9CrQ8apQUAAAAIbm" +
                "lzdHAyNTYAAABBBI/dlNvfssW9KYrB67TcDmz9zBzDf7eMvUupAroP3b3" +
                "FjUnYnpL3Utc4GkF/PiX7w2DuxaG70/+EX/CYHZBHKCsAAAAAAAAAAAAA" +
                "AAEAAAANbm9sb25nZXJ2YWxpZAAAAAAAAAAAAAAAAAAAAABlkgCAAAAAA" +
                "AAAAIIAAAAVcGVybWl0LVgxMS1mb3J3YXJkaW5nAAAAAAAAABdwZXJtaX" +
                "QtYWdlbnQtZm9yd2FyZGluZwAAAAAAAAAWcGVybWl0LXBvcnQtZm9yd2F" +
                "yZGluZwAAAAAAAAAKcGVybWl0LXB0eQAAAAAAAAAOcGVybWl0LXVzZXIt" +
                "cmMAAAAAAAAAAAAAADMAAAALc3NoLWVkMjU1MTkAAAAgDQlmcNCvFBlw0" +
                "At9lgbss8BbUxgQa9VbmeN7s6UwYyIAAABTAAAAC3NzaC1lZDI1NTE5AA" +
                "AAQMonLi0J282GmuMVyHGKS/PRoLpdj5GgmR0wrIkExRRCzKZaycLfPDL" +
                "+CGMa2jsH2QhFhTCG5AtKWVQbkqdHVAY= (null)";

            PrivateKeyFile pk;

            using (Stream keyStream = GetData("Key.OPENSSH.ECDSA.txt"))
            using (MemoryStream certStream = new MemoryStream(Encoding.UTF8.GetBytes(certString)))
            {
                pk = new PrivateKeyFile(keyStream, null, certStream);
            }

            Assert.IsNotNull(pk.Certificate);
            Assert.AreEqual(0uL, pk.Certificate.ValidAfterUnixSeconds);
            Assert.AreEqual(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), pk.Certificate.ValidBefore);

            byte[] data = Encoding.UTF8.GetBytes("hello world");

            CertificateHostAlgorithm certificateAlgorithm = new(
                "ecdsa-sha2-nistp256-cert-v01@openssh.com",
                pk.Certificate,
                DefaultKeyAlgs);

            KeyHostAlgorithm keyHostAlgorithm = new KeyHostAlgorithm("ecdsa-sha2-nistp256", pk.Key);

            byte[] signature = keyHostAlgorithm.Sign(data);

            Assert.IsTrue(keyHostAlgorithm.VerifySignature(data, signature));
            Assert.IsFalse(certificateAlgorithm.VerifySignature(data, signature));
        }

        private static (RsaKey Key, Certificate Certificate) GetRsaKey()
        {
            using (Stream keyStream = GetData("Key.OPENSSH.RSA.txt"))
            using (Stream certStream = GetData("Key.OPENSSH.RSA-cert.pub"))
            {
                var pkFile = new PrivateKeyFile(keyStream, null, certStream);
                return (Key: (RsaKey)pkFile.Key, pkFile.Certificate);
            }
        }
    }
}
