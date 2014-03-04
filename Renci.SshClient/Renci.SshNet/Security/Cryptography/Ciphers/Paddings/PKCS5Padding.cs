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

            var output = new byte[input.Length + numOfPaddedBytes];
            Buffer.BlockCopy(input, 0, output, 0, input.Length);
            for (int i = 0; i < numOfPaddedBytes; i++)
            {
                output[input.Length + i] = (byte)numOfPaddedBytes;
            }

            return output;
        }
    }
}
