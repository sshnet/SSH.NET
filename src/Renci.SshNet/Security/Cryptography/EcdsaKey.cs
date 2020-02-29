#if FEATURE_ECDSA
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography;

namespace Renci.SshNet.Security
{
    /// <summary>
    /// Contains ECDSA (ecdsa-sha2-nistp{256,384,521}) private and public key
    /// </summary>
    public class EcdsaKey : Key, IDisposable
    {
        internal const string ECDSA_P256_OID_VALUE = "1.2.840.10045.3.1.7"; // Also called nistP256 or secP256r1
        internal const string ECDSA_P384_OID_VALUE = "1.3.132.0.34"; // Also called nistP384 or secP384r1
        internal const string ECDSA_P521_OID_VALUE = "1.3.132.0.35"; // Also called nistP521or secP521r1

#if !NETSTANDARD2_0
        internal enum KeyBlobMagicNumber : int
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
            internal int cbKey;
        }

        private CngKey key;
#endif

        /// <summary>
        /// Gets the SSH name of the ECDSA Key
        /// </summary>
        public override string ToString()
        {
            return string.Format("ecdsa-sha2-nistp{0}", KeyLength);
        }

#if NETSTANDARD2_0
        /// <summary>
        /// Gets the HashAlgorithm to use
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
                }
                return HashAlgorithmName.SHA256;
            }
        }
#else
        /// <summary>
        /// Gets the HashAlgorithm to use
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
                        throw new SshException("Unknown KeySize: " + Ecdsa.KeySize);
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
                return Ecdsa.KeySize;
            }
        }

        private EcdsaDigitalSignature _digitalSignature;

        /// <summary>
        /// Gets the digital signature.
        /// </summary>
        protected override DigitalSignature DigitalSignature
        {
            get
            {
                if (_digitalSignature == null)
                {
                    _digitalSignature = new EcdsaDigitalSignature(this);
                }
                return _digitalSignature;
            }
        }

        /// <summary>
        /// Gets or sets the public.
        /// </summary>
        /// <value>
        /// The public.
        /// </value>
        public override BigInteger[] Public
        {
            get
            {
                byte[] curve;
                byte[] qx;
                byte[] qy;
#if NETSTANDARD2_0
                var parameter = Ecdsa.ExportParameters(false);
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
#else
                var blob = key.Export(CngKeyBlobFormat.EccPublicBlob);

                KeyBlobMagicNumber magic;
                using (var br = new BinaryReader(new MemoryStream(blob)))
                {
                    magic = (KeyBlobMagicNumber)br.ReadInt32();
                    int cbKey = br.ReadInt32();
                    qx = br.ReadBytes(cbKey);
                    qy = br.ReadBytes(cbKey);
                }

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
            set
            {
                var curve_s = Encoding.ASCII.GetString(value[0].ToByteArray().Reverse());
                string curve_oid = GetCurveOid(curve_s);

                var publickey = value[1].ToByteArray().Reverse();
                Import(curve_oid, publickey, null);
            }
        }

        /// <summary>
        /// Gets ECDsa Object
        /// </summary>
        public ECDsa Ecdsa { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EcdsaKey"/> class.
        /// </summary>
        public EcdsaKey()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EcdsaKey"/> class.
        /// </summary>
        /// <param name="curve">The curve name</param>
        /// <param name="publickey">Value of publickey</param>
        /// <param name="privatekey">Value of privatekey</param>
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
            var version = der.ReadBigInteger(); // skip version

            // PrivateKey
            var privatekey = der.ReadOctetString().TrimLeadingZeros();

            // Construct
            var s0 = der.ReadByte();
            if ((s0 & 0xe0) != 0xa0)
                throw new SshException(string.Format("UnexpectedDER: wanted constructed tag (0xa0-0xbf), got: {0:X}", s0));
            var tag = s0 & 0x1f;
            if (tag != 0)
                throw new SshException(string.Format("expected tag 0 in DER privkey, got: {0}", tag));
            var construct = der.ReadBytes(der.ReadLength()); // object length

            // curve OID
            var curve_der = new DerData(construct, true);
            var curve = curve_der.ReadObject();

            // Construct
            s0 = der.ReadByte();
            if ((s0 & 0xe0) != 0xa0)
                throw new SshException(string.Format("UnexpectedDER: wanted constructed tag (0xa0-0xbf), got: {0:X}", s0));
            tag = s0 & 0x1f;
            if (tag != 1)
                throw new SshException(string.Format("expected tag 1 in DER privkey, got: {0}", tag));
            construct = der.ReadBytes(der.ReadLength()); // object length

            // PublicKey
            var pubkey_der = new DerData(construct, true);
            var pubkey = pubkey_der.ReadBitString().TrimLeadingZeros();

            Import(OidByteArrayToString(curve), pubkey, privatekey);
        }

        private void Import(string curve_oid, byte[] publickey, byte[] privatekey)
        {
#if NETSTANDARD2_0
            var curve = ECCurve.CreateFromValue(curve_oid);
            var parameter = new ECParameters
            {
                Curve = curve
            };

            // ECPoint as BigInteger(2)
            var cord_size = (publickey.Length - 1) / 2;
            var qx = new byte[cord_size];
            Buffer.BlockCopy(publickey, 1, qx, 0, qx.Length);

            var qy = new byte[cord_size];
            Buffer.BlockCopy(publickey, cord_size + 1, qy, 0, qy.Length);

            parameter.Q.X = qx;
            parameter.Q.Y = qy;

            if (privatekey != null)
                parameter.D = privatekey.TrimLeadingZeros().Pad(cord_size);

            Ecdsa = ECDsa.Create(parameter);
#else
            var curve_magic = KeyBlobMagicNumber.BCRYPT_ECDH_PRIVATE_GENERIC_MAGIC;
            switch (GetCurveName(curve_oid))
            {
                case "nistp256":
                    if (privatekey != null)
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P256_MAGIC;
                    else
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P256_MAGIC;
                    break;
                case "nistp384":
                    if (privatekey != null)
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P384_MAGIC;
                    else
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P384_MAGIC;
                    break;
                case "nistp521":
                    if (privatekey != null)
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P521_MAGIC;
                    else
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P521_MAGIC;
                    break;
                default:
                    throw new SshException("Unknown: " + curve_oid);
            }

            // ECPoint as BigInteger(2)
            var cord_size = (publickey.Length - 1) / 2;
            var qx = new byte[cord_size];
            Buffer.BlockCopy(publickey, 1, qx, 0, qx.Length);

            var qy = new byte[cord_size];
            Buffer.BlockCopy(publickey, cord_size + 1, qy, 0, qy.Length);

            if (privatekey != null)
                privatekey = privatekey.Pad(cord_size);

            int headerSize = Marshal.SizeOf(typeof(BCRYPT_ECCKEY_BLOB));
            int blobSize = headerSize + qx.Length + qy.Length;
            if (privatekey != null)
                blobSize += privatekey.Length;

            byte[] blob = new byte[blobSize];
            using (var bw = new BinaryWriter(new MemoryStream(blob)))
            {
                bw.Write((int)curve_magic);
                bw.Write(cord_size);
                bw.Write(qx); // q.x
                bw.Write(qy); // q.y
                if (privatekey != null)
                    bw.Write(privatekey); // d
            }
            key = CngKey.Import(blob, privatekey == null ? CngKeyBlobFormat.EccPublicBlob : CngKeyBlobFormat.EccPrivateBlob);

            Ecdsa = new ECDsaCng(key);
#endif
        }

        private string GetCurveOid(string curve_s)
        {
            switch (curve_s.ToLower())
            {
                case "nistp256":
                    return ECDSA_P256_OID_VALUE;
                case "nistp384":
                    return ECDSA_P384_OID_VALUE;
                case "nistp521":
                    return ECDSA_P521_OID_VALUE;
                default:
                    throw new SshException("Unexpected Curve Name: " + curve_s);
            }
        }

        private string GetCurveName(string oid)
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

        private string OidByteArrayToString(byte[] oid)
        {
            StringBuilder retVal = new StringBuilder();

            for (int i = 0; i < oid.Length; i++)
            {
                if (i == 0)
                {
                    int b = oid[0] % 40;
                    int a = (oid[0] - b) / 40;
                    retVal.AppendFormat("{0}.{1}", a, b);
                }
                else
                {
                    if (oid[i] < 128)
                        retVal.AppendFormat(".{0}", oid[i]);
                    else
                    {
                        retVal.AppendFormat(".{0}",
                           ((oid[i] - 128) * 128) + oid[i + 1]);
                        i++;
                    }
                }
            }

            return retVal.ToString();
        }

        #region IDisposable Members

        private bool _isDisposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="DsaKey"/> is reclaimed by garbage collection.
        /// </summary>
        ~EcdsaKey()
        {
            Dispose(false);
        }

        #endregion
    }
}
#endif