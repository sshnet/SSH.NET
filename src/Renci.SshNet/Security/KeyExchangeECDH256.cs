using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Security
{
    internal sealed class KeyExchangeECDH256 : KeyExchangeECDH
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "ecdh-sha2-nistp256"; }
        }

        /// <summary>
        /// Gets curve name.
        /// </summary>
        protected override string CurveName
        {
            get { return "nistp256"; }
        }

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
            return CryptoAbstraction.HashSHA256(hashData);
        }
    }
}
