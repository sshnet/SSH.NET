namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Base class for cipher padding implementations
    /// </summary>
    public abstract class CipherPadding
    {
        /// <summary>
        /// Pads the specified input to match the block size.
        /// </summary>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="input">The input.</param>
        /// <returns>
        /// Padded data array.
        /// </returns>
        public byte[] Pad(int blockSize, byte[] input)
        {
            return Pad(blockSize, input, 0, input.Length);
        }

        /// <summary>
        /// Pads the specified input to match the block size.
        /// </summary>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which the data to pad starts.</param>
        /// <param name="length">The number of bytes in <paramref name="input"/> to take into account.</param>
        /// <returns>
        /// The padded data array.
        /// </returns>
        public abstract byte[] Pad(int blockSize, byte[] input, int offset, int length);

        /// <summary>
        /// Pads the specified input with a given number of bytes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="paddinglength">The number of bytes to pad the input with.</param>
        /// <returns>
        /// The padded data array.
        /// </returns>
        public byte[] Pad(byte[] input, int paddinglength)
        {
            return Pad(input, 0, input.Length, paddinglength);
        }

        /// <summary>
        /// Pads the specified input with a given number of bytes.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which the data to pad starts.</param>
        /// <param name="length">The number of bytes in <paramref name="input"/> to take into account.</param>
        /// <param name="paddinglength">The number of bytes to pad the input with.</param>
        /// <returns>
        /// The padded data array.
        /// </returns>
        public abstract byte[] Pad(byte[] input, int offset, int length, int paddinglength);
    }
}
