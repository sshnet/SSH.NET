#if NET8_0_OR_GREATER
using System;
using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
    internal abstract partial class KeyExchangeECDH
    {
        private sealed class BclImpl : Impl
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

                return EncodeECPoint(q);
            }

            public override byte[] CalculateAgreement(byte[] serverECPoint)
            {
                var q = DecodeECPoint(serverECPoint);

                var parameters = new ECParameters
                {
                    Curve = _curve,
                    Q = q,
                };

                using var serverECDH = ECDiffieHellman.Create(parameters);

                return _clientECDH.DeriveRawSecretAgreement(serverECDH.PublicKey);
            }

            private static byte[] EncodeECPoint(ECPoint point)
            {
                var q = new byte[1 + point.X.Length + point.Y.Length];
                q[0] = 0x04;
                Buffer.BlockCopy(point.X, 0, q, 1, point.X.Length);
                Buffer.BlockCopy(point.Y, 0, q, point.X.Length + 1, point.Y.Length);

                return q;
            }

            private static ECPoint DecodeECPoint(byte[] q)
            {
                var cordSize = (q.Length - 1) / 2;
                var x = new byte[cordSize];
                var y = new byte[cordSize];
                Buffer.BlockCopy(q, 1, x, 0, x.Length);
                Buffer.BlockCopy(q, cordSize + 1, y, 0, y.Length);

                return new ECPoint { X = x, Y = y };
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
