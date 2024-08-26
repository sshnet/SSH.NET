#if !NET462
using System;
using System.Security.Cryptography;
using System.Text;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    public partial class EcdsaKey : Key, IDisposable
    {
        private void Import(string curve_oid, int cord_size, byte[] qx, byte[] qy, byte[] privatekey)
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

#if !NET
            // Mono doesn't implement ECDsa.Create()
            // See https://github.com/mono/mono/blob/main/mcs/class/referencesource/System.Core/System/Security/Cryptography/ECDsa.cs#L32
            try
            {
                Ecdsa = ECDsa.Create(parameter);
            }
            catch (NotImplementedException)
            {
                Import_BouncyCastle(curve_oid, qx, qy, privatekey);
            }
#else
            Ecdsa = ECDsa.Create(parameter);
#endif
        }

        private void Export(out byte[] curve, out byte[] qx, out byte[] qy)
        {
#if !NET
            if (PublicKeyParameters != null)
            {
                Export_BouncyCastle(out curve, out qx, out qy);
            }
            else
#endif
            {
                var parameter = Ecdsa.ExportParameters(includePrivateParameters: false);
                qx = parameter.Q.X;
                qy = parameter.Q.Y;
                switch (parameter.Curve.Oid.FriendlyName)
                {
                    case "ECDSA_P256":
                    case "nistP256":
                        curve = Encoding.ASCII.GetBytes("nistp256");
                        break;
                    case "ECDSA_P384":
                    case "nistP384":
                        curve = Encoding.ASCII.GetBytes("nistp384");
                        break;
                    case "ECDSA_P521":
                    case "nistP521":
                        curve = Encoding.ASCII.GetBytes("nistp521");
                        break;
                    default:
                        throw new SshException("Unexpected Curve Name: " + parameter.Curve.Oid.FriendlyName);
                }
            }
        }
    }
}
#endif
