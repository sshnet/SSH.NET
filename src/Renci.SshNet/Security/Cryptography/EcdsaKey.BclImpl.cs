#if !NET462
#nullable enable
using System;
using System.Security.Cryptography;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    public partial class EcdsaKey : Key, IDisposable
    {
        private sealed class BclImpl : Impl
        {
            private readonly HashAlgorithmName _hashAlgorithmName;

            public BclImpl(string curve_oid, int cord_size, byte[] qx, byte[] qy, byte[]? privatekey)
            {
                var curve = ECCurve.CreateFromValue(curve_oid);
                var parameter = new ECParameters
                {
                    Curve = curve
                };

                parameter.Q.X = qx;
                parameter.Q.Y = qy;

                if (privatekey != null)
                {
                    parameter.D = privatekey.TrimLeadingZeros().Pad(cord_size);
                    PrivateKey = parameter.D;
                }

                Ecdsa = ECDsa.Create(parameter);

                _hashAlgorithmName = KeyLength switch
                {
                    <= 256 => HashAlgorithmName.SHA256,
                    <= 384 => HashAlgorithmName.SHA384,
                    _ => HashAlgorithmName.SHA512,
                };
            }

            public override byte[]? PrivateKey { get; }

            public override ECDsa Ecdsa { get; }

            public override int KeyLength
            {
                get
                {
                    return Ecdsa.KeySize;
                }
            }

            public override byte[] Sign(byte[] input)
            {
                return Ecdsa.SignData(input, _hashAlgorithmName);
            }

            public override bool Verify(byte[] input, byte[] signature)
            {
                return Ecdsa.VerifyData(input, signature, _hashAlgorithmName);
            }

            public override void Export(out byte[] qx, out byte[] qy)
            {
                var parameter = Ecdsa.ExportParameters(includePrivateParameters: false);
                qx = parameter.Q.X!;
                qy = parameter.Q.Y!;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Ecdsa.Dispose();
                }
            }
        }
    }
}
#endif
