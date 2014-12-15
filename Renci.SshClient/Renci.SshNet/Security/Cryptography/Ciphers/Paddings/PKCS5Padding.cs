using System;

namespace Renci.SshNet.Security.Cryptography.Ciphers.Paddings
{
    /// <summary>
    /// Implements PKCS5 cipher padding
    /// </summary>
    public class PKCS5Padding : CipherPadding
    {
        /// <summary>
        /// Transforms the specified input.
        /// </summary>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Padded data array.
        /// </returns>
        public override byte[] Pad(int blockSize, byte[] input)
        {
            var numOfPaddedBytes = blockSize - (input.Length % blockSize);
            return Pad(input, numOfPaddedBytes);
        }

        /// <summary>
        /// Pads specified input with a given number of bytes to match the block size.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="length">The number of bytes to pad the input with.</param>
        /// <returns>
        /// The padded data array.
        /// </returns>
        public override byte[] Pad(byte[] input, int length)
        {
            var output = new byte[input.Length + length];
            Buffer.BlockCopy(input, 0, output, 0, input.Length);
            for (var i = 0; i < length; i++)
            {
                output[input.Length + i] = (byte) length;
            }
            return output;
        }
    }
}
