using System;

using Renci.SshNet.Security.Org.BouncyCastle.Math;
using Renci.SshNet.Security.Org.BouncyCastle.Math.EC;

namespace Renci.SshNet.Security.Org.BouncyCastle.Asn1.X9
{
    internal class X9ECParameters
    {
        private byte[]		seed;

		public static X9ECParameters GetInstance(Object obj)
		{
			if (obj is X9ECParameters)
				return (X9ECParameters)obj;

            return null;
		}

        public X9ECParameters(
            ECCurve		curve,
            ECPoint		g,
            BigInteger	n)
            : this(curve, g, n, null, null)
        {
        }

        public X9ECParameters(
            ECCurve     curve,
            X9ECPoint   g,
            BigInteger  n,
            BigInteger  h)
            : this(curve, g, n, h, null)
        {
        }

        public X9ECParameters(
            ECCurve		curve,
            ECPoint		g,
            BigInteger	n,
            BigInteger	h)
            : this(curve, g, n, h, null)
        {
        }

        public X9ECParameters(
            ECCurve		curve,
            ECPoint		g,
            BigInteger	n,
            BigInteger	h,
            byte[]		seed)
            : this(curve, new X9ECPoint(g), n, h, seed)
        {
        }

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

        public X9Curve CurveEntry
        {
            get { return new X9Curve(Curve, seed); }
        }

        public X9ECPoint BaseEntry { get; private set; }
    }
}
