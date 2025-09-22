using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Security
{
    internal partial class KeyExchangeECCurve25519
    {
        private sealed class BouncyCastleImpl : Impl
        {
            private X25519Agreement _keyAgreement;

            public override byte[] GenerateClientECPoint()
            {
                var g = new X25519KeyPairGenerator();
                g.Init(new X25519KeyGenerationParameters(CryptoAbstraction.SecureRandom));

                var aKeyPair = g.GenerateKeyPair();
                _keyAgreement = new X25519Agreement();
                _keyAgreement.Init(aKeyPair.Private);

                return ((X25519PublicKeyParameters)aKeyPair.Public).GetEncoded();
            }

            public override byte[] CalculateAgreement(byte[] serverECPoint)
            {
                var publicKey = new X25519PublicKeyParameters(serverECPoint);

                var k1 = new byte[_keyAgreement.AgreementSize];
                _keyAgreement.CalculateAgreement(publicKey, k1, 0);

                return k1;
            }
        }
    }
}
