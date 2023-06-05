using Renci.SshNet.Security.Org.BouncyCastle.Math.EC;
using Renci.SshNet.Security.Org.BouncyCastle.Utilities;

namespace Renci.SshNet.Security.Org.BouncyCastle.Asn1.X9
{
    internal class X9ECPoint
    {
        private readonly byte[] encoding;

        private ECCurve c;
        private ECPoint p;

        public X9ECPoint(ECCurve c, byte[] encoding)
        {
            this.c = c;
            this.encoding = Arrays.Clone(encoding);
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
    }
}
