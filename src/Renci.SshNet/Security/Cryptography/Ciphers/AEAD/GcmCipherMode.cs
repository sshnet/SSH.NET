using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Modes   
{
    /// <summary>
    /// Implements GCM cipher mode
    /// </summary>
    public class GcmCipherMode : CipherMode
    {
        #if NETCOREAPP3_0 || NETSTANDARD2_1
        private readonly byte[] _ivOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="GcmCipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        public GcmCipherMode(byte[] iv)
            : base(iv)
        {
            _ivOutput = new byte[iv.Length];
        }
       
        /// <summary>
        /// Get the Key Length.
        /// </summary>
        public static int KEY_BYTES = 16;

        /// <summary>
        /// Get the IV length.
        /// </summary>
        public static int NONCE_BYTES = 12;

        /// <summary>
        /// Encrypt given bytes
        /// </summary>
        /// <param name="toEncrypt"></param>
        /// <param name="key"></param>
        /// <param name="associatedData"></param>
        /// <returns></returns>
        public byte[] GcmEncrypt(byte[] toEncrypt, byte[] key, byte[] associatedData = null)
        {
            byte[] tag = new byte[KEY_BYTES];
            byte[] cipherText = new byte[toEncrypt.Length];

            using (var cipher = new AesGcm(key))
            {
                cipher.Encrypt(_ivOutput, toEncrypt, cipherText, tag, associatedData);

                return Concat(tag, Concat(_ivOutput, cipherText));
            }
        }

        /// <summary>
        /// Decrypt Given Bytes
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="key"></param>
        /// <param name="associatedData"></param>
        /// <returns></returns>
        public byte[] GcmDecrypt(byte[] cipherText, byte[] key, byte[] associatedData = null)
        {
            byte[] tag = SubArray(cipherText, 0, KEY_BYTES);
            byte[] toDecrypt = SubArray(cipherText, KEY_BYTES + NONCE_BYTES, cipherText.Length - tag.Length - _ivOutput.Length);
            byte[] decryptedData = new byte[toDecrypt.Length];

            using (var cipher = new AesGcm(key))
            {
                cipher.Decrypt(_ivOutput, toDecrypt, tag, decryptedData, associatedData);

                return decryptedData;
            }
        }
    #else
        private readonly byte[] _ivOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="GcmCipherMode"/> class.
        /// </summary>
        /// <param name="iv">The iv.</param>
        public GcmCipherMode(byte[] iv)
            : base(iv)
        {
            _ivOutput = new byte[iv.Length];
        }
    #endif
        /// <summary>
        /// Concatening given array of bytes
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static byte[] Concat(byte[] a, byte[] b)
        {
            byte[] output = new byte[a.Length + b.Length];

            for (int i = 0; i < a.Length; i++)
            {
                output[i] = a[i];
            }

            for (int j = 0; j < b.Length; j ++)
            {
                output[a.Length + j] = b[j];
            }

            return output;
        }

        /// <summary>
        /// Return subarray of bytes
        /// </summary>
        /// <param name="data"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] SubArray(byte[] data, int start, int length)
        {
            byte[] result = new byte[length];

            Array.Copy(data, start, result, 0, length);

            return result;
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
            return 0;
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
            return 0;
        }
    }
}