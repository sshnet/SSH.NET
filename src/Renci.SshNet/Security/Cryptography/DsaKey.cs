#nullable enable
using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains DSA private and public key.
    /// </summary>
    public class DsaKey : Key, IDisposable
    {
        private DsaDigitalSignature? _digitalSignature;

        internal DSA DSA { get; }

        /// <summary>
        /// Gets the P.
        /// </summary>
        public BigInteger P { get; }

        /// <summary>
        /// Gets the Q.
        /// </summary>
        public BigInteger Q { get; }

        /// <summary>
        /// Gets the G.
        /// </summary>
        public BigInteger G { get; }

        /// <summary>
        /// Gets public key Y.
        /// </summary>
        public BigInteger Y { get; }

        /// <summary>
        /// Gets private key X.
        /// </summary>
        public BigInteger X { get; }

        /// <inheritdoc/>
        public override int KeyLength
        {
            get
            {
                return P.BitLength;
            }
        }

        /// <summary>
        /// Gets the digital signature.
        /// </summary>
        protected internal override DigitalSignature DigitalSignature
        {
            get
            {
                _digitalSignature ??= new DsaDigitalSignature(this);
                return _digitalSignature;
            }
        }

        /// <summary>
        /// Gets the DSA public key.
        /// </summary>
        /// <value>
        /// An array whose values are:
        /// <list>
        /// <item><term>0</term><description><see cref="P"/></description></item>
        /// <item><term>1</term><description><see cref="Q"/></description></item>
        /// <item><term>2</term><description><see cref="G"/></description></item>
        /// <item><term>3</term><description><see cref="Y"/></description></item>
        /// </list>
        /// </value>
        public override BigInteger[] Public
        {
            get
            {
                return new[] { P, Q, G, Y };
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey"/> class.
        /// </summary>
        /// <param name="publicKeyData">The encoded public key data.</param>
        public DsaKey(SshKeyData publicKeyData)
        {
            if (publicKeyData is null)
            {
                throw new ArgumentNullException(nameof(publicKeyData));
            }

            if (publicKeyData.Name != "ssh-dss" || publicKeyData.Keys.Length != 4)
            {
                throw new ArgumentException($"Invalid DSA public key data. ({publicKeyData.Name}, {publicKeyData.Keys.Length}).", nameof(publicKeyData));
            }

            P = publicKeyData.Keys[0];
            Q = publicKeyData.Keys[1];
            G = publicKeyData.Keys[2];
            Y = publicKeyData.Keys[3];

            DSA = LoadDSA();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey"/> class.
        /// </summary>
        /// <param name="privateKeyData">DER encoded private key data.</param>
        public DsaKey(byte[] privateKeyData)
        {
            if (privateKeyData is null)
            {
                throw new ArgumentNullException(nameof(privateKeyData));
            }

            var der = new DerData(privateKeyData);
            _ = der.ReadBigInteger(); // skip version

            P = der.ReadBigInteger();
            Q = der.ReadBigInteger();
            G = der.ReadBigInteger();
            Y = der.ReadBigInteger();
            X = der.ReadBigInteger();

            if (!der.IsEndOfData)
            {
                throw new InvalidOperationException("Invalid private key (expected EOF).");
            }

            DSA = LoadDSA();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DsaKey" /> class.
        /// </summary>
        /// <param name="p">The p.</param>
        /// <param name="q">The q.</param>
        /// <param name="g">The g.</param>
        /// <param name="y">The y.</param>
        /// <param name="x">The x.</param>
        public DsaKey(BigInteger p, BigInteger q, BigInteger g, BigInteger y, BigInteger x)
        {
            P = p;
            Q = q;
            G = g;
            Y = y;
            X = x;

            DSA = LoadDSA();
        }

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
#pragma warning disable CA5384 // Do Not Use Digital Signature Algorithm (DSA)
        private DSA LoadDSA()
        {
#if NETFRAMEWORK
            // On .NET Framework we use the concrete CNG type which is FIPS-186-3
            // compatible. The CryptoServiceProvider type returned by DSA.Create()
            // is limited to FIPS-186-1 (max 1024 bit key).
            var dsa = new DSACng();
#else
            var dsa = DSA.Create();
#endif
            dsa.ImportParameters(GetDSAParameters());

            return dsa;
        }
#pragma warning restore CA5384 // Do Not Use Digital Signature Algorithm (DSA)
#pragma warning restore CA1859 // Use concrete types when possible for improved performance

        internal DSAParameters GetDSAParameters()
        {
            // P, G, Y, Q are required.
            // P, G, Y must have the same length.
            // If X is present, it must have the same length as Q.

            // See https://github.com/dotnet/runtime/blob/fadd8313653f71abd0068c8bf914be88edb2c8d3/src/libraries/Common/src/System/Security/Cryptography/DSACng.ImportExport.cs#L23
            // and https://github.com/dotnet/runtime/blob/fadd8313653f71abd0068c8bf914be88edb2c8d3/src/libraries/Common/src/System/Security/Cryptography/DSAKeyFormatHelper.cs#L18
            // (and similar code in RsaKey.cs)

            var ret = new DSAParameters
            {
                P = P.ToByteArray(isUnsigned: true, isBigEndian: true),
                Q = Q.ToByteArray(isUnsigned: true, isBigEndian: true),
            };

            ret.G = G.ExportKeyParameter(ret.P.Length);
            ret.Y = Y.ExportKeyParameter(ret.P.Length);

            if (!X.IsZero)
            {
                ret.X = X.ExportKeyParameter(ret.Q.Length);
            }

            return ret;
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
                DSA.Dispose();
            }
        }
    }
}
