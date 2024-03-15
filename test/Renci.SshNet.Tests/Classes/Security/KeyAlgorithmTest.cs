﻿﻿using System.Security.Cryptography;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security
{
    [TestClass]
    internal class KeyHostAlgorithmTest : TestBase
    {
        [TestMethod]
        public void NoSuppliedDigitalSignature_PropertyIsKeyDigitalSignature()
        {
            RsaKey rsaKey = GetRsaKey();

            KeyHostAlgorithm keyHostAlgorithm = new KeyHostAlgorithm("ssh-rsa", rsaKey);

            Assert.AreEqual("ssh-rsa", keyHostAlgorithm.Name);
            Assert.AreSame(rsaKey, keyHostAlgorithm.Key);
            Assert.AreSame(rsaKey.DigitalSignature, keyHostAlgorithm.DigitalSignature);
        }

        [TestMethod]
        public void SuppliedDigitalSignature_PropertyIsSuppliedDigitalSignature()
        {
            RsaKey rsaKey = GetRsaKey();
            RsaDigitalSignature rsaDigitalSignature = new RsaDigitalSignature(rsaKey, HashAlgorithmName.SHA256);
            KeyHostAlgorithm keyHostAlgorithm = new KeyHostAlgorithm("rsa-sha2-256", rsaKey, rsaDigitalSignature);

            Assert.AreEqual("rsa-sha2-256", keyHostAlgorithm.Name);
            Assert.AreSame(rsaKey, keyHostAlgorithm.Key);
            Assert.AreSame(rsaDigitalSignature, keyHostAlgorithm.DigitalSignature);
        }

        [TestMethod]
        public void RsaPublicKeyDataDoesNotDependOnSignatureAlgorithm()
        {
            TestRsaPublicKeyData("ssh-rsa", HashAlgorithmName.SHA1);
            TestRsaPublicKeyData("rsa-sha2-256", HashAlgorithmName.SHA256);
        }

        private void TestRsaPublicKeyData(string signatureIdentifier, HashAlgorithmName hashAlgorithmName)
        {
            RsaKey key = GetRsaKey();
            KeyHostAlgorithm keyAlgorithm = new KeyHostAlgorithm(signatureIdentifier, key, new RsaDigitalSignature(key, hashAlgorithmName));

            CollectionAssert.AreEqual(GetRsaPublicKeyBytes(), keyAlgorithm.Data);
        }

        [TestMethod]
        public void SshRsa_SignAndVerify()
        {
            byte[] data = Encoding.UTF8.GetBytes("hello world");

            RsaKey key = GetRsaKey();
            KeyHostAlgorithm keyAlgorithm = new KeyHostAlgorithm("ssh-rsa", key);

            byte[] expectedEncodedSignatureBytes = new byte[]
            {
                0, 0, 0, 7, // byte count of "ssh-rsa"
                (byte)'s', (byte)'s', (byte)'h', (byte)'-', (byte)'r', (byte)'s', (byte)'a', // ssh-rsa
                0, 0, 1, 0, // byte count of signature (=256)
                
                // echo -n 'hello world' | openssl dgst -sha1 -sign Key.RSA.txt -out test.signed
                0x41, 0x50, 0x12, 0x14, 0xd3, 0x7c, 0xe0, 0x40, 0x50, 0x65, 0xfb, 0x33, 0xd9, 0x17, 0x89, 0xbf,
                0xb2, 0x4b, 0x85, 0x15, 0xbf, 0x9e, 0x57, 0x3b, 0x01, 0x15, 0x2b, 0x99, 0xfa, 0x62, 0x9b, 0x2a,
                0x05, 0xa0, 0x73, 0xc7, 0xb7, 0x5b, 0xd9, 0x01, 0xaa, 0x56, 0x73, 0x95, 0x13, 0x41, 0x33, 0x0d,
                0x7f, 0x83, 0x8a, 0x60, 0x4d, 0x19, 0xdc, 0x9b, 0xba, 0x8e, 0x61, 0xed, 0xd0, 0x8a, 0x3e, 0x38,
                0x71, 0xee, 0x34, 0xc3, 0x55, 0x0f, 0x55, 0x65, 0x89, 0xbb, 0x3e, 0x41, 0xee, 0xdf, 0xf5, 0x2f,
                0xab, 0x9e, 0x89, 0x37, 0x68, 0x1f, 0x9f, 0x38, 0x00, 0x81, 0x29, 0x93, 0xeb, 0x61, 0x37, 0xad,
                0x8d, 0x35, 0xf1, 0x3d, 0x4b, 0x9b, 0x99, 0x74, 0x7b, 0xeb, 0xf4, 0xfb, 0x76, 0xb4, 0xb6, 0xb4,
                0x09, 0x33, 0x5c, 0xfa, 0x6a, 0xad, 0x1e, 0xed, 0x1c, 0xe1, 0xb4, 0x4d, 0xf2, 0xa5, 0xc3, 0x64,
                0x9a, 0x45, 0x81, 0xee, 0x1b, 0xa6, 0x1d, 0x01, 0x3c, 0x4d, 0xb5, 0x62, 0x9e, 0xff, 0x8e, 0xff,
                0x6c, 0x18, 0xed, 0xe9, 0x8e, 0x03, 0x2c, 0xc5, 0x94, 0x81, 0xca, 0x8b, 0x18, 0x3f, 0x25, 0xcd,
                0xe5, 0x42, 0x49, 0x43, 0x23, 0x1f, 0xdc, 0x3f, 0xa2, 0x43, 0xbc, 0xbd, 0x42, 0xf5, 0x60, 0xfb,
                0x01, 0xd3, 0x67, 0x0d, 0x8d, 0x85, 0x7b, 0x51, 0x14, 0xec, 0x26, 0x53, 0x00, 0x61, 0x25, 0x16,
                0x19, 0x10, 0x3c, 0x86, 0x16, 0x59, 0x84, 0x08, 0xd1, 0xf9, 0x1e, 0x05, 0x88, 0xbd, 0x4a, 0x01,
                0x43, 0x4e, 0xec, 0x76, 0x0b, 0xd7, 0x2c, 0xe9, 0x98, 0xb1, 0x4c, 0x0a, 0x13, 0xc6, 0x95, 0xf9,
                0x8f, 0x95, 0x5c, 0x98, 0x4c, 0x8f, 0x97, 0x4a, 0xad, 0x0d, 0xfe, 0x84, 0xf0, 0x56, 0xc3, 0x29,
                0x73, 0x75, 0x55, 0x3c, 0xd9, 0x5e, 0x5b, 0x6f, 0xf9, 0x81, 0xbc, 0xbc, 0x50, 0x75, 0x7d, 0xa8
            };

            CollectionAssert.AreEqual(expectedEncodedSignatureBytes, keyAlgorithm.Sign(data));

            keyAlgorithm = new KeyHostAlgorithm("ssh-rsa", new RsaKey(new SshKeyData(GetRsaPublicKeyBytes())));
            Assert.IsTrue(keyAlgorithm.VerifySignature(data, expectedEncodedSignatureBytes));
        }

        [TestMethod]
        public void RsaSha256_SignAndVerify()
        {
            byte[] data = Encoding.UTF8.GetBytes("hello world");

            RsaKey key = GetRsaKey();
            KeyHostAlgorithm keyAlgorithm = new KeyHostAlgorithm("rsa-sha2-256", key, new RsaDigitalSignature(key, HashAlgorithmName.SHA256));

            byte[] expectedEncodedSignatureBytes = new byte[]
            {
                0, 0, 0, 12, // byte count of "rsa-sha2-256"
                (byte)'r', (byte)'s', (byte)'a', (byte)'-', (byte)'s', (byte)'h', (byte)'a', (byte)'2',
                (byte)'-', (byte)'2', (byte)'5', (byte)'6',
                0, 0, 1, 0, // byte count of signature (=256)
                
                // echo -n 'hello world' | openssl dgst -sha256 -sign Key.RSA.txt -out test.signed
                0x2e, 0xef, 0x01, 0x49, 0x5c, 0x66, 0x37, 0x56, 0xc2, 0xfb, 0x7b, 0xfa, 0x80, 0x2f, 0xdb, 0xaa,
                0x0d, 0x15, 0xd9, 0x8d, 0xa9, 0xad, 0x81, 0x4f, 0x09, 0x2e, 0x53, 0x9e, 0xce, 0x5d, 0x68, 0x07,
                0xae, 0xb9, 0xc0, 0x45, 0xfa, 0x30, 0xd0, 0xf7, 0xd6, 0xa6, 0x8d, 0x19, 0x24, 0x3a, 0xea, 0x91,
                0x3e, 0xa2, 0x4a, 0x42, 0x2e, 0x21, 0xf1, 0x48, 0x57, 0xca, 0x2b, 0x6c, 0x9f, 0x79, 0x54, 0x91,
                0x3e, 0x3a, 0x4d, 0xd1, 0x70, 0x87, 0x3d, 0xbe, 0x22, 0x97, 0xc9, 0xb0, 0x02, 0xf0, 0xa2, 0xae,
                0x7a, 0xbb, 0x8b, 0xaf, 0xc0, 0x3b, 0xab, 0x71, 0xe8, 0x29, 0x1c, 0x18, 0x88, 0xca, 0x74, 0x1b,
                0x34, 0x4f, 0xd1, 0x83, 0x39, 0x6e, 0x8f, 0x69, 0x3d, 0x7e, 0xef, 0xef, 0x57, 0x7c, 0xff, 0x21,
                0x9c, 0x10, 0x2b, 0xd1, 0x4f, 0x26, 0xbe, 0xaa, 0xd2, 0xd9, 0x03, 0x14, 0x75, 0x97, 0x11, 0xaf,
                0xf0, 0x28, 0xf2, 0xd3, 0x07, 0x79, 0x5b, 0x27, 0xdc, 0x97, 0xd8, 0xce, 0x4e, 0x78, 0x89, 0x16,
                0x91, 0x2a, 0xb2, 0x47, 0x53, 0x94, 0xe9, 0xa1, 0x15, 0x98, 0x29, 0x0c, 0xa1, 0xf5, 0xe2, 0x8e,
                0x11, 0xdc, 0x0c, 0x1c, 0x10, 0xa4, 0xf2, 0x46, 0x5c, 0x78, 0x0c, 0xc1, 0x4a, 0x65, 0x21, 0x8a,
                0x2e, 0x32, 0x6c, 0x72, 0x06, 0xf9, 0x7f, 0xa1, 0x6c, 0x2e, 0x13, 0x06, 0x41, 0xaa, 0x23, 0xdd,
                0xc8, 0x1c, 0x61, 0xb6, 0x96, 0x87, 0xc4, 0x84, 0xc8, 0x61, 0xec, 0x4e, 0xdd, 0x49, 0x9e, 0x4f,
                0x0d, 0x8c, 0xf1, 0x7f, 0xf2, 0x6c, 0x73, 0x5a, 0xa6, 0x3b, 0xbf, 0x4e, 0xba, 0x57, 0x6b, 0xb3,
                0x1e, 0x6c, 0x57, 0x76, 0x87, 0x9f, 0xb4, 0x3b, 0xcb, 0xcd, 0xe5, 0x10, 0x7a, 0x4c, 0xeb, 0xc0,
                0xc4, 0xc3, 0x75, 0x51, 0x5f, 0xb7, 0x7c, 0xbc, 0x55, 0x8d, 0x05, 0xc7, 0xed, 0xc7, 0x52, 0x4a
            };

            CollectionAssert.AreEqual(expectedEncodedSignatureBytes, keyAlgorithm.Sign(data));

            key = new RsaKey(new SshKeyData(GetRsaPublicKeyBytes()));
            keyAlgorithm = new KeyHostAlgorithm("rsa-sha2-256", key, new RsaDigitalSignature(key, HashAlgorithmName.SHA256));
            Assert.IsTrue(keyAlgorithm.VerifySignature(data, expectedEncodedSignatureBytes));
        }

        [TestMethod]
        public void RsaSha512_SignAndVerify()
        {
            byte[] data = Encoding.UTF8.GetBytes("hello world");

            RsaKey key = GetRsaKey();
            KeyHostAlgorithm keyAlgorithm = new KeyHostAlgorithm("rsa-sha2-512", key, new RsaDigitalSignature(key, HashAlgorithmName.SHA512));

            byte[] expectedEncodedSignatureBytes = new byte[]
            {
                0, 0, 0, 12, // byte count of "rsa-sha2-512"
                (byte)'r', (byte)'s', (byte)'a', (byte)'-', (byte)'s', (byte)'h', (byte)'a', (byte)'2',
                (byte)'-', (byte)'5', (byte)'1', (byte)'2',
                0, 0, 1, 0, // byte count of signature (=256)
                
                // echo -n 'hello world' | openssl dgst -sha512 -sign Key.RSA.txt -out test.signed
                0x69, 0x70, 0xb5, 0x9f, 0x32, 0x86, 0x3b, 0xae, 0xc0, 0x79, 0x6e, 0xdb, 0x35, 0xd5, 0xa6, 0x22,
                0xcd, 0x2b, 0x4b, 0xd2, 0x68, 0x1a, 0x65, 0x41, 0xa6, 0xd9, 0x20, 0x54, 0x31, 0x9a, 0xb1, 0x44,
                0x6e, 0x8f, 0x56, 0x4b, 0xfc, 0x27, 0x7f, 0x3f, 0xe7, 0x47, 0xcb, 0x78, 0x03, 0x05, 0x79, 0x8a,
                0x16, 0x7b, 0x12, 0x01, 0x3a, 0xa2, 0xd5, 0x0d, 0x2b, 0x16, 0x38, 0xef, 0x84, 0x6b, 0xd7, 0x19,
                0xeb, 0xac, 0x54, 0x01, 0x9d, 0xa6, 0x80, 0x74, 0x43, 0xa8, 0x6e, 0x5e, 0x33, 0x05, 0x06, 0x1d,
                0x6d, 0xfe, 0x32, 0x4f, 0xe3, 0xcb, 0x3e, 0x2d, 0x4e, 0xe1, 0x47, 0x03, 0x69, 0xb4, 0x59, 0x80,
                0x59, 0x05, 0x15, 0xa0, 0x11, 0x34, 0x47, 0x58, 0xd7, 0x93, 0x2d, 0x40, 0xf2, 0x2c, 0x37, 0x48,
                0x6b, 0x3c, 0xd3, 0x03, 0x09, 0x32, 0x74, 0xa0, 0x2d, 0x33, 0x11, 0x99, 0x10, 0xb4, 0x09, 0x31,
                0xec, 0xa3, 0x2c, 0x63, 0xba, 0x50, 0xd1, 0x02, 0x45, 0xae, 0xb5, 0x75, 0x7e, 0xfa, 0xfc, 0x06,
                0xb6, 0x6a, 0xb2, 0xa1, 0x73, 0x14, 0xa5, 0xaa, 0x17, 0x88, 0x03, 0x19, 0x14, 0x9b, 0xe1, 0x10,
                0xf8, 0x2f, 0x73, 0x01, 0xc7, 0x8d, 0x37, 0xef, 0x98, 0x69, 0xc2, 0xe2, 0x7a, 0x11, 0xd5, 0xb8,
                0xc9, 0x35, 0x45, 0xcb, 0x56, 0x4b, 0x92, 0x4a, 0xe0, 0x4c, 0xd6, 0x82, 0xae, 0xad, 0x5b, 0xe9,
                0x40, 0x7e, 0x2a, 0x48, 0x7d, 0x57, 0xc5, 0xfd, 0xe9, 0x98, 0xe0, 0xbb, 0x09, 0xa1, 0xf5, 0x48,
                0x45, 0xcb, 0xee, 0xb9, 0x99, 0x81, 0x44, 0x15, 0x2e, 0x50, 0x39, 0x64, 0x58, 0x4c, 0x34, 0x86,
                0xf8, 0x81, 0x9e, 0x1d, 0xb6, 0x97, 0xe0, 0xce, 0x16, 0xca, 0x20, 0x46, 0xe9, 0x49, 0x8f, 0xe6,
                0xa0, 0x23, 0x08, 0x80, 0xa6, 0x37, 0x70, 0x06, 0xcc, 0x8f, 0xf4, 0xa0, 0x74, 0x53, 0x26, 0x38
            };

            CollectionAssert.AreEqual(expectedEncodedSignatureBytes, keyAlgorithm.Sign(data));

            key = new RsaKey(new SshKeyData(GetRsaPublicKeyBytes()));
            keyAlgorithm = new KeyHostAlgorithm("rsa-sha2-512", key, new RsaDigitalSignature(key, HashAlgorithmName.SHA512));
            Assert.IsTrue(keyAlgorithm.VerifySignature(data, expectedEncodedSignatureBytes));
        }

        private static RsaKey GetRsaKey()
        {
            using (var stream = GetData("Key.RSA.txt"))
            {
                return (RsaKey) new PrivateKeyFile(stream).Key;
            }
        }

        private static byte[] GetRsaPublicKeyBytes()
        {
            return new byte[]
            {
                0, 0, 0, 7, // byte count of "ssh-rsa"
                (byte)'s', (byte)'s', (byte)'h', (byte)'-', (byte)'r', (byte)'s', (byte)'a', // ssh-rsa
                0, 0, 0, 1, // byte count of exponent
                35, // exponent
                0, 0, 1, 1, // byte count of modulus (=257)

                // openssl rsa -in Key.RSA.txt -text
                0x00, 0xb9, 0x3b, 0x57, 0x9f, 0xe0, 0x5a, 0xb5, 0x7d, 0x68, 0x26, 0xeb, 0xe1, 0xa9, 0xf2,
                0x59, 0xc3, 0x98, 0xdc, 0xfe, 0x97, 0x08, 0xc4, 0x95, 0x0f, 0x9a, 0xea, 0x05, 0x08, 0x7d,
                0xfe, 0x6d, 0x77, 0xca, 0x04, 0x9f, 0xfd, 0xe2, 0x2c, 0x4d, 0x11, 0x3c, 0xd9, 0x05, 0xab,
                0x32, 0xbd, 0x3f, 0xe8, 0xcd, 0xba, 0x00, 0x6c, 0x21, 0xb7, 0xa9, 0xc2, 0x4e, 0x63, 0x17,
                0xf6, 0x04, 0x47, 0x93, 0x00, 0x85, 0xde, 0xd6, 0x32, 0xc0, 0xa1, 0x37, 0x75, 0x18, 0xa0,
                0xb0, 0x32, 0xf6, 0x4e, 0xca, 0x39, 0xec, 0x3c, 0xdf, 0x79, 0xfe, 0x50, 0xa1, 0xc1, 0xf7,
                0x67, 0x05, 0xb3, 0x33, 0xa5, 0x96, 0x13, 0x19, 0xfa, 0x14, 0xca, 0x55, 0xe6, 0x7b, 0xf9,
                0xb3, 0x8e, 0x32, 0xee, 0xfc, 0x9d, 0x2a, 0x5e, 0x04, 0x79, 0x97, 0x29, 0x3d, 0x1c, 0x54,
                0xfe, 0xc7, 0x96, 0x04, 0xb5, 0x19, 0x7c, 0x55, 0x21, 0xe2, 0x0e, 0x42, 0xca, 0x4d, 0x9d,
                0xfb, 0x77, 0x08, 0x6c, 0xaa, 0x07, 0x2c, 0xf8, 0xf9, 0x1f, 0xbd, 0x83, 0x14, 0x2b, 0xe0,
                0xbc, 0x7a, 0xf9, 0xdf, 0x13, 0x4b, 0x60, 0x5a, 0x02, 0x99, 0x93, 0x41, 0x1a, 0xb6, 0x5f,
                0x3b, 0x9c, 0xb5, 0xb2, 0x55, 0x70, 0x78, 0x2f, 0x38, 0x52, 0x0e, 0xd1, 0x8a, 0x2c, 0x23,
                0xc0, 0x3a, 0x0a, 0xd7, 0xed, 0xf6, 0x1f, 0xa6, 0x50, 0xf0, 0x27, 0x65, 0x8a, 0xd4, 0xde,
                0xa7, 0x1b, 0x41, 0x67, 0xc5, 0x6d, 0x47, 0x84, 0x37, 0x92, 0x2b, 0xb7, 0xb6, 0x4d, 0xb0,
                0x1a, 0xda, 0xf6, 0x50, 0x82, 0xf1, 0x57, 0x31, 0x69, 0xce, 0xe0, 0xef, 0xcd, 0x64, 0xaa,
                0x78, 0x08, 0xea, 0x4e, 0x45, 0xec, 0xa5, 0x89, 0x68, 0x5d, 0xb4, 0xa0, 0x23, 0xaf, 0xff,
                0x9c, 0x0f, 0x8c, 0x83, 0x7c, 0xf8, 0xe1, 0x8e, 0x32, 0x8e, 0x61, 0xfc, 0x5b, 0xbd, 0xd4,
                0x46, 0xe1
            };
        }
    }
}
