using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    public partial class AesCipher
    {
        private sealed class CtrImpl : BlockCipher, IDisposable
        {
            private readonly Aes _aes;

            private readonly ICryptoTransform _encryptor;

            private ulong _ivUpper; // The upper 64 bits of the IV
            private ulong _ivLower; // The lower 64 bits of the IV

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

            // creates the Counter array filled with incrementing copies of IV
            private void CTRCreateCounterArray(byte[] buffer)
            {
                for (var i = 0; i < buffer.Length; i += 16)
                {
                    BinaryPrimitives.WriteUInt64BigEndian(buffer.AsSpan(i + 8), _ivLower);
                    BinaryPrimitives.WriteUInt64BigEndian(buffer.AsSpan(i), _ivUpper);

                    _ivLower += 1;
                    _ivUpper += (_ivLower == 0) ? 1UL : 0UL;
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
