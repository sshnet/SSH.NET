#if NET
using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
    internal partial class KeyExchangeECCurve25519
    {
        protected sealed class BclImpl : Impl
        {
            private readonly ECCurve _curve;
            private readonly ECDiffieHellman _clientECDH;

            public BclImpl(ECCurve curve)
            {
                _curve = curve;
                _clientECDH = ECDiffieHellman.Create();
            }

            public override byte[] GenerateClientECPoint()
            {
                _clientECDH.GenerateKey(_curve);

                var q = _clientECDH.PublicKey.ExportParameters().Q;

                return q.X;
            }

            public override byte[] CalculateAgreement(byte[] serverECPoint)
            {
                var parameters = new ECParameters
                {
                    Curve = _curve,
                    Q = new ECPoint
                    {
                        X = serverECPoint,
                        Y = new byte[serverECPoint.Length]
                    },
                };

                using var serverECDH = ECDiffieHellman.Create(parameters);

                return _clientECDH.DeriveRawSecretAgreement(serverECDH.PublicKey);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    _clientECDH.Dispose();
                }
            }
        }
    }
}
#endif
