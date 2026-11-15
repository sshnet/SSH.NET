#if NETFRAMEWORK || NET11_0_OR_GREATER
using System.Security.Cryptography;

namespace Renci.SshNet.Security
{
    internal partial class KeyExchangeECCurve25519
    {
        protected sealed class BclImpl : Impl
        {
            private readonly X25519DiffieHellman _clientX25519DH;

            public BclImpl()
            {
                _clientX25519DH = X25519DiffieHellman.GenerateKey();
            }

            public override byte[] GenerateClientPublicKey()
            {
                return _clientX25519DH.ExportPublicKey();
            }

            public override byte[] CalculateAgreement(byte[] serverPublicKey)
            {
                using var serverX25519DH = X25519DiffieHellman.ImportPublicKey(serverPublicKey);

                return _clientX25519DH.DeriveRawSecretAgreement(serverX25519DH.ExportPublicKey());
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (disposing)
                {
                    _clientX25519DH.Dispose();
                }
            }
        }
    }
}
#endif
