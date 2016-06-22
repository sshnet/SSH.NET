using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;
using Renci.SshNet.Tests.Common;

namespace Renci.SshNet.Tests.Classes.Security.Cryptography
{
    [TestClass]
    public class BlockCipherTest  : TestBase
    {
        [TestMethod]
        public void EncryptShouldTakeIntoAccountPaddingForLengthOfOutputBufferPassedToEncryptBlock()
        {
            var input = new byte[] { 0x2c, 0x1a, 0x05, 0x00, 0x68 };
            var output = new byte[] { 0x0a, 0x00, 0x03, 0x02, 0x06, 0x08, 0x07, 0x05 };
            var key = new byte[] { 0x17, 0x78, 0x56, 0xe1, 0x3e, 0xbd, 0x3e, 0x50, 0x1d, 0x79, 0x3f, 0x0f, 0x55, 0x37, 0x45, 0x54 };
            var blockCipher = new BlockCipherStub(key, 8, null, new PKCS5Padding())
                {
                    EncryptBlockDelegate = (inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset) =>
                        {
                            Assert.AreEqual(8, outputBuffer.Length);
                            Buffer.BlockCopy(output, 0, outputBuffer, 0, output.Length);
                            return inputBuffer.Length;
                        }
                };

            var actual = blockCipher.Encrypt(input);

            Assert.IsTrue(output.SequenceEqual(actual));
        }

        [TestMethod]
        public void DecryptShouldTakeIntoAccountPaddingForLengthOfOutputBufferPassedToDecryptBlock()
        {
            var input = new byte[] { 0x2c, 0x1a, 0x05, 0x00, 0x68 };
            var output = new byte[] { 0x0a, 0x00, 0x03, 0x02, 0x06, 0x08, 0x07, 0x05 };
            var key = new byte[] { 0x17, 0x78, 0x56, 0xe1, 0x3e, 0xbd, 0x3e, 0x50, 0x1d, 0x79, 0x3f, 0x0f, 0x55, 0x37, 0x45, 0x54 };
            var blockCipher = new BlockCipherStub(key, 8, null, new PKCS5Padding())
                {
                    DecryptBlockDelegate = (inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset) =>
                        {
                            Assert.AreEqual(8, outputBuffer.Length);
                            Buffer.BlockCopy(output, 0, outputBuffer, 0, output.Length);
                            return inputBuffer.Length;
                        }
                };

            var actual = blockCipher.Decrypt(input);

            Assert.IsTrue(output.SequenceEqual(actual));
        }


        private class BlockCipherStub : BlockCipher
        {
            public Func<byte[], int, int, byte[], int, int> EncryptBlockDelegate;
            public Func<byte[], int, int, byte[], int, int> DecryptBlockDelegate;

            public BlockCipherStub(byte[] key, byte blockSize, CipherMode mode, CipherPadding padding) : base(key, blockSize, mode, padding)
            {
            }

            public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                return EncryptBlockDelegate(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                return DecryptBlockDelegate(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }
        }
    }
}
