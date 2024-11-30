using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    public partial class TripleDesCipher
    {
        private sealed class BclImpl : BlockCipher, IDisposable
        {
            private readonly TripleDES _des;
            private readonly ICryptoTransform _encryptor;
            private readonly ICryptoTransform _decryptor;

            public BclImpl(
                byte[] key,
                byte[] iv,
                System.Security.Cryptography.CipherMode mode,
                PaddingMode padding)
                : base(key, 8, mode: null, padding: null)
            {
                var des = TripleDES.Create();
                des.FeedbackSize = 64; // We use CFB8
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
                    return _encryptor.TransformFinalBlock(input, offset, length);
                }

                var paddingLength = 0;
                if (length % BlockSize > 0)
                {
                    if (_des.Mode is System.Security.Cryptography.CipherMode.CFB or System.Security.Cryptography.CipherMode.OFB)
                    {
                        // Manually pad the input for cfb and ofb cipher mode as BCL doesn't support partial block.
                        // See https://github.com/dotnet/runtime/blob/e7d837da5b1aacd9325a8b8f2214cfaf4d3f0ff6/src/libraries/System.Security.Cryptography/src/System/Security/Cryptography/SymmetricPadding.cs#L20-L21
                        paddingLength = BlockSize - (length % BlockSize);
                        input = input.Take(offset, length);
                        length += paddingLength;
                        Array.Resize(ref input, length);
                        offset = 0;
                    }
                }

                // Otherwise, (the most important case) assume this instance is
                // used for one direction of an SSH connection, whereby the
                // encrypted data in all packets are considered a single data
                // stream i.e. we do not want to reset the state between calls to Encrypt.
                var output = new byte[length];
                _ = _encryptor.TransformBlock(input, offset, length, output, 0);

                if (paddingLength > 0)
                {
                    // Manually unpad the output.
                    Array.Resize(ref output, output.Length - paddingLength);
                }

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

                var paddingLength = 0;
                if (length % BlockSize > 0)
                {
                    if (_des.Mode is System.Security.Cryptography.CipherMode.CFB or System.Security.Cryptography.CipherMode.OFB)
                    {
                        // Manually pad the input for cfb and ofb cipher mode as BCL doesn't support partial block.
                        // See https://github.com/dotnet/runtime/blob/e7d837da5b1aacd9325a8b8f2214cfaf4d3f0ff6/src/libraries/System.Security.Cryptography/src/System/Security/Cryptography/SymmetricPadding.cs#L20-L21
                        paddingLength = BlockSize - (length % BlockSize);
                        input = input.Take(offset, length);
                        length += paddingLength;
                        Array.Resize(ref input, length);
                        offset = 0;
                    }
                }

                // Otherwise, (the most important case) assume this instance is
                // used for one direction of an SSH connection, whereby the
                // encrypted data in all packets are considered a single data
                // stream i.e. we do not want to reset the state between calls to Encrypt.
                var output = new byte[length];
                _ = _decryptor.TransformBlock(input, offset, length, output, 0);

                if (paddingLength > 0)
                {
                    // Manually unpad the output.
                    Array.Resize(ref output, output.Length - paddingLength);
                }

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
