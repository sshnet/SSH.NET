using Renci.SshNet.Security.Org.BouncyCastle.Math.EC;
using Renci.SshNet.Security.Org.BouncyCastle.Utilities;

namespace Renci.SshNet.Security.Org.BouncyCastle.Asn1.X9
{
    internal class X9ECPoint
    {
        private readonly byte[] encoding;

        private ECCurve c;
        private ECPoint p;

        public X9ECPoint(ECPoint p)
            : this(p, false)
        {
        }

        public X9ECPoint(ECPoint p, bool compressed)
        {
            this.p = p.Normalize();
            this.encoding = p.GetEncoded(compressed);
        }

        public X9ECPoint(ECCurve c, byte[] encoding)
        {
            this.c = c;
            this.encoding = Arrays.Clone(encoding);
        }

        public byte[] GetPointEncoding()
        {
            return Arrays.Clone(encoding);
        }

        public ECPoint Point
        {
            get
            {
                if (p == null)
                {
                    p = c.DecodePoint(encoding).Normalize();
                }

                return p;
            }
        }

        public bool IsPointCompressed
        {
            get
            {
                byte[] octets = encoding;
                return octets != null && octets.Length > 0 && (octets[0] == 2 || octets[0] == 3);
            }
        }
    }
}