using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Security
{
    internal abstract partial class KeyExchangeECDH
    {
        private sealed class BouncyCastleImpl : Impl
        {
            private readonly ECDomainParameters _domainParameters;
            private readonly ECDHCBasicAgreement _keyAgreement;

            public BouncyCastleImpl(X9ECParameters curveParameters)
            {
                _domainParameters = new ECDomainParameters(curveParameters);
                _keyAgreement = new ECDHCBasicAgreement();
            }

            public override byte[] GenerateClientECPoint()
            {
                var g = new ECKeyPairGenerator();
                g.Init(new ECKeyGenerationParameters(_domainParameters, CryptoAbstraction.SecureRandom));

                var aKeyPair = g.GenerateKeyPair();
                _keyAgreement.Init(aKeyPair.Private);

                return ((ECPublicKeyParameters)aKeyPair.Public).Q.GetEncoded();
            }

            public override byte[] CalculateAgreement(byte[] serverECPoint)
            {
                var c = _domainParameters.Curve;
                var q = c.DecodePoint(serverECPoint);
                var publicKey = new ECPublicKeyParameters("ECDH", q, _domainParameters);

                return _keyAgreement.CalculateAgreement(publicKey).ToByteArray();
            }
        }
    }
}
