#nullable enable
using System;
using System.Numerics;
using System.Security.Cryptography;

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains the RSA private and public key.
    /// </summary>
    public class RsaKey : Key, IDisposable
    {
        private RsaDigitalSignature? _digitalSignature;

        /// <summary>
        /// Gets the name of the key.
        /// </summary>
        /// <returns>
        /// The name of the key.
        /// </returns>
        public override string ToString()
        {
            return "ssh-rsa";
        }

        internal RSA RSA { get; }

        /// <summary>
        /// Gets the modulus.
        /// </summary>
        /// <value>
        /// The modulus.
        /// </value>
        public BigInteger Modulus { get; }

        /// <summary>
        /// Gets the exponent.
        /// </summary>
        /// <value>
        /// The exponent.
        /// </value>
        public BigInteger Exponent { get; }

        /// <summary>
        /// Gets the D.
        /// </summary>
        /// <value>
        /// The D.
        /// </value>
        public BigInteger D { get; }

        /// <summary>
        /// Gets the P.
        /// </summary>
        /// <value>
        /// The P.
        /// </value>
        public BigInteger P { get; }

        /// <summary>
        /// Gets the Q.
        /// </summary>
        /// <value>
        /// The Q.
        /// </value>
        public BigInteger Q { get; }

        /// <summary>
        /// Gets the DP.
        /// </summary>
        /// <value>
        /// The DP.
        /// </value>
        public BigInteger DP { get; }

        /// <summary>
        /// Gets the DQ.
        /// </summary>
        /// <value>
        /// The DQ.
        /// </value>
        public BigInteger DQ { get; }

        /// <summary>
        /// Gets the inverse Q.
        /// </summary>
        /// <value>
        /// The inverse Q.
        /// </value>
        public BigInteger InverseQ { get; }

        /// <inheritdoc/>
        public override int KeyLength
        {
            get
            {
                return (int)Modulus.GetBitLength();
            }
        }

        /// <summary>
        /// Gets the digital signature implementation for this key.
        /// </summary>
        /// <value>
        /// An implementation of an RSA digital signature using the SHA-1 hash algorithm.
        /// </value>
        protected internal override DigitalSignature DigitalSignature
        {
            get
            {
                _digitalSignature ??= new RsaDigitalSignature(this);

                return _digitalSignature;
            }
        }

        /// <summary>
        /// Gets the RSA public key.
        /// </summary>
        /// <value>
        /// An array with <see cref="Exponent"/> at index 0, and <see cref="Modulus"/>
        /// at index 1.
        /// </value>
        public override BigInteger[] Public
        {
            get
            {
                return new[] { Exponent, Modulus };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaKey"/> class.
        /// </summary>
        /// <param name="publicKeyData">The encoded public key data.</param>
        public RsaKey(SshKeyData publicKeyData)
        {
            if (publicKeyData is null)
            {
                throw new ArgumentNullException(nameof(publicKeyData));
            }

            if (publicKeyData.Name != "ssh-rsa" || publicKeyData.Keys.Length != 2)
            {
                throw new ArgumentException($"Invalid RSA public key data. ({publicKeyData.Name}, {publicKeyData.Keys.Length}).", nameof(publicKeyData));
            }

            Exponent = publicKeyData.Keys[0];
            Modulus = publicKeyData.Keys[1];

            RSA = RSA.Create();
            RSA.ImportParameters(GetRSAParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaKey"/> class.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <param name="exponent">The exponent.</param>
        /// <param name="d">The d.</param>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="dp">The dp.</param>
        /// <param name="dq">The dq.</param>
        /// <param name="inverseQ">The inverse Q.</param>
        public RsaKey(BigInteger modulus, BigInteger exponent, BigInteger d, BigInteger p, BigInteger q, BigInteger dp, BigInteger dq, BigInteger inverseQ)
        {
            Modulus = modulus;
            Exponent = exponent;
            D = d;
            P = p;
            Q = q;
            DP = dp;
            DQ = dq;
            InverseQ = inverseQ;

            RSA = RSA.Create();
            RSA.ImportParameters(GetRSAParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RsaKey"/> class.
        /// </summary>
        /// <param name="modulus">The modulus.</param>
        /// <param name="exponent">The exponent.</param>
        /// <param name="d">The d.</param>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="inverseQ">The inverse Q.</param>
        public RsaKey(BigInteger modulus, BigInteger exponent, BigInteger d, BigInteger p, BigInteger q, BigInteger inverseQ)
        {
            Modulus = modulus;
            Exponent = exponent;
            D = d;
            P = p;
            Q = q;
            DP = PrimeExponent(d, p);
            DQ = PrimeExponent(d, q);
            InverseQ = inverseQ;

            RSA = RSA.Create();
            RSA.ImportParameters(GetRSAParameters());
        }

        internal RSAParameters GetRSAParameters()
        {
            // Specification of the RSAParameters fields (taken from the CryptographicException
            // thrown when not done correctly):

            // Exponent and Modulus are required. If D is present, it must have the same length
            // as Modulus. If D is present, P, Q, DP, DQ, and InverseQ are required and must
            // have half the length of Modulus, rounded up, otherwise they must be omitted.

            // See also https://github.com/dotnet/runtime/blob/9b57a265c7efd3732b035bade005561a04767128/src/libraries/Common/src/System/Security/Cryptography/RSAKeyFormatHelper.cs#L42

            if (D.IsZero)
            {
                // Public key
                return new RSAParameters()
                {
                    Modulus = Modulus.ToByteArray(isUnsigned: true, isBigEndian: true),
                    Exponent = Exponent.ToByteArray(isUnsigned: true, isBigEndian: true),
                };
            }

            var n = Modulus.ToByteArray(isUnsigned: true, isBigEndian: true);
            var halfModulusLength = (n.Length + 1) / 2;

            return new RSAParameters()
            {
                Modulus = n,
                Exponent = Exponent.ToByteArray(isUnsigned: true, isBigEndian: true),
                D = D.ExportKeyParameter(n.Length),
                P = P.ExportKeyParameter(halfModulusLength),
                Q = Q.ExportKeyParameter(halfModulusLength),
                DP = DP.ExportKeyParameter(halfModulusLength),
                DQ = DQ.ExportKeyParameter(halfModulusLength),
                InverseQ = InverseQ.ExportKeyParameter(halfModulusLength),
            };
        }

        private static BigInteger PrimeExponent(BigInteger privateExponent, BigInteger prime)
        {
            var pe = prime - BigInteger.One;
            return privateExponent % pe;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _digitalSignature?.Dispose();
                RSA.Dispose();
            }
        }
    }
}
