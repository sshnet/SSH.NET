using System;
using System.Globalization;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
    /// <summary>
    /// Implements CTR cipher mode.
    /// </summary>
    public class CtrCipherMode : CipherMode
    {
        private readonly byte[] _ivOutput;

#if FEATURE_AES_CSP
        // IV as uint[] for usage with CryptoServiceProvider
        private uint[] _ivCSP;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="CtrCipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        public CtrCipherMode(byte[] iv)
            : base(iv)
        {
            _ivOutput = new byte[iv.Length];

#if FEATURE_AES_CSP
            // CTR uses ECB plus an incrementing IV
            cspMode = System.Security.Cryptography.CipherMode.ECB;      
            cspDecryptAsEncrypt = true;

            // convert the IV into an array of uint[4] for speed
            _ivCSP = new uint[4];
            _ivCSP[0] = (uint)((iv[0] << 24) | (iv[1] << 16) | (iv[2] << 8) | (iv[3]));
            _ivCSP[1] = (uint)((iv[4] << 24) | (iv[5] << 16) | (iv[6] << 8) | (iv[7]));
            _ivCSP[2] = (uint)((iv[8] << 24) | (iv[9] << 16) | (iv[10] << 8) | (iv[11]));
            _ivCSP[3] = (uint)((iv[12] << 24) | (iv[13] << 16) | (iv[14] << 8) | (iv[15]));
#endif
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
        public override int EncryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer.Length - inputOffset < _blockSize)
            {
                throw new ArgumentException("Invalid input buffer");
            }

            if (outputBuffer.Length - outputOffset < _blockSize)
            {
                throw new ArgumentException("Invalid output buffer");
            }

            if (inputCount != _blockSize)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "inputCount must be {0}.", _blockSize));
            }

            _ = Cipher.EncryptBlock(IV, 0, IV.Length, _ivOutput, 0);

            for (var i = 0; i < _blockSize; i++)
            {
                outputBuffer[outputOffset + i] = (byte)(_ivOutput[i] ^ inputBuffer[inputOffset + i]);
            }

            var j = IV.Length;
            while (--j >= 0 && ++IV[j] == 0)
            {
                // Intentionally empty block
            }

            return _blockSize;
        }

#if FEATURE_AES_CSP
        /// <summary>
        /// Encrypts the specified region of the input byte array using AesCryptoServiceProvider
        /// </summary>
        /// <param name="data">The input data to encrypt.</param>
        /// <param name="offset">The offset into the input byte array from which to begin using data.</param>
        /// <param name="output">The output to which to write encrypted data.</param>
        /// <returns>The number of bytes encrypted</returns>
        public override int EncryptWithCSP(byte[] data, int offset, byte[] output)
        {
            byte[] counter = CreateIVCounter(data, offset, output.Length);
            int bytes = aesEncryptor.TransformBlock(counter, 0, output.Length, output, 0);
            DataXorCounter(data, offset, output);
            return bytes;
        }

        // creates the Counter array filled with incrementing copies of IV
        private byte[] CreateIVCounter(byte[] inputBuffer, int offset, int length)
        {
            // fill an array with IV, increment by 1 for each copy
            uint[] counter = new uint[length / 4];
            for (int i = 0; i < counter.Length; i += 4)
            {
                // write IV to buffer (big endian)
                counter[i] = (_ivCSP[0] << 24) | ((_ivCSP[0] << 8) & 0x00FF0000) | ((_ivCSP[0] >> 8) & 0x0000FF00) | (_ivCSP[0] >> 24);
                counter[i + 1] = (_ivCSP[1] << 24) | ((_ivCSP[1] << 8) & 0x00FF0000) | ((_ivCSP[1] >> 8) & 0x0000FF00) | (_ivCSP[1] >> 24);
                counter[i + 2] = (_ivCSP[2] << 24) | ((_ivCSP[2] << 8) & 0x00FF0000) | ((_ivCSP[2] >> 8) & 0x0000FF00) | (_ivCSP[2] >> 24);
                counter[i + 3] = (_ivCSP[3] << 24) | ((_ivCSP[3] << 8) & 0x00FF0000) | ((_ivCSP[3] >> 8) & 0x0000FF00) | (_ivCSP[3] >> 24);

                // increment IV (little endian)
                for (int j = 3; j >= 0 && ++_ivCSP[j] == 0; j--) ;
            }

            // copy uint[] to byte[]
            byte[] counterBytes = new byte[length];
            System.Buffer.BlockCopy(counter, 0, counterBytes, 0, length);
            return counterBytes;
        }

        // XORs the input data with the encrypted Counter array to produce the final output
        private void DataXorCounter(byte[] data, int offset, byte[] output)
        {
            int length = output.Length;

            // original data 
            uint[] inwords = new uint[length / 4];
            System.Buffer.BlockCopy(data, offset, inwords, 0, length);

            // encrypted IV counter data
            uint[] outwords = new uint[length / 4];
            System.Buffer.BlockCopy(output, 0, outwords, 0, length);

            // XOR encrypted Counter with input data (use uint arrays for speed)
            for (int i = 0; i < outwords.Length; i++)
                outwords[i] = outwords[i] ^ inwords[i];

            // copy final data to output byte[]
            System.Buffer.BlockCopy(outwords, 0, output, 0, length);
        }
#endif

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
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }
    }
}
