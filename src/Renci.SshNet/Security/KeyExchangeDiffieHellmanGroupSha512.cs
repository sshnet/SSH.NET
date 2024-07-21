using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Base class for "diffie-hellman" SHA-512 group algorithm implementations.
    /// </summary>
    internal abstract class KeyExchangeDiffieHellmanGroupSha512 : KeyExchangeDiffieHellmanGroupShaBase
    {
        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <value>
        /// The size, in bits, of the computed hash code.
        /// </value>
        protected override int HashSize
        {
            get { return 512; }
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
            return SHA512.HashData(hashData);
#else
            using (var sha512 = SHA512.Create())
            {
                return sha512.ComputeHash(hashData);
            }
#endif
        }
    }
}
