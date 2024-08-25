#if !NET
using System.Text;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    public partial class EcdsaKey
    {
        private int _keySize;

        internal IDigest Digest
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

        private void Import_BouncyCastle(string curve_oid, byte[] qx, byte[] qy, byte[] privatekey)
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
        }

        private void Export_BouncyCastle(out byte[] curve, out byte[] qx, out byte[] qy)
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
    }
}
#endif
