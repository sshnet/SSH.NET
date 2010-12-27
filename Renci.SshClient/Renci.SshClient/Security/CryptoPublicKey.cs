using System.Collections.Generic;

namespace Renci.SshClient.Security
{
    /// <summary>
    /// Represents base class for public keys
    /// </summary>
    public abstract class CryptoPublicKey : CryptoKey
    {
        /// <summary>
        /// Verifies the signature.
        /// </summary>
        /// <param name="hash">The hash.</param>
        /// <param name="signature">The signature.</param>
        /// <returns>true if signature verified; otherwise false.</returns>
        public abstract bool VerifySignature(IEnumerable<byte> hash, IEnumerable<byte> signature);
    }
}
