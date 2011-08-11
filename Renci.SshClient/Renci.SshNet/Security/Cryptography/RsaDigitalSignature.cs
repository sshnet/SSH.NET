using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace Renci.SshNet.Security.Cryptography
{
    /// <summary>
    /// Implements RSA digital signature algorithm.
    /// </summary>
    public class RsaDigitalSignature : CipherDigitalSignature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RsaDigitalSignature"/> class.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <param name="modulus">The modulus.</param>
        /// <param name="d">The D value.</param>
        /// <param name="dp">The DP value.</param>
        /// <param name="dq">The DQ value.</param>
        /// <param name="inverseQ">The InverseQ value.</param>
        /// <param name="p">The P value.</param>
        /// <param name="q">The Q value.</param>
        public RsaDigitalSignature(byte[] exponent, byte[] modulus, byte[] d, byte[] dp, byte[] dq, byte[] inverseQ, byte[] p, byte[] q)
            : base(new SHA1Hash(), new RsaCipher(new BigInteger(exponent.Reverse().ToArray()), new BigInteger(modulus.Reverse().ToArray()), new BigInteger(d.Reverse().ToArray()), new BigInteger(dp.Reverse().ToArray()), new BigInteger(dq.Reverse().ToArray()), new BigInteger(inverseQ.Reverse().ToArray()), new BigInteger(p.Reverse().ToArray()), new BigInteger(q.Reverse().ToArray())))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaDigitalSignature"/> class.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <param name="modulus">The modulus.</param>
        public RsaDigitalSignature(byte[] exponent, byte[] modulus)
            : base(new SHA1Hash(), new RsaCipher(new BigInteger(exponent.Reverse().ToArray()), new BigInteger(modulus.Reverse().ToArray())))
        {
        }
    }
}
