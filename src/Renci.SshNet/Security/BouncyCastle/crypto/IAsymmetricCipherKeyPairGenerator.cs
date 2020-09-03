namespace Renci.SshNet.Security.Org.BouncyCastle.Crypto
{
    internal interface IAsymmetricCipherKeyPairGenerator
    {
        void Init(KeyGenerationParameters parameters);

        AsymmetricCipherKeyPair GenerateKeyPair();
    }
}