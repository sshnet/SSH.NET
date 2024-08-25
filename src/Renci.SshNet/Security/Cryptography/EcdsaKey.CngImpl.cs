#if NET462
using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    public partial class EcdsaKey : Key, IDisposable
    {
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

        /// <summary>
        /// Gets the <see cref="ECDsaCng"/> object.
        /// </summary>
        public ECDsaCng Ecdsa { get; private set; }

        private void Import_Cng(string curve_oid, int cord_size, byte[] qx, byte[] qy, byte[] privatekey)
        {
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

            try
            {
                _key = CngKey.Import(blob, privatekey is null ? CngKeyBlobFormat.EccPublicBlob : CngKeyBlobFormat.EccPrivateBlob);
                Ecdsa = new ECDsaCng(_key);
            }
            catch (NotImplementedException)
            {
                Import_BouncyCastle(curve_oid, qx, qy, privatekey);
            }
        }

        private void Export_Cng(out byte[] curve, out byte[] qx, out byte[] qy)
        {
            if (PublicKeyParameters != null)
            {
                Export_BouncyCastle(out curve, out qx, out qy);
            }
            else
            {
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
            }
        }

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
    }
}
#endif
