using System;

using Renci.SshNet.Security.Org.BouncyCastle.Math;
using Renci.SshNet.Security.Org.BouncyCastle.Math.EC;

namespace Renci.SshNet.Security.Org.BouncyCastle.Asn1.X9
{
    internal class X9ECParameters
    {
        private byte[]		seed;

        public X9ECParameters(
            ECCurve     curve,
            X9ECPoint   g,
            BigInteger  n,
            BigInteger  h,
            byte[]      seed)
        {
            this.Curve = curve;
            this.BaseEntry = g;
            this.N = n;
            this.H = h;
            this.seed = seed;
        }

        public ECCurve Curve { get; private set; }

        public ECPoint G
        {
            get { return BaseEntry.Point; }
        }

        public BigInteger N { get; private set; }

        public BigInteger H { get; private set; }

        public byte[] GetSeed()
        {
            return seed;
        }

        public X9ECPoint BaseEntry { get; private set; }
    }
}
