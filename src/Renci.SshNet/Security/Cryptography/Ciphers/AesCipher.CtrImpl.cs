#nullable enable
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    public partial class AesCipher
    {
        private sealed class CtrImpl : BlockCipher, IDisposable
        {
            private const int KeystreamBufferLength = 4096;

            private readonly Aes _aes;

            private readonly ICryptoTransform _encryptor;

            private ulong _ivUpper; // The upper 64 bits of the IV
            private ulong _ivLower; // The lower 64 bits of the IV

            private byte[]? _keystreamBuffer;

            public CtrImpl(
                byte[] key,
                byte[] iv)
                : base(key, 16, mode: null, padding: null)
            {
                var aes = Aes.Create();
                aes.Key = key;
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                aes.Padding = PaddingMode.None;
                _aes = aes;
                _encryptor = aes.CreateEncryptor();

                _ivLower = BinaryPrimitives.ReadUInt64BigEndian(iv.AsSpan(8));
                _ivUpper = BinaryPrimitives.ReadUInt64BigEndian(iv);
            }

            public override byte[] Encrypt(byte[] input, int offset, int length)
            {
                return Decrypt(input, offset, length);
            }

            public override int Encrypt(byte[] input, int offset, int length, byte[] output, int outputOffset)
            {
                return Decrypt(input, offset, length, output, outputOffset);
            }

            public override byte[] Decrypt(byte[] input, int offset, int length)
            {
                ArgumentNullException.ThrowIfNull(input);

                var buffer = CTREncryptDecrypt(input, offset, length, output: null, 0);

                // adjust output for non-blocksized lengths
                if (buffer.Length > length)
                {
                    Array.Resize(ref buffer, length);
                }

                return buffer;
            }

            public override int Decrypt(byte[] input, int offset, int length, byte[] output, int outputOffset)
            {
                ArgumentNullException.ThrowIfNull(input);
                ArgumentNullException.ThrowIfNull(output);

                _ = CTREncryptDecrypt(input, offset, length, output, outputOffset);

                return length;
            }

            public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                throw new NotImplementedException($"Invalid usage of {nameof(DecryptBlock)}.");
            }

            public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                throw new NotImplementedException($"Invalid usage of {nameof(EncryptBlock)}.");
            }

            private byte[] CTREncryptDecrypt(byte[] data, int offset, int length, byte[]? output, int outputOffset)
            {
                var blockSizedLength = length;
                if (blockSizedLength % BlockSize != 0)
                {
                    blockSizedLength += BlockSize - (blockSizedLength % BlockSize);
                }

                Debug.Assert(blockSizedLength % BlockSize == 0);

                byte[] keystream;
                int keystreamOffset;
                int chunkSize;

                if (data == output && offset == outputOffset)
                {
                    keystream = _keystreamBuffer ??= new byte[KeystreamBufferLength];
                    keystreamOffset = 0;
                    chunkSize = KeystreamBufferLength;
                }
                else
                {
                    if (output is null)
                    {
                        output = new byte[blockSizedLength];
                        outputOffset = 0;
                    }
                    else if (data.AsSpan(offset, length).Overlaps(output.AsSpan(outputOffset, blockSizedLength)))
                    {
                        throw new ArgumentException("Input and output buffers must not overlap (except when identical).");
                    }

                    keystream = output;
                    keystreamOffset = outputOffset;
                    chunkSize = length;
                }

                var bytesProcessed = 0;
                while (bytesProcessed < length)
                {
                    var bytesThisChunk = Math.Min(chunkSize, length - bytesProcessed);
                    var blockSizedChunk = (bytesThisChunk + BlockSize - 1) & ~(BlockSize - 1);

                    CTRCreateCounterArray(keystream.AsSpan(keystreamOffset, blockSizedChunk));

                    var bytesWritten = _encryptor.TransformBlock(
                        inputBuffer: keystream,
                        inputOffset: keystreamOffset,
                        inputCount: blockSizedChunk,
                        outputBuffer: keystream,
                        outputOffset: keystreamOffset);

                    Debug.Assert(bytesWritten == blockSizedChunk);

                    ArrayXOR(
                        dst: output,
                        dstOffset: outputOffset + bytesProcessed,
                        a: data,
                        aOffset: offset + bytesProcessed,
                        b: keystream,
                        bOffset: keystreamOffset,
                        length: bytesThisChunk);

                    bytesProcessed += bytesThisChunk;
                }

                return output;
            }

            // creates the Counter array filled with incrementing copies of IV
            private void CTRCreateCounterArray(Span<byte> buffer)
            {
                Debug.Assert(buffer.Length % 16 == 0);

                for (var i = 0; i < buffer.Length; i += 16)
                {
                    BinaryPrimitives.WriteUInt64BigEndian(buffer.Slice(i + 8), _ivLower);
                    BinaryPrimitives.WriteUInt64BigEndian(buffer.Slice(i), _ivUpper);

                    _ivLower += 1;
                    _ivUpper += (_ivLower == 0) ? 1UL : 0UL;
                }
            }

            // dst[i] = a[i] ^ b[i]
            private static void ArrayXOR(byte[] dst, int dstOffset, byte[] a, int aOffset, byte[] b, int bOffset, int length)
            {
                var i = 0;

                var oneVectorFromEnd = length - Vector<byte>.Count;
                for (; i <= oneVectorFromEnd; i += Vector<byte>.Count)
                {
                    var v = new Vector<byte>(a, aOffset + i) ^ new Vector<byte>(b, bOffset + i);
                    v.CopyTo(dst, dstOffset + i);
                }

                for (; i < length; i++)
                {
                    dst[dstOffset + i] = (byte)(a[aOffset + i] ^ b[bOffset + i]);
                }
            }

            public void Dispose()
            {
                _aes.Dispose();
                _encryptor.Dispose();
            }
        }
    }
}
