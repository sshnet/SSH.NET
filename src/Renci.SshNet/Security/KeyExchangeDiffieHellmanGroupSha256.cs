using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Base class for "diffie-hellman" SHA-256 group algorithm implementations.
    /// </summary>
    internal abstract class KeyExchangeDiffieHellmanGroupSha256 : KeyExchangeDiffieHellmanGroupShaBase
    {
        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <value>
        /// The size, in bits, of the computed hash code.
        /// </value>
        protected override int HashSize
        {
            get { return 256; }
        }

        /// <summary>
        /// Hashes the specified data bytes.
        /// </summary>
        /// <param name="hashData">The hash data.</param>
        /// <returns>
        /// The hash of the data.
        /// </returns>
        protected override byte[] Hash(byte[] hashData)
        {
#if NET6_0_OR_GREATER
            return SHA256.HashData(hashData);
#else
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(hashData);
            }
#endif
        }
    }
}
