using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
    internal sealed partial class KeyExchangeMLKem768X25519Sha256
    {
        private sealed class MLKemBclImpl : Impl
        {
            private MLKem _mlkem;

            public override byte[] GenerateClientPublicKey()
            {
                _mlkem = MLKem.GenerateKey(MLKemAlgorithm.MLKem768);
                return _mlkem.ExportEncapsulationKey();
            }

            public override byte[] CalculateAgreement(byte[] serverPublicKey)
            {
                var mlkemSecret = new byte[MLKemAlgorithm.MLKem768.SharedSecretSizeInBytes];
                _mlkem.Decapsulate(serverPublicKey.AsSpan(0, MLKemAlgorithm.MLKem768.CiphertextSizeInBytes), mlkemSecret);
                return mlkemSecret;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _mlkem?.Dispose();
                }

                base.Dispose(disposing);
            }
        }
    }
}
