namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Base class for cipher implementation.
    /// </summary>
    public abstract class Cipher
    {
        /// <summary>
        /// Gets the minimum data size.
        /// </summary>
        /// <value>
        /// The minimum data size.
        /// </value>
        public abstract byte MinimumSize { get; }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>Encrypted data.</returns>
        public byte[] Encrypt(byte[] input)
        {
            return Encrypt(input, 0, input.Length);
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin encrypting.</param>
        /// <param name="length">The number of bytes to encrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The encrypted data.
        /// </returns>
        public abstract byte[] Encrypt(byte[] input, int offset, int length);

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        public abstract byte[] Decrypt(byte[] input);

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="offset">The zero-based offset in <paramref name="input"/> at which to begin decrypting.</param>
        /// <param name="length">The number of bytes to decrypt from <paramref name="input"/>.</param>
        /// <returns>
        /// The decrypted data.
        /// </returns>
        public abstract byte[] Decrypt(byte[] input, int offset, int length);
    }
}
