using System;
using Renci.SshNet.Security.Cryptography.Ciphers.Modes;
using CSP = System.Security.Cryptography;

namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// AES cipher implementation using the accelerated AesCryptoServiceProvider
    /// This CSP makes use of AES-NI instructions if available (faster, less CPU usage).
    /// </summary>
    public class AesCipherCSP
    {
        internal bool IsCSPEnabled { get; private set; }

        private readonly CipherPadding _padding;
        private readonly int _blockSize;
        private readonly bool _isCTRMode;

        private uint[] _ctrIV;
        private CSP.ICryptoTransform _aesDecryptor;
        private CSP.ICryptoTransform _aesEncryptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AesCipherCSP"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="blockSize">The block size.</param>
        /// <param name="mode">The mode.</param>
        /// <param name="padding">The padding.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Keysize is not valid for this algorithm.</exception>
        public AesCipherCSP(byte[] key, int blockSize, CipherMode mode, CipherPadding padding)
        {
            _blockSize = blockSize;
            _padding = padding;
            _isCTRMode = mode is CtrCipherMode;

            if (mode is not CtrCipherMode and not CbcCipherMode)
            {
                // OFB and CFB not supported, let IsCSPEnabled = false
                return;
            }

#if FEATURE_AES_CSP
            // initialize AesCryptoServiceProvider
            IsCSPEnabled = InitCryptoServiceProvider(key, mode.IV);
#endif
        }

        /// <summary>
        /// Encrypts the specified data using CSP acceleration if enabled.
        /// </summary>
        /// <param name="input">The data.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The encrypted data.
        /// </returns>
        public byte[] Encrypt(byte[] input, int offset, int length)
        {
            if (length % _blockSize > 0)
            {
                if (_padding is null)
                {
                    throw new ArgumentException("input");
                }

                input = _padding.Pad(_blockSize, input, offset, length);
                offset = 0;
                length = input.Length;
            }

            if (_isCTRMode)
            {
                return CTREncryptDecrypt(input, offset, length);
            }

            var output = new byte[length];
            _ = _aesEncryptor.TransformBlock(input, offset, length, output, 0);
            return output;
        }

        /// <summary>
        /// Encrypts the specified data using CSP acceleration if enabled.
        /// </summary>
        /// <param name="input">The data.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The encrypted data.
        /// </returns>
        public byte[] Decrypt(byte[] input, int offset, int length)
        {
            if (length % _blockSize > 0)
            {
                if (_padding is null)
                {
                    throw new ArgumentException("input");
                }

                input = _padding.Pad(_blockSize, input, offset, length);
                offset = 0;
                length = input.Length;
            }

            if (_isCTRMode)
            {
                return CTREncryptDecrypt(input, offset, length);
            }

            var output = new byte[length];
            _ = _aesDecryptor.TransformBlock(input, offset, length, output, 0);
            return output;
        }

        // initialize AesCryptoServiceProvider
        private bool InitCryptoServiceProvider(byte[] key, byte[] iv)
        {
            try
            {
                // prepare IV array for CTR mode
                _ctrIV = GetPackedIV(iv);

                // create ICryptoTransform instances
                using var aesProvider = CSP.Aes.Create();
                aesProvider.BlockSize = _blockSize * 8;
                aesProvider.KeySize = key.Length * 8;
#pragma warning disable CA5358 // allow ECB
                aesProvider.Mode = _isCTRMode ? CSP.CipherMode.ECB : CSP.CipherMode.CBC;    // CTR uses ECB
#pragma warning restore CA5358
                aesProvider.Padding = CSP.PaddingMode.None;
                aesProvider.Key = key;
                aesProvider.IV = iv;

#pragma warning disable CA5401 // allow specific IV in this context
                _aesEncryptor = aesProvider.CreateEncryptor(key, iv);
#pragma warning restore CA5358
                _aesDecryptor = aesProvider.CreateDecryptor(key, iv);

                return true;
            }
            catch
            {
                // fallback for unsupported key/iv/blocksize combinations
            }

            return false;
        }

        // convert the IV into an array of uint[4]
        private static uint[] GetPackedIV(byte[] iv)
        {
            var packedIV = new uint[4];
            packedIV[0] = (uint)((iv[0] << 24) | (iv[1] << 16) | (iv[2] << 8) | iv[3]);
            packedIV[1] = (uint)((iv[4] << 24) | (iv[5] << 16) | (iv[6] << 8) | iv[7]);
            packedIV[2] = (uint)((iv[8] << 24) | (iv[9] << 16) | (iv[10] << 8) | iv[11]);
            packedIV[3] = (uint)((iv[12] << 24) | (iv[13] << 16) | (iv[14] << 8) | iv[15]);
            return packedIV;
        }

        // Perform AES-CTR encryption/decryption
        private byte[] CTREncryptDecrypt(byte[] data, int offset, int length)
        {
            var count = length / _blockSize;
            if (length % _blockSize != 0)
            {
                count++;
            }

            var counter = CTRCreateCounterArray(count);
            var aesCounter = _aesEncryptor.TransformFinalBlock(counter, 0, counter.Length);
            var output = CTRArrayXOR(aesCounter, data, offset, length);

            return output;
        }

        // creates the Counter array filled with incrementing copies of IV
        private byte[] CTRCreateCounterArray(int blocks)
        {
            // fill an array with IV, increment by 1 for each copy
            var counter = new uint[blocks * 4];
            for (var i = 0; i < counter.Length; i += 4)
            {
                // write IV to buffer (big endian)
                counter[i] = (_ctrIV[0] << 24) | ((_ctrIV[0] << 8) & 0x00FF0000) | ((_ctrIV[0] >> 8) & 0x0000FF00) | (_ctrIV[0] >> 24);
                counter[i + 1] = (_ctrIV[1] << 24) | ((_ctrIV[1] << 8) & 0x00FF0000) | ((_ctrIV[1] >> 8) & 0x0000FF00) | (_ctrIV[1] >> 24);
                counter[i + 2] = (_ctrIV[2] << 24) | ((_ctrIV[2] << 8) & 0x00FF0000) | ((_ctrIV[2] >> 8) & 0x0000FF00) | (_ctrIV[2] >> 24);
                counter[i + 3] = (_ctrIV[3] << 24) | ((_ctrIV[3] << 8) & 0x00FF0000) | ((_ctrIV[3] >> 8) & 0x0000FF00) | (_ctrIV[3] >> 24);

                // increment IV (little endian)
                for (var j = 3; j >= 0 && ++_ctrIV[j] == 0; j--)
                {
                    // empty block
                }
            }

            // copy uint[] to byte[]
            var counterBytes = new byte[blocks * 16];
            Buffer.BlockCopy(counter, 0, counterBytes, 0, counterBytes.Length);
            return counterBytes;
        }

        // XORs the input data with the encrypted Counter array to produce the final output
        // uses uint arrays for speed
        private static byte[] CTRArrayXOR(byte[] counter, byte[] data, int offset, int length)
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

            // adjust output for non-aligned lengths
            if (output.Length > length)
            {
                Array.Resize(ref output, length);
            }

            return output;
        }
    }
}
