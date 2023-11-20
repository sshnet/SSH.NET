using System;
#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER
using System.Numerics;
using System.Runtime.InteropServices;
#endif
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    public partial class AesCipher
    {
        private sealed class CtrImpl : BlockCipher, IDisposable
        {
            private readonly Aes _aes;
            private readonly uint[] _packedIV;
            private readonly ICryptoTransform _encryptor;

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

                _packedIV = GetPackedIV(iv);
            }

            public override byte[] Encrypt(byte[] input, int offset, int length)
            {
                return CTREncryptDecrypt(input, offset, length);
            }

            public override byte[] Decrypt(byte[] input, int offset, int length)
            {
                return CTREncryptDecrypt(input, offset, length);
            }

            public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                throw new NotImplementedException($"Invalid usage of {nameof(DecryptBlock)}.");
            }

            public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
            {
                throw new NotImplementedException($"Invalid usage of {nameof(EncryptBlock)}.");
            }

            private byte[] CTREncryptDecrypt(byte[] data, int offset, int length)
            {
                var count = length / BlockSize;
                if (length % BlockSize != 0)
                {
                    count++;
                }

                var buffer = new byte[count * BlockSize];
                CTRCreateCounterArray(buffer);
                _ = _encryptor.TransformBlock(buffer, 0, buffer.Length, buffer, 0);
                ArrayXOR(buffer, data, offset, length);

                // adjust output for non-blocksized lengths
                if (buffer.Length > length)
                {
                    Array.Resize(ref buffer, length);
                }

                return buffer;
            }

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER

            // creates the Counter array filled with incrementing copies of IV
            private void CTRCreateCounterArray(byte[] buffer)
            {
                var counter = MemoryMarshal.Cast<byte, uint>(buffer.AsSpan());

                // fill array with IV, increment by 1 for each copy
                var len = counter.Length;
                for (var i = 0; i < len; i += 4)
                {
                    counter[i] = _packedIV[0];
                    counter[i + 1] = _packedIV[1];
                    counter[i + 2] = _packedIV[2];
                    counter[i + 3] = _packedIV[3];

                    // increment IV (little endian)
                    if (_packedIV[3] < 0xFF000000u)
                    {
                        _packedIV[3] += 0x01000000u;
                    }
                    else
                    {
                        uint j = 3;
                        do
                        {
                            _packedIV[j] = SwapEndianness(SwapEndianness(_packedIV[j]) + 1);
                        }
                        while (_packedIV[j] == 0 && --j >= 0);
                    }
                }
            }

            // XOR 2 arrays using Vector<byte>
            private static void ArrayXOR(byte[] buffer, byte[] data, int offset, int length)
            {
                var i = 0;

                var oneVectorFromEnd = length - Vector<byte>.Count;
                for (; i <= oneVectorFromEnd; i += Vector<byte>.Count)
                {
                    var v = new Vector<byte>(buffer, i) ^ new Vector<byte>(data, offset + i);
                    v.CopyTo(buffer, i);
                }

                for (; i < length; i++)
                {
                    buffer[i] ^= data[offset + i];
                }
            }

#else
            // creates the Counter array filled with incrementing copies of IV
            private void CTRCreateCounterArray(byte[] buffer)
            {
                // fill array with IV, increment by 1 for each copy
                var words = buffer.Length / 4;
                var counter = new uint[words];
                for (var i = 0; i < words; i += 4)
                {
                    // write IV to buffer (big endian)
                    counter[i] = _packedIV[0];
                    counter[i + 1] = _packedIV[1];
                    counter[i + 2] = _packedIV[2];
                    counter[i + 3] = _packedIV[3];

                    // increment IV (little endian)
                    if (_packedIV[3] < 0xFF000000u)
                    {
                        _packedIV[3] += 0x01000000u;
                    }
                    else
                    {
                        uint j = 3;
                        do
                        {
                            _packedIV[j] = SwapEndianness(SwapEndianness(_packedIV[j]) + 1);
                        }
                        while (_packedIV[j] == 0 && --j >= 0);
                    }
                }

                // copy uint[] to byte[]
                Buffer.BlockCopy(counter, 0, buffer, 0, buffer.Length);
            }

            // XOR 2 arrays using Uint[] and blockcopy
            private static void ArrayXOR(byte[] buffer, byte[] data, int offset, int length)
            {
                var words = length / 4;
                if (length % 4 != 0)
                {
                    words++;
                }

                // convert original data to words
                var datawords = new uint[words];
                Buffer.BlockCopy(data, offset, datawords, 0, length);

                // convert encrypted IV counter to words
                var bufferwords = new uint[words];
                Buffer.BlockCopy(buffer, 0, bufferwords, 0, length);

                // XOR encrypted Counter with input data
                for (var i = 0; i < words; i++)
                {
                    bufferwords[i] = bufferwords[i] ^ datawords[i];
                }

                // copy uint[] to byte[]
                Buffer.BlockCopy(bufferwords, 0, buffer, 0, length);
            }

#endif

            // pack the IV into an array of uint[4]
            private static uint[] GetPackedIV(byte[] iv)
            {
                var packedIV = new uint[4];
                packedIV[0] = BitConverter.ToUInt32(iv, 0);
                packedIV[1] = BitConverter.ToUInt32(iv, 4);
                packedIV[2] = BitConverter.ToUInt32(iv, 8);
                packedIV[3] = BitConverter.ToUInt32(iv, 12);

                return packedIV;
            }

            private static uint SwapEndianness(uint x)
            {
                x = (x >> 16) | (x << 16);
                return ((x & 0xFF00FF00) >> 8) | ((x & 0x00FF00FF) << 8);
            }

            private void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _aes.Dispose();
                    _encryptor.Dispose();
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
