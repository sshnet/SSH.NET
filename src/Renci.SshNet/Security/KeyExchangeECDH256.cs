using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;

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

#if NET8_0_OR_GREATER
        /// <summary>
        /// Gets the curve.
        /// </summary>
        protected override System.Security.Cryptography.ECCurve Curve
        {
            get
            {
                return System.Security.Cryptography.ECCurve.NamedCurves.nistP256;
            }
        }
#endif

        /// <summary>
        /// Gets Curve Parameter.
        /// </summary>
        protected override X9ECParameters CurveParameter
        {
            get
            {
                return SecNamedCurves.GetByOid(SecObjectIdentifiers.SecP256r1);
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
        /// The hash of the data.
        /// </returns>
        protected override byte[] Hash(byte[] hashData)
        {
            return CryptoAbstraction.HashSHA256(hashData);
        }
    }
}
