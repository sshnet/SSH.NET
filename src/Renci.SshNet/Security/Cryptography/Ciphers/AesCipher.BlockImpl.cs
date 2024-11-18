using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    public partial class AesCipher
    {
        private sealed class BlockImpl : BlockCipher, IDisposable
        {
            private readonly Aes _aes;
            private readonly ICryptoTransform _encryptor;
            private readonly ICryptoTransform _decryptor;

            public BlockImpl(byte[] key, CipherMode mode, CipherPadding padding)
                : base(key, 16, mode, padding)
            {
                var aes = Aes.Create();
                aes.Key = key;
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                aes.Padding = PaddingMode.None;
                _aes = aes;
                _encryptor = aes.CreateEncryptor();
                _decryptor = aes.CreateDecryptor();
            }

            public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                return _encryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                return _decryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _aes.Dispose();
                    _encryptor.Dispose();
                    _decryptor.Dispose();
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
