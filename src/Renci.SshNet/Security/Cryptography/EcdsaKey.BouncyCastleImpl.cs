#if !NET
#nullable enable
using System.Security.Cryptography;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    public partial class EcdsaKey
    {
        private sealed class BouncyCastleImpl : Impl
        {
            private readonly ECPublicKeyParameters _publicKeyParameters;
            private readonly ECPrivateKeyParameters? _privateKeyParameters;
            private readonly DsaDigestSigner _signer;

            public BouncyCastleImpl(string curve_oid, byte[] qx, byte[] qy, byte[]? privatekey)
            {
                DerObjectIdentifier oid;
                IDigest digest;
                switch (curve_oid)
                {
                    case ECDSA_P256_OID_VALUE:
                        oid = SecObjectIdentifiers.SecP256r1;
                        digest = new Sha256Digest();
                        KeyLength = 256;
                        break;
                    case ECDSA_P384_OID_VALUE:
                        oid = SecObjectIdentifiers.SecP384r1;
                        digest = new Sha384Digest();
                        KeyLength = 384;
                        break;
                    case ECDSA_P521_OID_VALUE:
                        oid = SecObjectIdentifiers.SecP521r1;
                        digest = new Sha512Digest();
                        KeyLength = 521;
                        break;
                    default:
                        throw new SshException("Unexpected OID: " + curve_oid);
                }

                _signer = new DsaDigestSigner(new ECDsaSigner(), digest, PlainDsaEncoding.Instance);

                var x9ECParameters = SecNamedCurves.GetByOid(oid);
                var domainParameter = new ECNamedDomainParameters(oid, x9ECParameters);

                if (privatekey != null)
                {
                    _privateKeyParameters = new ECPrivateKeyParameters(
                        new Org.BouncyCastle.Math.BigInteger(1, privatekey),
                        domainParameter);

                    _publicKeyParameters = new ECPublicKeyParameters(
                        domainParameter.G.Multiply(_privateKeyParameters.D).Normalize(),
                        domainParameter);
                }
                else
                {
                    _publicKeyParameters = new ECPublicKeyParameters(
                        x9ECParameters.Curve.CreatePoint(
                            new Org.BouncyCastle.Math.BigInteger(1, qx),
                            new Org.BouncyCastle.Math.BigInteger(1, qy)),
                        domainParameter);
                }
            }

            public override byte[]? PrivateKey { get; }

            public override ECDsa? Ecdsa { get; }

            public override int KeyLength { get; }

            public override byte[] Sign(byte[] input)
            {
                _signer.Init(forSigning: true, _privateKeyParameters);
                _signer.BlockUpdate(input, 0, input.Length);

                return _signer.GenerateSignature();
            }

            public override bool Verify(byte[] input, byte[] signature)
            {
                _signer.Init(forSigning: false, _publicKeyParameters);
                _signer.BlockUpdate(input, 0, input.Length);

                return _signer.VerifySignature(signature);
            }

            public override void Export(out byte[] qx, out byte[] qy)
            {
                qx = _publicKeyParameters.Q.XCoord.GetEncoded();
                qy = _publicKeyParameters.Q.YCoord.GetEncoded();
            }

            protected override void Dispose(bool disposing)
            {
            }
        }
    }
}
#endif
