using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Implements DES cipher algorithm.
    /// </summary>
    [TestClass]
    public class DesCipherTest : TestBase
    {
        [TestMethod]
        public void Cbc_Encrypt()
        {
            // echo -n 'www.javaCODEgeeks.com!!!' | openssl enc -e -des-cbc -K 0123456789ABCDEF -iv 0706050403020100 -provider legacy | hd
            var expectedCypher = new byte[]
                {
                    0x15, 0x43, 0x3e, 0x97, 0x65, 0x66, 0xea, 0x81, 0x22, 0xab, 0xe3,
                    0x11, 0x0f, 0x7d, 0xcb, 0x78, 0x56, 0x91, 0x22, 0x3d, 0xd6, 0xca,
                    0xe3, 0xbd, 0xb9, 0x8a, 0x38, 0x0c, 0xea, 0x5a, 0x90, 0x06
                };

            var input = Encoding.ASCII.GetBytes("www.javaCODEgeeks.com!!!");
            var key = new byte[] {0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef};
            var iv = new byte[] {0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00};

            var des = new DesCipher(key, new CbcCipherMode(iv), new PKCS7Padding());
            var actualCypher = des.Encrypt(input);

            Assert.IsTrue((expectedCypher.IsEqualTo(actualCypher)));
        }

        [TestMethod]
        public void Cbc_Decrypt()
        {
            var expectedPlain = Encoding.ASCII.GetBytes("www.javaCODEgeeks.com!!!");

            var key = new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef };
            var iv = new byte[] { 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01, 0x00 };
            var cypher = new byte[]
                {
                    0x15, 0x43, 0x3e, 0x97, 0x65, 0x66, 0xea, 0x81, 0x22, 0xab, 0xe3,
                    0x11, 0x0f, 0x7d, 0xcb, 0x78, 0x56, 0x91, 0x22, 0x3d, 0xd6, 0xca,
                    0xe3, 0xbd, 0xb9, 0x8a, 0x38, 0x0c, 0xea, 0x5a, 0x90, 0x06
                };

            var des = new DesCipher(key, new CbcCipherMode(iv), new PKCS7Padding());
            var plain = des.Decrypt(cypher);

            Assert.IsTrue(expectedPlain.IsEqualTo(plain));
        }
   }
}
