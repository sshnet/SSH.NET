using Renci.SshNet.Abstractions;

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
            return CryptoAbstraction.HashSHA1(hashData);
        }
    }
}
