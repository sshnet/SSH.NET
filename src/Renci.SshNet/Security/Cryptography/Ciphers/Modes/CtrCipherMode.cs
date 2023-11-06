#pragma warning disable CA5358 // Review cipher mode usage with cryptography experts
#pragma warning disable IDE0005 // Using directive is unnecessary

using System;
using System.Numerics;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes
{
    /// <summary>
    /// Implements CTR cipher mode.
    /// </summary>
    public class CtrCipherMode : CipherMode
    {
        private readonly uint[] _ivOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="CtrCipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        public CtrCipherMode(byte[] iv)
            : base(iv, System.Security.Cryptography.CipherMode.ECB)
        {
            _ivOutput = GetPackedIV(iv);
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
            var buffer = CTREncryptDecrypt(inputBuffer, inputOffset, inputCount);
            Buffer.BlockCopy(buffer, 0, outputBuffer, 0, buffer.Length);

            return buffer.Length;
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
        public override int DecryptBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return EncryptBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
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
            return CTREncryptDecrypt(input, offset, length);
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
            return CTREncryptDecrypt(input, offset, length);
        }

        // convert the IV into an array of uint[4]
        private static uint[] GetPackedIV(byte[] iv)
        {
            var packedIV = new uint[4];
            packedIV[0] = (uint) ((iv[0] << 24) | (iv[1] << 16) | (iv[2] << 8) | iv[3]);
            packedIV[1] = (uint) ((iv[4] << 24) | (iv[5] << 16) | (iv[6] << 8) | iv[7]);
            packedIV[2] = (uint) ((iv[8] << 24) | (iv[9] << 16) | (iv[10] << 8) | iv[11]);
            packedIV[3] = (uint) ((iv[12] << 24) | (iv[13] << 16) | (iv[14] << 8) | iv[15]);
            return packedIV;
        }

        // Perform AES-CTR encryption/decryption
        private byte[] CTREncryptDecrypt(byte[] data, int offset, int length)
        {
            var count = length / BlockSize;
            if (length % BlockSize != 0)
            {
                count++;
            }

            var counter = CTRCreateCounterArray(count);
            var aesCounter = Encryptor.TransformFinalBlock(counter, 0, counter.Length);
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
                counter[i] = (_ivOutput[0] << 24) | ((_ivOutput[0] << 8) & 0x00FF0000) | ((_ivOutput[0] >> 8) & 0x0000FF00) | (_ivOutput[0] >> 24);
                counter[i + 1] = (_ivOutput[1] << 24) | ((_ivOutput[1] << 8) & 0x00FF0000) | ((_ivOutput[1] >> 8) & 0x0000FF00) | (_ivOutput[1] >> 24);
                counter[i + 2] = (_ivOutput[2] << 24) | ((_ivOutput[2] << 8) & 0x00FF0000) | ((_ivOutput[2] >> 8) & 0x0000FF00) | (_ivOutput[2] >> 24);
                counter[i + 3] = (_ivOutput[3] << 24) | ((_ivOutput[3] << 8) & 0x00FF0000) | ((_ivOutput[3] >> 8) & 0x0000FF00) | (_ivOutput[3] >> 24);

                // increment IV (little endian)
                for (var j = 3; j >= 0 && ++_ivOutput[j] == 0; j--)
                {
                    // empty block
                }
            }

            // copy uint[] to byte[]
            var counterBytes = new byte[blocks * 16];
            Buffer.BlockCopy(counter, 0, counterBytes, 0, counterBytes.Length);
            return counterBytes;
        }

#if NETSTANDARD2_1_OR_GREATER

        // XOR 2 arrays using Vector<byte>
        private static byte[] CTRArrayXOR(byte[] counter, byte[] data, int offset, int length)
        {
            for (var loopOffset = 0; length > 0; length -= Vector<byte>.Count)
            {
                var v = new Vector<byte>(counter, loopOffset) ^ new Vector<byte>(data, offset + loopOffset);
                if (length >= Vector<byte>.Count)
                {
                    v.CopyTo(counter, loopOffset);
                    loopOffset += Vector<byte>.Count;
                }
                else
                {
                    for (var i = 0; i < length; i++)
                    {
                        counter[loopOffset++] = v[i];
                    }
                }
            }

            return counter;
        }

#else   // fallback to blockcopy and regular XOR

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
#endif
    }
}
