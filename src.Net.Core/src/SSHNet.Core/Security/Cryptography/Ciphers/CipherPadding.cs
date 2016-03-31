namespace Renci.SshNet.Security.Cryptography.Ciphers
{
    /// <summary>
    /// Base class for cipher padding implementations
    /// </summary>
    public abstract class CipherPadding
    {
        /// <summary>
        /// Pads specified input to match block size.
        /// </summary>
        /// <param name="blockSize">Size of the block.</param>
        /// <param name="input">The input.</param>
        /// <returns>Padded data array.</returns>
        public abstract byte[] Pad(int blockSize, byte[] input);

        /// <summary>
        /// Pads specified input with a given number of bytes to match the block size.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="length">The number of bytes to pad the input with.</param>
        /// <returns>
        /// The padded data array.
        /// </returns>
        public abstract byte[] Pad(byte[] input, int length);
    }
}
