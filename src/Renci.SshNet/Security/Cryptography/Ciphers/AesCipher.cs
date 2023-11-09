#pragma warning disable IDE0005 // Using directive is unnecessary.
#pragma warning disable CA5358  // Review cipher mode usage with cryptography experts

using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using Renci.SshNet.Common;
using Renci.SshNet.Messages.Authentication;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using Renci.SshNet.Security.Cryptography.Ciphers.Paddings;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Custom AES Cipher Mode, follows System.Security.Cryptography.CipherMode.
    /// </summary>
    public enum AesCipherMode
    {
        /// <summary>CBC Mode.</summary>
        CBC = 1,

        /// <summary>ECB Mode.</summary>
        ECB = 2,

        /// <summary>OFB Mode.</summary>
        OFB = 3,

        /// <summary>CFB Mode.</summary>
        CFB = 4,

        /// <summary>CTS Mode.</summary>
        CTS = 5,

        /// <summary>CTR Mode.</summary>
        CTR = 6
    }

    /// <summary>
    /// AES cipher implementation.
    /// </summary>
    public sealed class AesCipher : BlockCipher, IDisposable
    {
        private readonly AesCipherMode _aesMode;
        private readonly CipherMode _blockMode;

        private readonly Aes _aes;
        private ICryptoTransform _encryptor;
        private ICryptoTransform _decryptor;

        private AesCipher _ecbHelper;
        private uint[] _packedIV;

        /// <summary>
        /// Initializes a new instance of the <see cref="AesCipher"/> class in ECB mode.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="pkcs7Padding">Enable PKCS7 padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public AesCipher(byte[] key, bool pkcs7Padding = false)
            : this(key, iv: null, AesCipherMode.ECB, pkcs7Padding)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AesCipher"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="iv">The IV.</param>
        /// <param name="pkcs7Padding">Enable PKCS7 padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public AesCipher(byte[] key, byte[] iv, AesCipherMode mode, bool pkcs7Padding = false)
            : base(key, 16, mode: null, padding: pkcs7Padding ? new PKCS7Padding() : null)
        {
            var keySize = key.Length * 8;

            if (keySize is not (256 or 192 or 128))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "KeySize '{0}' is not valid for this algorithm.", keySize));
            }

            _aesMode = mode;
            iv = iv?.Take(16) ?? new byte[16];
            var bclMode = GetBCLMode(mode, iv, out _blockMode);

            if (_blockMode is not null)
            {
                _ecbHelper = new AesCipher(key, pkcs7Padding: false);    // ECB with no padding
                _blockMode.Init(_ecbHelper);
            }

            _aes = Aes.Create();
            _aes.BlockSize = 128;
            _aes.FeedbackSize = 128;
            _aes.Mode = bclMode;
            _aes.Padding = pkcs7Padding ? PaddingMode.PKCS7 : PaddingMode.None;
            _aes.Key = key;
            _aes.IV = iv;

#pragma warning disable S3329  // Cipher Block Chaining IVs should be unpredictable
#pragma warning disable CA5401 // Cipher Block Chaining IVs should be unpredictable
            _encryptor = _aes.CreateEncryptor();
            _decryptor = _aes.CreateDecryptor();
        }

        // get the BCL-equivalent AES mode and optional legacy CipherMode
        private System.Security.Cryptography.CipherMode GetBCLMode(AesCipherMode mode, byte[] iv, out CipherMode blockMode)
        {
            blockMode = null;

#pragma warning disable IDE0010 // allow missing cases, fallback to ECB
            switch (mode)
            {
                case AesCipherMode.CBC:
                    return System.Security.Cryptography.CipherMode.CBC;

                case AesCipherMode.CTS:
                    return System.Security.Cryptography.CipherMode.CTS;

                // OFB is supported on 4.62 but has a faulty implementation, so always fallback to ECB
                case AesCipherMode.OFB:
                    blockMode = new OfbCipherMode(iv);
                    break;

                // CFB not supported on NetStandard 2.1
                case AesCipherMode.CFB:
#if NET6_0_OR_GREATER
                    return System.Security.Cryptography.CipherMode.CFB;
#else
                    blockMode = new CfbCipherMode(iv);
                    break;
#endif

                // CTR not supported by the BCL
                case AesCipherMode.CTR:
                    blockMode = new CtrCipherMode(iv);
                    _packedIV = GetPackedIV(iv);
                    break;
            }
#pragma warning restore IDE0010 // Add missing cases

            return System.Security.Cryptography.CipherMode.ECB;
        }

        /// <summary>
        /// Encrypts the specified region of the input byte array and copies the encrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to encrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write encrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes encrypted.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is too short.</exception>
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            // fallback: legacy block-by-block encryption
            if (_blockMode is not null)
            {
                return _blockMode.EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            CheckArgs(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);

            // fast crypto using the BCL CryptoServiceProvider/CNG
            return _encryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <summary>
        /// Decrypts the specified region of the input byte array and copies the decrypted data to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input data to decrypt.</param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data.</param>
        /// <param name="outputBuffer">The output to which to write decrypted data.</param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data.</param>
        /// <returns>
        /// The number of bytes decrypted.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is <see langword="null"/>.</exception>
        /// <exception cref="IndexOutOfRangeException"><paramref name="inputBuffer"/> or <paramref name="outputBuffer"/> is too short.</exception>
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            // fallback: legacy block-by-block decryption
            if (_blockMode is not null)
            {
                return _blockMode.DecryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
            }

            CheckArgs(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);

            // fast crypto using the BCL CryptoServiceProvider/CNG
            return _decryptor.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <summary>
        /// Encrypts the specified data.
        /// </summary>
        /// <param name="input">The data.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The encrypted data.
        /// </returns>
        public override byte[] Encrypt(byte[] input, int offset, int length)
        {
            if (_aesMode == AesCipherMode.CTR)
            {
                return CTREncryptDecrypt(input, offset, length);
            }

            // fast crypto using the BCL CryptoServiceProvider/CNG
            if (_blockMode is null && _encryptor.CanTransformMultipleBlocks)
            {
                if (_aes.Padding == PaddingMode.None)
                {
                    var output = new byte[length];
                    _ = _encryptor.TransformBlock(input, offset, length, output, 0);
                    return output;
                }

                return _encryptor.TransformFinalBlock(input, offset, length);
            }

            // fallback: legacy block-by-block encryption
            return base.Encrypt(input, offset, length);
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin decrypting.</param>
        /// <param name="length">The number of bytes to decrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        public override byte[] Decrypt(byte[] input, int offset, int length)
        {
            if (_aesMode == AesCipherMode.CTR)
            {
                return CTREncryptDecrypt(input, offset, length);
            }

            // fast crypto using the BCL CryptoServiceProvider/CNG
            if (_blockMode is null && _decryptor.CanTransformMultipleBlocks)
            {
                if (_aes.Padding == PaddingMode.None)
                {
                    var output = new byte[length];
                    _ = _decryptor.TransformBlock(input, offset, length, output, 0);
                    return output;
                }

                return _decryptor.TransformFinalBlock(input, offset, length);
            }

            // fallback: legacy block-by-block decryption
            return base.Decrypt(input, offset, length);
        }

        private void CheckArgs(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer.Length - inputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid input buffer");
            }

            if (outputBuffer.Length - outputOffset < BlockSize)
            {
                throw new ArgumentException("Invalid output buffer");
            }

            if (inputCount != BlockSize)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", BlockSize));
            }
        }

        // CTR implementation, optimized for large arrays
        // This provides huge performance gain by avoiding a block-by-block encrpyt/decrypt loop
        // AES-CTR is used by AWS SFTP Transfer Family
        #region CTR

        // Perform AES-CTR encryption/decryption
        private byte[] CTREncryptDecrypt(byte[] data, int offset, int length)
        {
            var count = length / BlockSize;
            if (length % BlockSize != 0)
            {
                count++;
            }

            var counter = CTRCreateCounterArray(count);
            var ouput = new byte[counter.Length];
            _ = _encryptor.TransformBlock(counter, 0, counter.Length, ouput, 0);
            var output = ArrayXOR(ouput, data, offset, length);

            // adjust output for non-blocksized lengths
            if (output.Length > length)
            {
                Array.Resize(ref output, length);
            }

            return output;
        }

#if NETSTANDARD2_1_OR_GREATER || NET6_0_OR_GREATER

        // creates the Counter array filled with incrementing copies of IV
        private byte[] CTRCreateCounterArray(int blocks)
        {
            var bytes = new byte[blocks * 16];
            var counter = MemoryMarshal.Cast<byte, uint>(bytes.AsSpan());

            // fill array with IV, increment by 1 for each copy
            for (var i = 0; i < counter.Length; i += 4)
            {
                // write IV to buffer (big endian)
                counter[i] = _packedIV[0];
                counter[i + 1] = _packedIV[1];
                counter[i + 2] = _packedIV[2];
                counter[i + 3] = _packedIV[3];

                // increment IV (little endian - swap, increment, swap back)
                uint j = 3;
                do
                {
                    _packedIV[j] = SwapEndianness(SwapEndianness(_packedIV[j]) + 1);
                }
                while (_packedIV[j] == 0 && --j >= 0);
            }

            return bytes;
        }

        // XOR 2 arrays using Vector<byte>
        private static byte[] ArrayXOR(byte[] counter, byte[] data, int offset, int length)
        {
            for (var loopOffset = 0; length > 0; length -= Vector<byte>.Count)
            {
                if (length >= Vector<byte>.Count)
                {
                    var v = new Vector<byte>(counter, loopOffset) ^ new Vector<byte>(data, offset + loopOffset);
                    v.CopyTo(counter, loopOffset);
                    loopOffset += Vector<byte>.Count;
                }
                else
                {
                    for (var i = 0; i < length; i++)
                    {
                        counter[loopOffset] ^= data[offset + loopOffset];
                        loopOffset++;
                    }
                }
            }

            return counter;
        }

#else
        // creates the Counter array filled with incrementing copies of IV
        private byte[] CTRCreateCounterArray(int blocks)
        {
            // fill array with IV, increment by 1 for each copy
            var counter = new uint[blocks * 4];
            for (var i = 0; i < counter.Length; i += 4)
            {
                // write IV to buffer (big endian)
                counter[i] = _packedIV[0];
                counter[i + 1] = _packedIV[1];
                counter[i + 2] = _packedIV[2];
                counter[i + 3] = _packedIV[3];

                // increment IV (little endian - swap, increment, swap back)
                uint j = 3;
                do
                {
                    _packedIV[j] = SwapEndianness(SwapEndianness(_packedIV[j]) + 1);
                }
                while (_packedIV[j] == 0 && --j >= 0);
            }

            // copy uint[] to byte[]
            var counterBytes = new byte[blocks * 16];
            Buffer.BlockCopy(counter, 0, counterBytes, 0, counterBytes.Length);
            return counterBytes;
        }

        // XOR 2 arrays using Uint[] and blockcopy
        private static byte[] ArrayXOR(byte[] counter, byte[] data, int offset, int length)
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
            var counterwords = new uint[words];
            Buffer.BlockCopy(counter, 0, counterwords, 0, length);

            // XOR encrypted Counter with input data
            for (var i = 0; i < words; i++)
            {
                counterwords[i] = counterwords[i] ^ datawords[i];
            }

            // copy uint[] to byte[]
            var output = counter;
            Buffer.BlockCopy(counterwords, 0, output, 0, length);

            return output;
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

        #endregion

        /// <summary>
        /// Dispose the instance.
        /// </summary>
        /// <param name="disposing">Set to True to dispose of resouces.</param>
        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _encryptor?.Dispose();
                _encryptor = null;

                _decryptor?.Dispose();
                _decryptor = null;

                _ecbHelper?.Dispose();
                _ecbHelper = null;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
