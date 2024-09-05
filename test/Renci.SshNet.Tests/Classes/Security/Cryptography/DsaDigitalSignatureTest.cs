using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Renci.SshNet.Abstractions;
#if !NET6_0_OR_GREATER
using Renci.SshNet.Common;
#endif
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
{
    [TestClass]
    public class DsaDigitalSignatureTest : TestBase
    {
        [TestMethod]
        public void Verify()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello, World!");

            DsaKey dsaKey = GetDsaKey("Key.DSA.txt");

            Assert.AreEqual(1024, dsaKey.P.GetBitLength());
            Assert.AreEqual(160, dsaKey.Q.GetBitLength());

            var digitalSignature = new DsaDigitalSignature(dsaKey);

            byte[] signedBytes = digitalSignature.Sign(data);

            // We can't compare signatures for value equality because they have a source of randomness
            Assert.AreEqual(40, signedBytes.Length);
            Assert.IsTrue(digitalSignature.Verify(data, signedBytes));

            byte[] signatureToVerify = new byte[]
            {
                // Generated with a previous DsaDigitalSignature implementation in order to confirm consistent
                // behaviour. We can't seem to validate against openssl because openssl outputs a DER signature,
                // where as we want IEEE P1363 (fixed size) format.
                0x07, 0x4c, 0x5e, 0x15, 0x53, 0x36, 0x21, 0xbe, 0x5a, 0x82, 0x35, 0xd5, 0xb6, 0xe6, 0x7d, 0x2f,
                0x01, 0x2a, 0x78, 0x9b, 0x16, 0x4a, 0xe5, 0x8d, 0x85, 0xa6, 0x34, 0x56, 0x9d, 0x38, 0xd6, 0x1a,
                0xa4, 0xa1, 0x5b, 0x98, 0x7d, 0xd5, 0x35, 0x40
            };

            Assert.IsTrue(digitalSignature.Verify(data, signatureToVerify));

            Assert.IsFalse(digitalSignature.Verify(data, CryptoAbstraction.GenerateRandom(40)));
        }

        private static DsaKey GetDsaKey(string fileName, string passPhrase = null)
        {
            using (var stream = GetData(fileName))
            {
                return (DsaKey)new PrivateKeyFile(stream, passPhrase).Key;
            }
        }
    }
}
