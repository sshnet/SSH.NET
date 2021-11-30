using System.Security.Cryptography;

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
        /// AEAD Mode or not
        /// </summary>
        /// <value>
        /// AEAD Mode is set to false by default.
        /// </value>
        public virtual bool isAEAD
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Server mac length based on the chosen hash algorithm
        /// </summary>
        /// <param name="_serverMac">The mac algorithm to use.</param>
        /// <returns>The server mac length.</returns>
        public virtual int serverMacLength(HashAlgorithm _serverMac)
        {
            return (_serverMac != null ? _serverMac.HashSize/8 : 0);
        }

        /// <summary>
        /// Find the right offset for decrypt based on chosen cipher suite
        /// </summary>
        /// <param name="blockSz">The default block size</param>
        /// <param name="inboundPacketSequenceLength">The inbound packet sequence length.</param>
        /// <returns>The default offset value used for the decrypt function, which is inboundPacketSequenceLength + blockSz</returns>
        public virtual int decryptOffset(int inboundPacketSequenceLength, int blockSz)
        {
            return inboundPacketSequenceLength + blockSz;
        }

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
