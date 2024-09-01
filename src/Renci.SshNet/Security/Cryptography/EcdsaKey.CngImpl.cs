#if NET462
#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    public partial class EcdsaKey : Key, IDisposable
    {
        private sealed class CngImpl : Impl
        {
            private readonly CngKey _key;
            private readonly HashAlgorithmName _hashAlgorithmName;

            private enum KeyBlobMagicNumber
            {
                BCRYPT_ECDSA_PUBLIC_P256_MAGIC = 0x31534345,
                BCRYPT_ECDSA_PRIVATE_P256_MAGIC = 0x32534345,
                BCRYPT_ECDSA_PUBLIC_P384_MAGIC = 0x33534345,
                BCRYPT_ECDSA_PRIVATE_P384_MAGIC = 0x34534345,
                BCRYPT_ECDSA_PUBLIC_P521_MAGIC = 0x35534345,
                BCRYPT_ECDSA_PRIVATE_P521_MAGIC = 0x36534345,
            }

            public CngImpl(string curve_oid, int cord_size, byte[] qx, byte[] qy, byte[]? privatekey)
            {
                KeyBlobMagicNumber curve_magic;

                switch (curve_oid)
                {
                    case ECDSA_P256_OID_VALUE:
                        curve_magic = privatekey != null
                            ? KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P256_MAGIC
                            : KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P256_MAGIC;
                        break;
                    case ECDSA_P384_OID_VALUE:
                        curve_magic = privatekey != null
                            ? KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P384_MAGIC
                            : KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P384_MAGIC;
                        break;
                    case ECDSA_P521_OID_VALUE:
                        curve_magic = privatekey != null
                            ? KeyBlobMagicNumber.BCRYPT_ECDSA_PRIVATE_P521_MAGIC
                            : KeyBlobMagicNumber.BCRYPT_ECDSA_PUBLIC_P521_MAGIC;
                        break;
                    default:
                        throw new SshException("Unknown: " + curve_oid);
                }

                if (privatekey != null)
                {
                    privatekey = privatekey.Pad(cord_size);
                    PrivateKey = privatekey;
                }

                var blobSize = sizeof(int) + sizeof(int) + qx.Length + qy.Length;
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

                _hashAlgorithmName = KeyLength switch
                {
                    <= 256 => HashAlgorithmName.SHA256,
                    <= 384 => HashAlgorithmName.SHA384,
                    _ => HashAlgorithmName.SHA512,
                };
            }

            public override byte[]? PrivateKey { get; }

            public override ECDsa Ecdsa { get; }

            public override int KeyLength
            {
                get
                {
                    return Ecdsa.KeySize;
                }
            }

            public override byte[] Sign(byte[] input)
            {
                return Ecdsa.SignData(input, _hashAlgorithmName);
            }

            public override bool Verify(byte[] input, byte[] signature)
            {
                return Ecdsa.VerifyData(input, signature, _hashAlgorithmName);
            }

            public override void Export(out byte[] qx, out byte[] qy)
            {
                var blob = _key.Export(CngKeyBlobFormat.EccPublicBlob);

                using (var br = new BinaryReader(new MemoryStream(blob)))
                {
                    var magic = br.ReadInt32();
                    Debug.Assert(Enum.IsDefined(typeof(KeyBlobMagicNumber), magic), magic.ToString("x"));
                    var cbKey = br.ReadInt32();
                    qx = br.ReadBytes(cbKey);
                    qy = br.ReadBytes(cbKey);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Ecdsa.Dispose();
                    _key.Dispose();
                }
            }
        }
    }
}
#endif
