#if !NET462
using System;
using System.Security.Cryptography;
using System.Text;

using Renci.SshNet.Common;

namespace Renci.SshNet.Security
{
    public partial class EcdsaKey : Key, IDisposable
    {
        /// <summary>
        /// Gets the HashAlgorithm to use.
        /// </summary>
        public HashAlgorithmName HashAlgorithm
        {
            get
            {
                switch (KeyLength)
                {
                    case 256:
                        return HashAlgorithmName.SHA256;
                    case 384:
                        return HashAlgorithmName.SHA384;
                    case 521:
                        return HashAlgorithmName.SHA512;
                    default:
                        return HashAlgorithmName.SHA256;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ECDsa"/> object.
        /// </summary>
        public ECDsa Ecdsa { get; private set; }

        private void Import_Bcl(string curve_oid, int cord_size, byte[] qx, byte[] qy, byte[] privatekey)
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

        private void Export_Bcl(out byte[] curve, out byte[] qx, out byte[] qy)
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
