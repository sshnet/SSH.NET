using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    public partial class DesCipher
    {
        private sealed class BclImpl : BlockCipher, IDisposable
        {
            private readonly DES _des;
            private readonly ICryptoTransform _encryptor;
            private readonly ICryptoTransform _decryptor;

            public BclImpl(
                byte[] key,
                byte[] iv,
                System.Security.Cryptography.CipherMode mode,
                PaddingMode padding)
                : base(key, 8, mode: null, padding: null)
            {
                var des = DES.Create();
                des.Key = Key;
                des.IV = iv.Take(8);
                des.Mode = mode;
                des.Padding = padding;
                _des = des;
                _encryptor = _des.CreateEncryptor();
                _decryptor = _des.CreateDecryptor();
            }

            public override byte[] Encrypt(byte[] input, int offset, int length)
            {
                if (_des.Padding != PaddingMode.None)
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
                if (_des.Padding != PaddingMode.None)
                {
                    // If padding has been specified, call TransformFinalBlock to apply
                    // the padding and reset the state.
                    return _decryptor.TransformFinalBlock(input, offset, length);
                }

                // Otherwise, (the most important case) assume this instance is
                // used for one direction of an SSH connection, whereby the
                // encrypted data in all packets are considered a single data
                // stream i.e. we do not want to reset the state between calls to Encrypt.
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

            public void Dispose()
            {
                _des.Dispose();
                _encryptor.Dispose();
                _decryptor.Dispose();
            }
        }
    }
}
