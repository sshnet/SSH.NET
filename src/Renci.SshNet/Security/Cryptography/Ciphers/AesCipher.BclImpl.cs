#nullable enable
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
                    ArgumentNullException.ThrowIfNull(iv);

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
                return Transform(_encryptor, input, offset, length, output: null, 0, out _);
            }

            public override byte[] Decrypt(byte[] input, int offset, int length)
            {
                return Transform(_decryptor, input, offset, length, output: null, 0, out _);
            }

            public override int Decrypt(byte[] input, int offset, int length, byte[] output, int outputOffset)
            {
                _ = Transform(_decryptor, input, offset, length, output, outputOffset, out var bytesWritten);

                return bytesWritten;
            }

            private byte[] Transform(ICryptoTransform transform, byte[] input, int offset, int length, byte[]? output, int outputOffset, out int bytesWritten)
            {
                if (_aes.Padding != PaddingMode.None)
                {
                    // If padding has been specified, call TransformFinalBlock to apply
                    // the padding and reset the state.

                    var finalBlock = transform.TransformFinalBlock(input, offset, length);

                    if (output is not null)
                    {
                        finalBlock.AsSpan().CopyTo(output.AsSpan(outputOffset));
                    }

                    bytesWritten = finalBlock.Length;

                    return finalBlock;
                }

                // Otherwise, (the most important case) assume this instance is
                // used for one direction of an SSH connection, whereby the
                // encrypted data in all packets are considered a single data
                // stream i.e. we do not want to reset the state between calls to Decrypt.

                var paddingLength = 0;
                if (length % BlockSize > 0)
                {
                    if (_aes.Mode is System.Security.Cryptography.CipherMode.CFB or System.Security.Cryptography.CipherMode.OFB)
                    {
                        // Manually pad the input for cfb and ofb cipher mode as BCL doesn't support partial block.
                        // See https://github.com/dotnet/runtime/blob/e7d837da5b1aacd9325a8b8f2214cfaf4d3f0ff6/src/libraries/System.Security.Cryptography/src/System/Security/Cryptography/SymmetricPadding.cs#L20-L21
                        paddingLength = BlockSize - (length % BlockSize);

                        var tmp = new byte[length + paddingLength];

                        input.AsSpan(offset, length).CopyTo(tmp);

                        input = tmp;
                        offset = 0;
                        length = tmp.Length;
                    }
                }

                if (output is null)
                {
                    output = new byte[length];

                    bytesWritten = transform.TransformBlock(input, offset, length, output, outputOffset);

                    bytesWritten -= paddingLength;

                    // Manually unpad the output.
                    Array.Resize(ref output, bytesWritten);
                }
                else
                {
                    bytesWritten = transform.TransformBlock(input, offset, length, output, outputOffset);

                    bytesWritten -= paddingLength;
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
                _aes.Dispose();
                _encryptor.Dispose();
                _decryptor.Dispose();
            }
        }
    }
}
