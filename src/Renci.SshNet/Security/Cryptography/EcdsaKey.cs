using System;
using System.Security.Cryptography;
using System.Text;

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains ECDSA (ecdsa-sha2-nistp{256,384,521}) private and public key.
    /// </summary>
    public partial class EcdsaKey : Key, IDisposable
    {
#pragma warning disable SA1310 // Field names should not contain underscore
        private const string ECDSA_P256_OID_VALUE = "1.2.840.10045.3.1.7"; // Also called nistP256 or secP256r1
        private const string ECDSA_P384_OID_VALUE = "1.3.132.0.34"; // Also called nistP384 or secP384r1
        private const string ECDSA_P521_OID_VALUE = "1.3.132.0.35"; // Also called nistP521or secP521r1
#pragma warning restore SA1310 // Field names should not contain underscore

        private EcdsaDigitalSignature _digitalSignature;
        private bool _isDisposed;

        /// <summary>
        /// Gets the SSH name of the ECDSA Key.
        /// </summary>
        /// <returns>
        /// The SSH name of the ECDSA Key.
        /// </returns>
        public override string ToString()
        {
            return string.Format("ecdsa-sha2-nistp{0}", KeyLength);
        }

        /// <summary>
        /// Gets the HashAlgorithm to use.
        /// </summary>
        public HashAlgorithmName HashAlgorithm
        {
            get
            {
                switch (KeyLength)
                {
                    case 256:
                        return HashAlgorithmName.SHA256;
                    case 384:
                        return HashAlgorithmName.SHA384;
                    case 521:
                        return HashAlgorithmName.SHA512;
                    default:
                        return HashAlgorithmName.SHA256;
                }
            }
        }

        /// <summary>
        /// Gets the length of the key.
        /// </summary>
        /// <value>
        /// The length of the key.
        /// </value>
        public override int KeyLength
        {
            get
            {
#if !NET
                return Ecdsa?.KeySize ?? _keySize;
#else
                return Ecdsa.KeySize;
#endif
            }
        }

        /// <summary>
        /// Gets the digital signature.
        /// </summary>
        protected internal override DigitalSignature DigitalSignature
        {
            get
            {
                _digitalSignature ??= new EcdsaDigitalSignature(this);

                return _digitalSignature;
            }
        }

        /// <summary>
        /// Gets the ECDSA public key.
        /// </summary>
        /// <value>
        /// An array with the ASCII-encoded curve identifier (e.g. "nistp256")
        /// at index 0, and the public curve point Q at index 1.
        /// </value>
        public override BigInteger[] Public
        {
            get
            {
#if NET462
                Export_Cng(out var curve, out var qx, out var qy);
#else
                Export_Bcl(out var curve, out var qx, out var qy);
#endif

                // Make ECPoint from x and y
                // Prepend 04 (uncompressed format) + qx-bytes + qy-bytes
                var q = new byte[1 + qx.Length + qy.Length];
                Buffer.SetByte(q, 0, 4);
                Buffer.BlockCopy(qx, 0, q, 1, qx.Length);
                Buffer.BlockCopy(qy, 0, q, qx.Length + 1, qy.Length);

                // returns Curve-Name and x/y as ECPoint
                return new[] { new BigInteger(curve.Reverse()), new BigInteger(q.Reverse()) };
            }
        }

        /// <summary>
        /// Gets the PrivateKey Bytes.
        /// </summary>
        public byte[] PrivateKey { get; private set; }

        /// <summary>
        /// Gets the <see cref="ECDsa"/> object.
        /// </summary>
        public ECDsa Ecdsa { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EcdsaKey"/> class.
        /// </summary>
        /// <param name="publicKeyData">The encoded public key data.</param>
        public EcdsaKey(SshKeyData publicKeyData)
        {
            if (publicKeyData is null)
            {
                throw new ArgumentNullException(nameof(publicKeyData));
            }

            if (!publicKeyData.Name.StartsWith("ecdsa-sha2-", StringComparison.Ordinal) || publicKeyData.Keys.Length != 2)
            {
                throw new ArgumentException($"Invalid ECDSA public key data. ({publicKeyData.Name}, {publicKeyData.Keys.Length}).", nameof(publicKeyData));
            }

            var curve_s = Encoding.ASCII.GetString(publicKeyData.Keys[0].ToByteArray().Reverse());
            var curve_oid = GetCurveOid(curve_s);

            var publickey = publicKeyData.Keys[1].ToByteArray().Reverse();
            Import(curve_oid, publickey, privatekey: null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EcdsaKey"/> class.
        /// </summary>
        /// <param name="curve">The curve name.</param>
        /// <param name="publickey">Value of publickey.</param>
        /// <param name="privatekey">Value of privatekey.</param>
        public EcdsaKey(string curve, byte[] publickey, byte[] privatekey)
        {
            Import(GetCurveOid(curve), publickey, privatekey);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EcdsaKey"/> class.
        /// </summary>
        /// <param name="data">DER encoded private key data.</param>
        public EcdsaKey(byte[] data)
        {
            var der = new DerData(data);
            _ = der.ReadBigInteger(); // skip version

            // PrivateKey
            var privatekey = der.ReadOctetString().TrimLeadingZeros();

            // Construct
            var s0 = der.ReadByte();
            if ((s0 & 0xe0) != 0xa0)
            {
                throw new SshException(string.Format("UnexpectedDER: wanted constructed tag (0xa0-0xbf), got: {0:X}", s0));
            }

            var tag = s0 & 0x1f;
            if (tag != 0)
            {
                throw new SshException(string.Format("expected tag 0 in DER privkey, got: {0}", tag));
            }

            var construct = der.ReadBytes(der.ReadLength()); // object length

            // curve OID
            var curve_der = new DerData(construct, construct: true);
            var curve = curve_der.ReadObject();

            // Construct
            s0 = der.ReadByte();
            if ((s0 & 0xe0) != 0xa0)
            {
                throw new SshException(string.Format("UnexpectedDER: wanted constructed tag (0xa0-0xbf), got: {0:X}", s0));
            }

            tag = s0 & 0x1f;
            if (tag != 1)
            {
                throw new SshException(string.Format("expected tag 1 in DER privkey, got: {0}", tag));
            }

            construct = der.ReadBytes(der.ReadLength()); // object length

            // PublicKey
            var pubkey_der = new DerData(construct, construct: true);
            var pubkey = pubkey_der.ReadBitString().TrimLeadingZeros();

            Import(OidByteArrayToString(curve), pubkey, privatekey);
        }

        private void Import(string curve_oid, byte[] publickey, byte[] privatekey)
        {
            // ECPoint as BigInteger(2)
            var cord_size = (publickey.Length - 1) / 2;
            var qx = new byte[cord_size];
            Buffer.BlockCopy(publickey, 1, qx, 0, qx.Length);

            var qy = new byte[cord_size];
            Buffer.BlockCopy(publickey, cord_size + 1, qy, 0, qy.Length);
#if NET462
            Import_Cng(curve_oid, cord_size, qx, qy, privatekey);
#else
            Import_Bcl(curve_oid, cord_size, qx, qy, privatekey);
#endif
        }

        private static string GetCurveOid(string curve_s)
        {
            switch (curve_s.ToUpperInvariant())
            {
                case "NISTP256":
                    return ECDSA_P256_OID_VALUE;
                case "NISTP384":
                    return ECDSA_P384_OID_VALUE;
                case "NISTP521":
                    return ECDSA_P521_OID_VALUE;
                default:
                    throw new SshException("Unexpected Curve Name: " + curve_s);
            }
        }

        private static string OidByteArrayToString(byte[] oid)
        {
            var retVal = new StringBuilder();

            for (var i = 0; i < oid.Length; i++)
            {
                if (i == 0)
                {
                    var b = oid[0] % 40;
                    var a = (oid[0] - b) / 40;
                    _ = retVal.AppendFormat("{0}.{1}", a, b);
                }
                else
                {
                    if (oid[i] < 128)
                    {
                        _ = retVal.AppendFormat(".{0}", oid[i]);
                    }
                    else
                    {
                        _ = retVal.AppendFormat(".{0}", ((oid[i] - 128) * 128) + oid[i + 1]);
                        i++;
                    }
                }
            }

            return retVal.ToString();
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
            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="EcdsaKey"/> class.
        /// </summary>
        ~EcdsaKey()
        {
            Dispose(disposing: false);
        }
    }
}
