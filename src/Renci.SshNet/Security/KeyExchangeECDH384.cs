using System.Security.Cryptography;

using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;

namespace Renci.SshNet.Security
{
    internal sealed class KeyExchangeECDH384 : KeyExchangeECDH
    {
        /// <summary>
        /// Gets algorithm name.
        /// </summary>
        public override string Name
        {
            get { return "ecdh-sha2-nistp384"; }
        }

        /// <summary>
        /// Gets Curve Parameter.
        /// </summary>
        protected override X9ECParameters CurveParameter
        {
            get
            {
                return SecNamedCurves.GetByName("secp384r1");
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
            get { return 384; }
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
            return SHA384.HashData(hashData);
#else
            using (var sha384 = SHA384.Create())
            {
                return sha384.ComputeHash(hashData);
            }
#endif
        }
    }
}
