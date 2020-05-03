using System;

using Renci.SshNet.Security.Org.BouncyCastle.Math;
using Renci.SshNet.Security.Org.BouncyCastle.Math.EC;
using Renci.SshNet.Security.Org.BouncyCastle.Crypto.Parameters;

namespace Renci.SshNet.Security.Org.BouncyCastle.Crypto.Agreement
{
    internal class ECDHCBasicAgreement
    {
        private ECPrivateKeyParameters privKey;

        public virtual void Init(
            AsymmetricKeyParameter parameters)
        {
            this.privKey = (ECPrivateKeyParameters)parameters;
        }

        public virtual int GetFieldSize()
        {
            return (privKey.Parameters.Curve.FieldSize + 7) / 8;
        }

        public virtual BigInteger CalculateAgreement(
            ECPublicKeyParameters pubKey)
        {
            ECPublicKeyParameters pub = pubKey;
            ECDomainParameters dp = privKey.Parameters;
            if (!dp.Equals(pub.Parameters))
                throw new InvalidOperationException("ECDHC public key has wrong domain parameters");

            BigInteger hd = dp.H.Multiply(privKey.D).Mod(dp.N);

            // Always perform calculations on the exact curve specified by our private key's parameters
            ECPoint pubPoint = ECAlgorithms.CleanPoint(dp.Curve, pub.Q);
            if (pubPoint.IsInfinity)
                throw new InvalidOperationException("Infinity is not a valid public key for ECDHC");

            ECPoint P = pubPoint.Multiply(hd).Normalize();
            if (P.IsInfinity)
                throw new InvalidOperationException("Infinity is not a valid agreement value for ECDHC");

            return P.AffineXCoord.ToBigInteger();
        }
    }
}