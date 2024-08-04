using System;
#if NETFRAMEWORK
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
#endif // NETFRAMEWORK
using System.Security.Cryptography;
using System.Text;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains ECDSA (ecdsa-sha2-nistp{256,384,521}) private and public key.
    /// </summary>
    public class EcdsaKey : Key, IDisposable
    {
#pragma warning disable SA1310 // Field names should not contain underscore
        private const string ECDSA_P256_OID_VALUE = "1.2.840.10045.3.1.7"; // Also called nistP256 or secP256r1
        private const string ECDSA_P384_OID_VALUE = "1.3.132.0.34"; // Also called nistP384 or secP384r1
        private const string ECDSA_P521_OID_VALUE = "1.3.132.0.35"; // Also called nistP521or secP521r1
#pragma warning restore SA1310 // Field names should not contain underscore

        private int _keySize;
        private EcdsaDigitalSignature _digitalSignature;
        private bool _isDisposed;

#if NETFRAMEWORK
        private CngKey _key;

        internal enum KeyBlobMagicNumber
        {
            BCRYPT_ECDSA_PUBLIC_P256_MAGIC = 0x31534345,
            BCRYPT_ECDSA_PRIVATE_P256_MAGIC = 0x32534345,
            BCRYPT_ECDSA_PUBLIC_P384_MAGIC = 0x33534345,
            BCRYPT_ECDSA_PRIVATE_P384_MAGIC = 0x34534345,
            BCRYPT_ECDSA_PUBLIC_P521_MAGIC = 0x35534345,
            BCRYPT_ECDSA_PRIVATE_P521_MAGIC = 0x36534345,

            BCRYPT_ECDH_PUBLIC_P256_MAGIC = 0x314B4345,
            BCRYPT_ECDH_PRIVATE_P256_MAGIC = 0x324B4345,
            BCRYPT_ECDH_PUBLIC_P384_MAGIC = 0x334B4345,
            BCRYPT_ECDH_PRIVATE_P384_MAGIC = 0x344B4345,
            BCRYPT_ECDH_PUBLIC_P521_MAGIC = 0x354B4345,
            BCRYPT_ECDH_PRIVATE_P521_MAGIC = 0x364B4345,

            BCRYPT_ECDH_PUBLIC_GENERIC_MAGIC = 0x504B4345,
            BCRYPT_ECDH_PRIVATE_GENERIC_MAGIC = 0x564B4345,
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BCRYPT_ECCKEY_BLOB
        {
            internal KeyBlobMagicNumber Magic;
            internal int CbKey;
        }
#endif

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
        /// Gets the Digest to use.
        /// </summary>
        public IDigest Digest
        {
            get
            {
                switch (KeyLength)
                {
                    case 256:
                        return new Sha256Digest();
                    case 384:
                        return new Sha384Digest();
                    case 521:
                        return new Sha512Digest();
                    default:
                        throw new SshException("Unknown KeySize: " + KeyLength.ToString());
                }
            }
        }

#if NETFRAMEWORK
        /// <summary>
        /// Gets the HashAlgorithm to use.
        /// </summary>
        public CngAlgorithm HashAlgorithm
        {
            get
            {
                switch (Ecdsa.KeySize)
                {
                    case 256:
                        return CngAlgorithm.Sha256;
                    case 384:
                        return CngAlgorithm.Sha384;
                    case 521:
                        return CngAlgorithm.Sha512;
                    default:
                        throw new SshException("Unknown KeySize: " + Ecdsa.KeySize.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
#else
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
#endif

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
                return _keySize;
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
                byte[] curve;
                byte[] qx;
                byte[] qy;

                if (PublicKeyParameters != null)
                {
                    var oid = PublicKeyParameters.PublicKeyParamSet.GetID();
                    switch (oid)
                    {
                        case ECDSA_P256_OID_VALUE:
                            curve = Encoding.ASCII.GetBytes("nistp256");
                            break;
                        case ECDSA_P384_OID_VALUE:
                            curve = Encoding.ASCII.GetBytes("nistp384");
                            break;
                        case ECDSA_P521_OID_VALUE:
                            curve = Encoding.ASCII.GetBytes("nistp521");
                            break;
                        default:
                            throw new SshException("Unexpected OID: " + oid);
                    }

                    qx = PublicKeyParameters.Q.XCoord.GetEncoded();
                    qy = PublicKeyParameters.Q.YCoord.GetEncoded();
                }
                else
                {
#if NETFRAMEWORK
                    var blob = _key.Export(CngKeyBlobFormat.EccPublicBlob);

                    KeyBlobMagicNumber magic;
                    using (var br = new BinaryReader(new MemoryStream(blob)))
                    {
                        magic = (KeyBlobMagicNumber)br.ReadInt32();
                        var cbKey = br.ReadInt32();
                        qx = br.ReadBytes(cbKey);
                        qy = br.ReadBytes(cbKey);
                    }

#pragma warning disable IDE0010 // Add missing cases
                    switch (magic)
                    {
                        case KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P256_MAGIC:
                            curve = Encoding.ASCII.GetBytes("nistp256");
                            break;
                        case KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P384_MAGIC:
                            curve = Encoding.ASCII.GetBytes("nistp384");
                            break;
                        case KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P521_MAGIC:
                            curve = Encoding.ASCII.GetBytes("nistp521");
                            break;
                        default:
                            throw new SshException("Unexpected Curve Magic: " + magic);
                    }
#pragma warning restore IDE0010 // Add missing cases
#else
                    var parameter = Ecdsa.ExportParameters(includePrivateParameters: false);
                    qx = parameter.Q.X;
                    qy = parameter.Q.Y;
                    switch (parameter.Curve.Oid.FriendlyName)
                    {
                        case "ECDSA_P256":
                        case "nistP256":
                            curve = Encoding.ASCII.GetBytes("nistp256");
                            break;
                        case "ECDSA_P384":
                        case "nistP384":
                            curve = Encoding.ASCII.GetBytes("nistp384");
                            break;
                        case "ECDSA_P521":
                        case "nistP521":
                            curve = Encoding.ASCII.GetBytes("nistp521");
                            break;
                        default:
                            throw new SshException("Unexpected Curve Name: " + parameter.Curve.Oid.FriendlyName);
                    }
#endif
                }

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

        internal ECPrivateKeyParameters PrivateKeyParameters
        {
            get;
            private set;
        }

        internal ECPublicKeyParameters PublicKeyParameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the PrivateKey Bytes.
        /// </summary>
        public byte[] PrivateKey { get; private set; }

#if NETFRAMEWORK
        /// <summary>
        /// Gets the <see cref="ECDsa"/> object.
        /// </summary>
        public ECDsaCng Ecdsa { get; private set; }
#else
        /// <summary>
        /// Gets the <see cref="ECDsa"/> object.
        /// </summary>
        public ECDsa Ecdsa { get; private set; }
#endif

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

            var isMono = Type.GetType("Mono.Runtime") != null;

            if (isMono)
            {
                DerObjectIdentifier oid;
                switch (curve_oid)
                {
                    case ECDSA_P256_OID_VALUE:
                        oid = SecObjectIdentifiers.SecP256r1;
                        _keySize = 256;
                        break;
                    case ECDSA_P384_OID_VALUE:
                        oid = SecObjectIdentifiers.SecP384r1;
                        _keySize = 384;
                        break;
                    case ECDSA_P521_OID_VALUE:
                        oid = SecObjectIdentifiers.SecP521r1;
                        _keySize = 521;
                        break;
                    default:
                        throw new SshException("Unexpected OID: " + curve_oid);
                }

                var x9ECParameters = SecNamedCurves.GetByOid(oid);
                var domainParameter = new ECNamedDomainParameters(oid, x9ECParameters);

                if (privatekey != null)
                {
                    privatekey = privatekey.TrimLeadingZeros().Pad(cord_size);
                    PrivateKey = privatekey;

                    PrivateKeyParameters = new ECPrivateKeyParameters(
                        new Org.BouncyCastle.Math.BigInteger(1, privatekey),
                        domainParameter);

                    PublicKeyParameters = new ECPublicKeyParameters(
                        domainParameter.G.Multiply(PrivateKeyParameters.D).Normalize(),
                        domainParameter);
                }
                else
                {
                    PublicKeyParameters = new ECPublicKeyParameters(
                        x9ECParameters.Curve.CreatePoint(
                            new Org.BouncyCastle.Math.BigInteger(1, qx),
                            new Org.BouncyCastle.Math.BigInteger(1, qy)),
                        domainParameter);
                }

                return;
            }

#if NETFRAMEWORK
            KeyBlobMagicNumber curve_magic;

            switch (GetCurveName(curve_oid))
            {
                case "nistp256":
                    if (privatekey != null)
                    {
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P256_MAGIC;
                    }
                    else
                    {
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P256_MAGIC;
                    }

                    break;
                case "nistp384":
                    if (privatekey != null)
                    {
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P384_MAGIC;
                    }
                    else
                    {
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P384_MAGIC;
                    }

                    break;
                case "nistp521":
                    if (privatekey != null)
                    {
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P521_MAGIC;
                    }
                    else
                    {
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P521_MAGIC;
                    }

                    break;
                default:
                    throw new SshException("Unknown: " + curve_oid);
            }

            if (privatekey != null)
            {
                privatekey = privatekey.Pad(cord_size);
                PrivateKey = privatekey;
            }

            var headerSize = Marshal.SizeOf<BCRYPT_ECCKEY_BLOB>();
            var blobSize = headerSize + qx.Length + qy.Length;
            if (privatekey != null)
            {
                blobSize += privatekey.Length;
            }

            var blob = new byte[blobSize];
            using (var bw = new BinaryWriter(new MemoryStream(blob)))
            {
                bw.Write((int)curve_magic);
                bw.Write(cord_size);
                bw.Write(qx); // q.x
                bw.Write(qy); // q.y
                if (privatekey != null)
                {
                    bw.Write(privatekey); // d
                }
            }

            _key = CngKey.Import(blob, privatekey is null ? CngKeyBlobFormat.EccPublicBlob : CngKeyBlobFormat.EccPrivateBlob);

            Ecdsa = new ECDsaCng(_key);
            _keySize = Ecdsa.KeySize;
#else
            var curve = ECCurve.CreateFromValue(curve_oid);
            var parameter = new ECParameters
            {
                Curve = curve
            };

            parameter.Q.X = qx;
            parameter.Q.Y = qy;

            if (privatekey != null)
            {
                parameter.D = privatekey.TrimLeadingZeros().Pad(cord_size);
                PrivateKey = parameter.D;
            }

            Ecdsa = ECDsa.Create(parameter);
            _keySize = Ecdsa.KeySize;
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

#if NETFRAMEWORK
        private static string GetCurveName(string oid)
        {
            switch (oid)
            {
                case ECDSA_P256_OID_VALUE:
                    return "nistp256";
                case ECDSA_P384_OID_VALUE:
                    return "nistp384";
                case ECDSA_P521_OID_VALUE:
                    return "nistp521";
                default:
                    throw new SshException("Unexpected OID: " + oid);
            }
        }
#endif // NETFRAMEWORK

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
