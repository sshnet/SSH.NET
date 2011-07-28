using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Renci.SshNet.Common;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Represents the standard parameters for the Renci.SshNet.Security.Cryptography.RSACipher algorithm.
    /// </summary>
    public class RSAPublicKey
    {
        /// <summary>
        /// Represents the Exponent parameter for the Renci.SshNet.Security.Cryptography.RSACipher algorithm.
        /// </summary>
        public BigInteger Exponent { get; private set; }

        /// <summary>
        /// Represents the Modulus parameter for the Renci.SshNet.Security.Cryptography.RSACipher algorithm.
        /// </summary>
        public BigInteger Modulus { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RSAPublicKey"/> class.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <param name="modulus">The modulus.</param>
        public RSAPublicKey(byte[] exponent, byte[] modulus)
        {
            if (exponent == null)
                throw new ArgumentNullException("exponent");
            if (modulus == null)
                throw new ArgumentNullException("modulus");

            this.Exponent = new BigInteger(exponent.Reverse().ToArray());
            this.Modulus = new BigInteger(modulus.Reverse().ToArray());
        }
    }
}
