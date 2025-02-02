#if !NET
using System;
using System.Security.Cryptography;

using Org.BouncyCastle.Crypto.Paddings;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    public partial class TripleDesCipher
    {
        private sealed class BlockImpl : BlockCipher, IDisposable
        {
            private readonly TripleDES _tripleDES;
            private readonly ICryptoTransform _encryptor;
            private readonly ICryptoTransform _decryptor;

            public BlockImpl(byte[] key, CipherMode mode, IBlockCipherPadding padding)
                : base(key, 8, mode, padding)
            {
                var tripleDES = TripleDES.Create();
                tripleDES.Key = key;
                tripleDES.Mode = System.Security.Cryptography.CipherMode.ECB;
                tripleDES.Padding = PaddingMode.None;
                _tripleDES = tripleDES;
                _encryptor = tripleDES.CreateEncryptor();
                _decryptor = tripleDES.CreateDecryptor();
            }

            public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                return _encryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                return _decryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            public void Dispose()
            {
                _tripleDES.Dispose();
                _encryptor.Dispose();
                _decryptor.Dispose();
            }
        }
    }
}
#endif
