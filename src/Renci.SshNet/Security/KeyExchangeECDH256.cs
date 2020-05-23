using Renci.SshNet.Abstractions;
using Renci.SshNet.Security.Org.BouncyCastle.Asn1.Sec;
using Renci.SshNet.Security.Org.BouncyCastle.Asn1.X9;

namespace Renci.SshNet.Security
{
    internal class KeyExchangeECDH256 : KeyExchangeECDH
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "ecdh-sha2-nistp256"; }
        }

        /// <summary>
        /// Gets Curve Parameter.
        /// </summary>
        protected override X9ECParameters CurveParameter
        {
            get
            {
                return SecNamedCurves.GetByName("P-256");
            }
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
        /// Hashed bytes
        /// </returns>
        protected override byte[] Hash(byte[] hashData)
        {
            using (var sha256 = CryptoAbstraction.CreateSHA256())
            {
                return sha256.ComputeHash(hashData, 0, hashData.Length);
            }
        }
    }
}