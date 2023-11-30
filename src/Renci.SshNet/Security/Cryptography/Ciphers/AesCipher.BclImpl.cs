using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    public partial class AesCipher
    {
        private sealed class BclImpl : BlockCipher, IDisposable
        {
            private readonly Aes _aes;
            private readonly ICryptoTransform _encryptor;
            private readonly ICryptoTransform _decryptor;

            public BclImpl(
                byte[] key,
                byte[] iv,
                System.Security.Cryptography.CipherMode cipherMode,
                PaddingMode paddingMode)
                : base(key, 16, mode: null, padding: null)
            {
                var aes = Aes.Create();
                aes.Key = key;

                if (cipherMode != System.Security.Cryptography.CipherMode.ECB)
                {
                    if (iv is null)
                    {
                        throw new ArgumentNullException(nameof(iv));
                    }

                    aes.IV = iv.Take(16);
                }

                aes.Mode = cipherMode;
                aes.Padding = paddingMode;
                aes.FeedbackSize = 128; // We use CFB128
                _aes = aes;
                _encryptor = aes.CreateEncryptor();
                _decryptor = aes.CreateDecryptor();
            }

            public override byte[] Encrypt(byte[] input, int offset, int length)
            {
                if (_aes.Padding != PaddingMode.None)
                {
                    // If padding has been specified, call TransformFinalBlock to apply
                    // the padding and reset the state.
                    return _encryptor.TransformFinalBlock(input, offset, length);
                }

                // Otherwise, (the most important case) assume this instance is
                // used for one direction of an SSH connection, whereby the
                // encrypted data in all packets are considered a single data
                // stream i.e. we do not want to reset the state between calls to Encrypt.
                var output = new byte[length];
                _ = _encryptor.TransformBlock(input, offset, length, output, 0);
                return output;
            }

            public override byte[] Decrypt(byte[] input, int offset, int length)
            {
                if (_aes.Padding != PaddingMode.None)
                {
                    // If padding has been specified, call TransformFinalBlock to apply
                    // the padding and reset the state.
                    return _decryptor.TransformFinalBlock(input, offset, length);
                }

                // Otherwise, (the most important case) assume this instance is
                // used for one direction of an SSH connection, whereby the
                // encrypted data in all packets are considered a single data
                // stream i.e. we do not want to reset the state between calls to Decrypt.
                var output = new byte[length];
                _ = _decryptor.TransformBlock(input, offset, length, output, 0);
                return output;
            }

            public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                throw new NotImplementedException($"Invalid usage of {nameof(EncryptBlock)}.");
            }

            public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                throw new NotImplementedException($"Invalid usage of {nameof(DecryptBlock)}.");
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
