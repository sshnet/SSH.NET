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

        /// <summary>
        /// Gets dsa
        /// </summary>
        public ECDsaCng dsa;

        private CngKey key;

        /// <summary>
        /// Gets the SSH name of the ECDSA Key
        /// </summary>
        public override string ToString()
        {
            return string.Format("ecdsa-sha2-nistp{0}", KeyLength);
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
                return key.KeySize;
            }
        }

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
                var blob = key.Export(CngKeyBlobFormat.EccPublicBlob);

                byte[] qx;
                byte[] qy;
                KeyBlobMagicNumber magic;
                using (var br = new BinaryReader(new MemoryStream(blob)))
                {
                    magic = (KeyBlobMagicNumber)br.ReadInt32();
                    int cbKey = br.ReadInt32();
                    qx = br.ReadBytes(cbKey);
                    qy = br.ReadBytes(cbKey);
                }

                string curve = "";
                switch (magic)
                {
                    case KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P256_MAGIC:
                        curve = "nistp256";
                        break;
                    case KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P384_MAGIC:
                        curve = "nistp384";
                        break;
                    case KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P521_MAGIC:
                        curve = "nistp521";
                        break;
                    default:
                        throw new SshException("Unexpected Curve Magic: " + magic);
                }
                var curve_bn = new BigInteger(Encoding.ASCII.GetBytes(curve).Reverse());

                // Make ECPoint from x and y
                // Prepend 04 (uncompressed format) + qx-bytes + qy-bytes
                var point_bn_bytes = new byte[1 + qx.Length + qy.Length];
                Buffer.SetByte(point_bn_bytes, 0, 4);
                Buffer.BlockCopy(qx, 0, point_bn_bytes, 1, qx.Length);
                Buffer.BlockCopy(qy, 0, point_bn_bytes, qx.Length + 1, qy.Length);

                // returns Curve-Name and x/y as ECPoint
                return new[] { curve_bn, new BigInteger(point_bn_bytes.Reverse()) };
            }
            set
            {
                // value[0]:
                // Curve Name as String
                // nistp{256,384,521}
                var value_0 = Encoding.ASCII.GetString(value[0].ToByteArray().Reverse()); // Curve Name as String

                var curve_magic = KeyBlobMagicNumber.BCRYPT_ECDH_PRIVATE_GENERIC_MAGIC;
                var hash_algo = CngAlgorithm.Sha256;
                switch (value_0)
                {
                    case "nistp256":
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P256_MAGIC;
                        hash_algo = CngAlgorithm.Sha256;
                        break;
                    case "nistp384":
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P384_MAGIC;
                        hash_algo = CngAlgorithm.Sha384;
                        break;
                    case "nistp521":
                        curve_magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P521_MAGIC;
                        hash_algo = CngAlgorithm.Sha512;
                        break;
                    default:
                        throw new SshException("Unexpected Curve Name: " + value_0);
                }

                // ECPoint as BigInteger(2)
                var value_1 = value[1].ToByteArray().Reverse();

                var cord_size = (value_1.Length - 1) / 2;
                var value_1_x = new byte[cord_size];
                Buffer.BlockCopy(value_1, 1, value_1_x, 0, value_1_x.Length); // first byte is format. should be checked?

                var value_1_y = new byte[cord_size];
                Buffer.BlockCopy(value_1, cord_size + 1, value_1_y, 0, value_1_y.Length);

                int headerSize = Marshal.SizeOf(typeof(BCRYPT_ECCKEY_BLOB));
                int blobSize = headerSize + (2 * cord_size);
                byte[] blob = new byte[blobSize];

                using (var bw = new BinaryWriter(new MemoryStream(blob)))
                {
                    bw.Write((int)curve_magic);
                    bw.Write(cord_size);
                    bw.Write(value_1_x); // q.x
                    bw.Write(value_1_y); // q.y
                }
                key = CngKey.Import(blob, CngKeyBlobFormat.EccPublicBlob);
                dsa = new ECDsaCng(key)
                {
                    HashAlgorithm = hash_algo
                };
            }
        }

        private EcdsaDigitalSignature _digitalSignature;

        /// <summary>
        /// Initializes a new instance of the <see cref="EcdsaKey"/> class.
        /// </summary>
        public EcdsaKey()
        {
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
            var privatekey = der.ReadOctetString();

            // Construct
            var s0 = der.ReadByte();
            if ((s0 & 0xe0) != 0xa0)
                throw new SshException(string.Format("UnexpectedDER: wanted constructed tag (0xa0-0xbf), got: {0:X}", s0));
            var tag = s0 & 0x1f;
            if (tag != 0)
                throw new SshException(string.Format("expected tag 0 in DER privkey, got: {0}", tag));
            der.ReadByte(); // object length

            // curve OID
            var curve = der.ReadObject();

            var pkcs8_data = PemToPkcs8(privatekey, curve, true);
            key = ImportPkcs8(pkcs8_data, privatekey);
            dsa = new ECDsaCng(key);

            switch (dsa.KeySize)
            {
                case 256:
                    dsa.HashAlgorithm = CngAlgorithm.Sha256;
                    break;
                case 384:
                    dsa.HashAlgorithm = CngAlgorithm.Sha384;
                    break;
                case 521:
                    dsa.HashAlgorithm = CngAlgorithm.Sha512;
                    break;
                default:
                    throw new SshException("Unknown KeySize: " + dsa.KeySize);
            }
        }

        private CngKey ImportPkcs8(byte[] pkcs8_data, byte[] privatekey)
        {
            // There was an issue in older .NET Versions which prevents
            // the usage our keys. Was fixed with .NET 4.6.1. Workaround:
            // Change KeyBlobMagicNumber from Key-exchange (ECDH) to Signing (ECDSA)
            // See: https://stackoverflow.com/a/43982666
            var key =CngKey.Import(pkcs8_data, CngKeyBlobFormat.Pkcs8PrivateBlob);

            var blob = key.Export(CngKeyBlobFormat.EccPublicBlob);
            key.Dispose();
            byte[] qx;
            byte[] qy;
            KeyBlobMagicNumber magic;
            int cbkey;
            using (var br = new BinaryReader(new MemoryStream(blob)))
            {
                magic = (KeyBlobMagicNumber)br.ReadInt32();
                cbkey = br.ReadInt32();
                qx = br.ReadBytes(cbkey);
                qy = br.ReadBytes(cbkey);
            }

            switch (magic)
            {
                case KeyBlobMagicNumber.BCRYPT_ECDH_PUBLIC_P256_MAGIC:
                    magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P256_MAGIC;
                    break;
                case KeyBlobMagicNumber.BCRYPT_ECDH_PUBLIC_P384_MAGIC:
                    magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P384_MAGIC;
                    break;
                case KeyBlobMagicNumber.BCRYPT_ECDH_PUBLIC_P521_MAGIC:
                    magic = KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P521_MAGIC;
                    break;
            }

            int headerSize = Marshal.SizeOf(typeof(BCRYPT_ECCKEY_BLOB));
            int blobSize = headerSize + qx.Length + qy.Length + privatekey.Length;
            byte[] new_blob = new byte[blobSize];

            using (var bw = new BinaryWriter(new MemoryStream(new_blob)))
            {
                bw.Write((int)magic);
                bw.Write(cbkey);
                bw.Write(qx); // q.x
                bw.Write(qy); // q.y
                bw.Write(privatekey); // d
            }
            return CngKey.Import(new_blob, CngKeyBlobFormat.EccPrivateBlob);
        }

        // Since Windows Crypto API is unable to parse/read our PEM format directly
        // we have to convert it to PKCS#8 Format first
        private byte[] PemToPkcs8(byte[] key, byte[] curve, bool isprivate)
        {
            var newdata = new DerData();
            newdata.Write(new BigInteger(0));

            // Create Infos about Content and our Curve
            var objdata = new DerData();
            // 1.2.840.10045.2.1 - ECDSA and ECDH Public Key.
            var obj = new ObjectIdentifier(1, 2, 840, 10045, 2, 1);
            objdata.Write(obj);
            objdata.WriteObjectIdentifier(curve);
            newdata.WriteBytes(objdata.Encode());

            var keydata = new DerData();
            keydata.Write(new BigInteger(isprivate ? 1: 0));
            keydata.Write(key);

            newdata.Write(keydata.Encode());

            return newdata.Encode();
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