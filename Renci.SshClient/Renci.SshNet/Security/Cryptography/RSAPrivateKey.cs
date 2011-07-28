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
    public class RSAPrivateKey : RSAPublicKey
    {
        /// <summary>
        /// Represents the D parameter for the Renci.SshNet.Security.Cryptography.RSACipher algorithm.
        /// </summary>
        public BigInteger D { get; private set; }

        /// <summary>
        /// Represents the DP parameter for the Renci.SshNet.Security.Cryptography.RSACipher algorithm.
        /// </summary>
        public BigInteger DP { get; private set; }

        /// <summary>
        /// Represents the DQ parameter for the Renci.SshNet.Security.Cryptography.RSACipher algorithm.
        /// </summary>
        public BigInteger DQ { get; private set; }

        /// <summary>
        /// Represents the InverseQ parameter for the Renci.SshNet.Security.Cryptography.RSACipher algorithm.
        /// </summary>
        public BigInteger InverseQ { get; private set; }

        /// <summary>
        /// Represents the P parameter for the Renci.SshNet.Security.Cryptography.RSACipher algorithm.
        /// </summary>
        public BigInteger P { get; private set; }

        /// <summary>
        /// Represents the Q parameter for the Renci.SshNet.Security.Cryptography.RSACipher algorithm.
        /// </summary>
        public BigInteger Q { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RSAPrivateKey"/> class.
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <param name="modulus">The modulus.</param>
        /// <param name="d">The d.</param>
        /// <param name="dp">The dp.</param>
        /// <param name="q">The q.</param>
        /// <param name="dq">The dq.</param>
        /// <param name="p">The p.</param>
        /// <param name="inverseQ">The inverse Q.</param>
        public RSAPrivateKey(byte[] exponent, byte[] modulus, byte[] d, byte[] dp, byte[] q, byte[] dq, byte[] p, byte[] inverseQ)
            : base(exponent, modulus)
        {
            if (d == null)
                throw new ArgumentNullException("d");
            if (dp == null)
                throw new ArgumentNullException("dp");
            if (q == null)
                throw new ArgumentNullException("q");
            if (dq == null)
                throw new ArgumentNullException("dq");
            if (p == null)
                throw new ArgumentNullException("p");
            if (inverseQ == null)
                throw new ArgumentNullException("inverseQ");

            this.D = new BigInteger(d.Reverse().ToArray());
            this.DP = new BigInteger(dp.Reverse().ToArray());
            this.Q = new BigInteger(q.Reverse().ToArray());
            this.DQ = new BigInteger(dq.Reverse().ToArray());
            this.P = new BigInteger(p.Reverse().ToArray());
            this.InverseQ = new BigInteger(inverseQ.Reverse().ToArray());
        }
    }
}
