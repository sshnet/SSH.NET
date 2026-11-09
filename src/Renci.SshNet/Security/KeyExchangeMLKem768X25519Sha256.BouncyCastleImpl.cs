using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Kems;
using Org.BouncyCastle.Crypto.Parameters;

using Renci.SshNet.Abstractions;

namespace Renci.SshNet.Security
{
    internal sealed partial class KeyExchangeMLKem768X25519Sha256
    {
        private sealed class MLKemBouncyCastleImpl : Impl
        {
            private MLKemDecapsulator _mlkemDecapsulator;

            public override byte[] GenerateClientPublicKey()
            {
                var mlkem768KeyPairGenerator = new MLKemKeyPairGenerator();
                mlkem768KeyPairGenerator.Init(new MLKemKeyGenerationParameters(CryptoAbstraction.SecureRandom, MLKemParameters.ml_kem_768));
                var mlkem768KeyPair = mlkem768KeyPairGenerator.GenerateKeyPair();

                _mlkemDecapsulator = new MLKemDecapsulator(MLKemParameters.ml_kem_768);
                _mlkemDecapsulator.Init(mlkem768KeyPair.Private);

                return ((MLKemPublicKeyParameters)mlkem768KeyPair.Public).GetEncoded();
            }

            public override byte[] CalculateAgreement(byte[] serverPublicKey)
            {
                var mlkemSecret = new byte[_mlkemDecapsulator.SecretLength];
                _mlkemDecapsulator.Decapsulate(serverPublicKey, 0, _mlkemDecapsulator.EncapsulationLength, mlkemSecret, 0, _mlkemDecapsulator.SecretLength);

                return mlkemSecret;
            }
        }
    }
}
