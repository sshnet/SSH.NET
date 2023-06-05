using System;

using Renci.SshNet.Security.Org.BouncyCastle.Math.EC.Multiplier;
using Renci.SshNet.Security.Org.BouncyCastle.Math.Field;

namespace Renci.SshNet.Security.Org.BouncyCastle.Math.EC
{
    internal class ECAlgorithms
    {
        public static bool IsFpCurve(ECCurve c)
        {
            return IsFpField(c.Field);
        }

        public static bool IsFpField(IFiniteField field)
        {
            return field.Dimension == 1;
        }

        public static ECPoint ImportPoint(ECCurve c, ECPoint p)
        {
            ECCurve cp = p.Curve;
            if (!c.Equals(cp))
                throw new ArgumentException("Point must be on the same curve");

            return c.ImportPoint(p);
        }

        public static void MontgomeryTrick(ECFieldElement[] zs, int off, int len, ECFieldElement scale)
        {
            /*
             * Uses the "Montgomery Trick" to invert many field elements, with only a single actual
             * field inversion. See e.g. the paper:
             * "Fast Multi-scalar Multiplication Methods on Elliptic Curves with Precomputation Strategy Using Montgomery Trick"
             * by Katsuyuki Okeya, Kouichi Sakurai.
             */

            ECFieldElement[] c = new ECFieldElement[len];
            c[0] = zs[off];

            int i = 0;
            while (++i < len)
            {
                c[i] = c[i - 1].Multiply(zs[off + i]);
            }

            --i;

            if (scale != null)
            {
                c[i] = c[i].Multiply(scale);
            }

            ECFieldElement u = c[i].Invert();

            while (i > 0)
            {
                int j = off + i--;
                ECFieldElement tmp = zs[j];
                zs[j] = c[i].Multiply(u);
                u = u.Multiply(tmp);
            }

            zs[off] = u;
        }

        /**
         * Simple shift-and-add multiplication. Serves as reference implementation
         * to verify (possibly faster) implementations, and for very small scalars.
         * 
         * @param p
         *            The point to multiply.
         * @param k
         *            The multiplier.
         * @return The result of the point multiplication <code>kP</code>.
         */
        public static ECPoint ReferenceMultiply(ECPoint p, BigInteger k)
        {
            BigInteger x = k.Abs();
            ECPoint q = p.Curve.Infinity;
            int t = x.BitLength;
            if (t > 0)
            {
                if (x.TestBit(0))
                {
                    q = p;
                }
                for (int i = 1; i < t; i++)
                {
                    p = p.Twice();
                    if (x.TestBit(i))
                    {
                        q = q.Add(p);
                    }
                }
            }
            return k.SignValue < 0 ? q.Negate() : q;
        }

        public static ECPoint CleanPoint(ECCurve c, ECPoint p)
        {
            ECCurve cp = p.Curve;
            if (!c.Equals(cp))
                throw new ArgumentException("Point must be on the same curve", "p");

            return c.DecodePoint(p.GetEncoded(false));
        }

        internal static ECPoint ImplCheckResult(ECPoint p)
        {
            if (!p.IsValidPartial())
                throw new InvalidOperationException("Invalid result");

            return p;
        }
    }
}
