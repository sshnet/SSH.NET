using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Represents "diffie-hellman-group1-sha1" algorithm implementation.
    /// </summary>
    internal abstract class KeyExchangeDiffieHellmanGroupSha1 : KeyExchangeDiffieHellmanGroupShaBase
    {
        /// <summary>
        /// Gets the size, in bits, of the computed hash code.
        /// </summary>
        /// <value>
        /// The size, in bits, of the computed hash code.
        /// </value>
        protected override int HashSize
        {
            get { return 160; }
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
            return SHA1.HashData(hashData);
#else
            using (var sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(hashData);
            }
#endif
        }
    }
}
