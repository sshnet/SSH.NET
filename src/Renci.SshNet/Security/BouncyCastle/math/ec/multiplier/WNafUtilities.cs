using System;

using Renci.SshNet.Security.Org.BouncyCastle.Utilities;

namespace Renci.SshNet.Security.Org.BouncyCastle.Math.EC.Multiplier
{
    internal abstract class WNafUtilities
    {
        public static readonly string PRECOMP_NAME = "bc_wnaf";

        private static readonly int[] DEFAULT_WINDOW_SIZE_CUTOFFS = new int[]{ 13, 41, 121, 337, 897, 2305 };

        private static readonly ECPoint[] EMPTY_POINTS = new ECPoint[0];

        public static int[] GenerateCompactNaf(BigInteger k)
        {
            if ((k.BitLength >> 16) != 0)
                throw new ArgumentException("must have bitlength < 2^16", "k");
            if (k.SignValue == 0)
                return Arrays.EmptyInts;

            BigInteger _3k = k.ShiftLeft(1).Add(k);

            int bits = _3k.BitLength;
            int[] naf = new int[bits >> 1];

            BigInteger diff = _3k.Xor(k);

            int highBit = bits - 1, length = 0, zeroes = 0;
            for (int i = 1; i < highBit; ++i)
            {
                if (!diff.TestBit(i))
                {
                    ++zeroes;
                    continue;
                }

                int digit = k.TestBit(i) ? -1 : 1;
                naf[length++] = (digit << 16) | zeroes;
                zeroes = 1;
                ++i;
            }

            naf[length++] = (1 << 16) | zeroes;

            if (naf.Length > length)
            {
                naf = Trim(naf, length);
            }

            return naf;
        }

        public static int[] GenerateCompactWindowNaf(int width, BigInteger k)
        {
            if (width == 2)
            {
                return GenerateCompactNaf(k);
            }

            if (width < 2 || width > 16)
                throw new ArgumentException("must be in the range [2, 16]", "width");
            if ((k.BitLength >> 16) != 0)
                throw new ArgumentException("must have bitlength < 2^16", "k");
            if (k.SignValue == 0)
                return Arrays.EmptyInts;

            int[] wnaf = new int[k.BitLength / width + 1];

            // 2^width and a mask and sign bit set accordingly
            int pow2 = 1 << width;
            int mask = pow2 - 1;
            int sign = pow2 >> 1;

            bool carry = false;
            int length = 0, pos = 0;

            while (pos <= k.BitLength)
            {
                if (k.TestBit(pos) == carry)
                {
                    ++pos;
                    continue;
                }

                k = k.ShiftRight(pos);

                int digit = k.IntValue & mask;
                if (carry)
                {
                    ++digit;
                }

                carry = (digit & sign) != 0;
                if (carry)
                {
                    digit -= pow2;
                }

                int zeroes = length > 0 ? pos - 1 : pos;
                wnaf[length++] = (digit << 16) | zeroes;
                pos = width;
            }

            // Reduce the WNAF array to its actual length
            if (wnaf.Length > length)
            {
                wnaf = Trim(wnaf, length);
            }

            return wnaf;
        }

        public static byte[] GenerateJsf(BigInteger g, BigInteger h)
        {
            int digits = System.Math.Max(g.BitLength, h.BitLength) + 1;
            byte[] jsf = new byte[digits];

            BigInteger k0 = g, k1 = h;
            int j = 0, d0 = 0, d1 = 0;

            int offset = 0;
            while ((d0 | d1) != 0 || k0.BitLength > offset || k1.BitLength > offset)
            {
                int n0 = ((int)((uint)k0.IntValue >> offset) + d0) & 7;
                int n1 = ((int)((uint)k1.IntValue >> offset) + d1) & 7;

                int u0 = n0 & 1;
                if (u0 != 0)
                {
                    u0 -= (n0 & 2);
                    if ((n0 + u0) == 4 && (n1 & 3) == 2)
                    {
                        u0 = -u0;
                    }
                }

                int u1 = n1 & 1;
                if (u1 != 0)
                {
                    u1 -= (n1 & 2);
                    if ((n1 + u1) == 4 && (n0 & 3) == 2)
                    {
                        u1 = -u1;
                    }
                }

                if ((d0 << 1) == 1 + u0)
                {
                    d0 ^= 1;
                }
                if ((d1 << 1) == 1 + u1)
                {
                    d1 ^= 1;
                }

                if (++offset == 30)
                {
                    offset = 0;
                    k0 = k0.ShiftRight(30);
                    k1 = k1.ShiftRight(30);
                }

                jsf[j++] = (byte)((u0 << 4) | (u1 & 0xF));
            }

            // Reduce the JSF array to its actual length
            if (jsf.Length > j)
            {
                jsf = Trim(jsf, j);
            }

            return jsf;
        }

        public static byte[] GenerateNaf(BigInteger k)
        {
            if (k.SignValue == 0)
                return Arrays.EmptyBytes;

            BigInteger _3k = k.ShiftLeft(1).Add(k);

            int digits = _3k.BitLength - 1;
            byte[] naf = new byte[digits];

            BigInteger diff = _3k.Xor(k);

            for (int i = 1; i < digits; ++i)
            {
                if (diff.TestBit(i))
                {
                    naf[i - 1] = (byte)(k.TestBit(i) ? -1 : 1);
                    ++i;
                }
            }

            naf[digits - 1] = 1;

            return naf;
        }

        /**
         * Computes the Window NAF (non-adjacent Form) of an integer.
         * @param width The width <code>w</code> of the Window NAF. The width is
         * defined as the minimal number <code>w</code>, such that for any
         * <code>w</code> consecutive digits in the resulting representation, at
         * most one is non-zero.
         * @param k The integer of which the Window NAF is computed.
         * @return The Window NAF of the given width, such that the following holds:
         * <code>k = &amp;sum;<sub>i=0</sub><sup>l-1</sup> k<sub>i</sub>2<sup>i</sup>
         * </code>, where the <code>k<sub>i</sub></code> denote the elements of the
         * returned <code>byte[]</code>.
         */
        public static byte[] GenerateWindowNaf(int width, BigInteger k)
        {
            if (width == 2)
            {
                return GenerateNaf(k);
            }

            if (width < 2 || width > 8)
                throw new ArgumentException("must be in the range [2, 8]", "width");
            if (k.SignValue == 0)
                return Arrays.EmptyBytes;

            byte[] wnaf = new byte[k.BitLength + 1];

            // 2^width and a mask and sign bit set accordingly
            int pow2 = 1 << width;
            int mask = pow2 - 1;
            int sign = pow2 >> 1;

            bool carry = false;
            int length = 0, pos = 0;

            while (pos <= k.BitLength)
            {
                if (k.TestBit(pos) == carry)
                {
                    ++pos;
                    continue;
                }

                k = k.ShiftRight(pos);

                int digit = k.IntValue & mask;
                if (carry)
                {
                    ++digit;
                }

                carry = (digit & sign) != 0;
                if (carry)
                {
                    digit -= pow2;
                }

                length += (length > 0) ? pos - 1 : pos;
                wnaf[length++] = (byte)digit;
                pos = width;
            }

            // Reduce the WNAF array to its actual length
            if (wnaf.Length > length)
            {
                wnaf = Trim(wnaf, length);
            }
        
            return wnaf;
        }

        public static int GetNafWeight(BigInteger k)
        {
            if (k.SignValue == 0)
                return 0;

            BigInteger _3k = k.ShiftLeft(1).Add(k);
            BigInteger diff = _3k.Xor(k);

            return diff.BitCount;
        }

        public static WNafPreCompInfo GetWNafPreCompInfo(ECPoint p)
        {
            return GetWNafPreCompInfo(p.Curve.GetPreCompInfo(p, PRECOMP_NAME));
        }

        public static WNafPreCompInfo GetWNafPreCompInfo(PreCompInfo preCompInfo)
        {
            return preCompInfo as WNafPreCompInfo;
        }

        /**
         * Determine window width to use for a scalar multiplication of the given size.
         * 
         * @param bits the bit-length of the scalar to multiply by
         * @return the window size to use
         */
        public static int GetWindowSize(int bits)
        {
            return GetWindowSize(bits, DEFAULT_WINDOW_SIZE_CUTOFFS);
        }

        /**
         * Determine window width to use for a scalar multiplication of the given size.
         * 
         * @param bits the bit-length of the scalar to multiply by
         * @param windowSizeCutoffs a monotonically increasing list of bit sizes at which to increment the window width
         * @return the window size to use
         */
        public static int GetWindowSize(int bits, int[] windowSizeCutoffs)
        {
            int w = 0;
            for (; w < windowSizeCutoffs.Length; ++w)
            {
                if (bits < windowSizeCutoffs[w])
                {
                    break;
                }
            }
            return w + 2;
        }

        public static ECPoint MapPointWithPrecomp(ECPoint p, int width, bool includeNegated,
            ECPointMap pointMap)
        {
            ECCurve c = p.Curve;
            WNafPreCompInfo wnafPreCompP = Precompute(p, width, includeNegated);

            ECPoint q = pointMap.Map(p);
            c.Precompute(q, PRECOMP_NAME, new MapPointCallback(wnafPreCompP, includeNegated, pointMap));
            return q;
        }

        public static WNafPreCompInfo Precompute(ECPoint p, int width, bool includeNegated)
        {
            return (WNafPreCompInfo)p.Curve.Precompute(p, PRECOMP_NAME, new WNafCallback(p, width, includeNegated));
        }

        private static byte[] Trim(byte[] a, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(a, 0, result, 0, result.Length);
            return result;
        }

        private static int[] Trim(int[] a, int length)
        {
            int[] result = new int[length];
            Array.Copy(a, 0, result, 0, result.Length);
            return result;
        }

        private static ECPoint[] ResizeTable(ECPoint[] a, int length)
        {
            ECPoint[] result = new ECPoint[length];
            Array.Copy(a, 0, result, 0, a.Length);
            return result;
        }

        private class MapPointCallback
            : IPreCompCallback
        {
            private readonly WNafPreCompInfo m_wnafPreCompP;
            private readonly bool m_includeNegated;
            private readonly ECPointMap m_pointMap;

            internal MapPointCallback(WNafPreCompInfo wnafPreCompP, bool includeNegated, ECPointMap pointMap)
            {
                this.m_wnafPreCompP = wnafPreCompP;
                this.m_includeNegated = includeNegated;
                this.m_pointMap = pointMap;
            }

            public PreCompInfo Precompute(PreCompInfo existing)
            {
                WNafPreCompInfo result = new WNafPreCompInfo();

                ECPoint twiceP = m_wnafPreCompP.Twice;
                if (twiceP != null)
                {
                    ECPoint twiceQ = m_pointMap.Map(twiceP);
                    result.Twice = twiceQ;
                }

                ECPoint[] preCompP = m_wnafPreCompP.PreComp;
                ECPoint[] preCompQ = new ECPoint[preCompP.Length];
                for (int i = 0; i < preCompP.Length; ++i)
                {
                    preCompQ[i] = m_pointMap.Map(preCompP[i]);
                }
                result.PreComp = preCompQ;

                if (m_includeNegated)
                {
                    ECPoint[] preCompNegQ = new ECPoint[preCompQ.Length];
                    for (int i = 0; i < preCompNegQ.Length; ++i)
                    {
                        preCompNegQ[i] = preCompQ[i].Negate();
                    }
                    result.PreCompNeg = preCompNegQ;
                }

                return result;
            }
        }

        private class WNafCallback
            : IPreCompCallback
        {
            private readonly ECPoint m_p;
            private readonly int m_width;
            private readonly bool m_includeNegated;

            internal WNafCallback(ECPoint p, int width, bool includeNegated)
            {
                this.m_p = p;
                this.m_width = width;
                this.m_includeNegated = includeNegated;
            }

            public PreCompInfo Precompute(PreCompInfo existing)
            {
                WNafPreCompInfo existingWNaf = existing as WNafPreCompInfo;

                int reqPreCompLen = 1 << System.Math.Max(0, m_width - 2);

                if (CheckExisting(existingWNaf, reqPreCompLen, m_includeNegated))
                    return existingWNaf;

                ECCurve c = m_p.Curve;
                ECPoint[] preComp = null, preCompNeg = null;
                ECPoint twiceP = null;

                if (existingWNaf != null)
                {
                    preComp = existingWNaf.PreComp;
                    preCompNeg = existingWNaf.PreCompNeg;
                    twiceP = existingWNaf.Twice;
                }

                int iniPreCompLen = 0;
                if (preComp == null)
                {
                    preComp = EMPTY_POINTS;
                }
                else
                {
                    iniPreCompLen = preComp.Length;
                }

                if (iniPreCompLen < reqPreCompLen)
                {
                    preComp = WNafUtilities.ResizeTable(preComp, reqPreCompLen);

                    if (reqPreCompLen == 1)
                    {
                        preComp[0] = m_p.Normalize();
                    }
                    else
                    {
                        int curPreCompLen = iniPreCompLen;
                        if (curPreCompLen == 0)
                        {
                            preComp[0] = m_p;
                            curPreCompLen = 1;
                        }

                        ECFieldElement iso = null;

                        if (reqPreCompLen == 2)
                        {
                            preComp[1] = m_p.ThreeTimes();
                        }
                        else
                        {
                            ECPoint isoTwiceP = twiceP, last = preComp[curPreCompLen - 1];
                            if (isoTwiceP == null)
                            {
                                isoTwiceP = preComp[0].Twice();
                                twiceP = isoTwiceP;

                                /*
                                 * For Fp curves with Jacobian projective coordinates, use a (quasi-)isomorphism
                                 * where 'twiceP' is "affine", so that the subsequent additions are cheaper. This
                                 * also requires scaling the initial point's X, Y coordinates, and reversing the
                                 * isomorphism as part of the subsequent normalization.
                                 * 
                                 *  NOTE: The correctness of this optimization depends on:
                                 *      1) additions do not use the curve's A, B coefficients.
                                 *      2) no special cases (i.e. Q +/- Q) when calculating 1P, 3P, 5P, ...
                                 */
                                if (!twiceP.IsInfinity && ECAlgorithms.IsFpCurve(c) && c.FieldSize >= 64)
                                {
                                    switch (c.CoordinateSystem)
                                    {
                                    case ECCurve.COORD_JACOBIAN:
                                    case ECCurve.COORD_JACOBIAN_CHUDNOVSKY:
                                    case ECCurve.COORD_JACOBIAN_MODIFIED:
                                    {
                                        iso = twiceP.GetZCoord(0);
                                        isoTwiceP = c.CreatePoint(twiceP.XCoord.ToBigInteger(),
                                            twiceP.YCoord.ToBigInteger());

                                        ECFieldElement iso2 = iso.Square(), iso3 = iso2.Multiply(iso);
                                        last = last.ScaleX(iso2).ScaleY(iso3);

                                        if (iniPreCompLen == 0)
                                        {
                                            preComp[0] = last;
                                        }
                                        break;
                                    }
                                    }
                                }
                            }

                            while (curPreCompLen < reqPreCompLen)
                            {
                                /*
                                 * Compute the new ECPoints for the precomputation array. The values 1, 3,
                                 * 5, ..., 2^(width-1)-1 times p are computed
                                 */
                                preComp[curPreCompLen++] = last = last.Add(isoTwiceP);
                            }
                        }

                        /*
                         * Having oft-used operands in affine form makes operations faster.
                         */
                        c.NormalizeAll(preComp, iniPreCompLen, reqPreCompLen - iniPreCompLen, iso);
                    }
                }

                if (m_includeNegated)
                {
                    int pos;
                    if (preCompNeg == null)
                    {
                        pos = 0;
                        preCompNeg = new ECPoint[reqPreCompLen]; 
                    }
                    else
                    {
                        pos = preCompNeg.Length;
                        if (pos < reqPreCompLen)
                        {
                            preCompNeg = WNafUtilities.ResizeTable(preCompNeg, reqPreCompLen);
                        }
                    }

                    while (pos < reqPreCompLen)
                    {
                        preCompNeg[pos] = preComp[pos].Negate();
                        ++pos;
                    }
                }

                WNafPreCompInfo result = new WNafPreCompInfo();
                result.PreComp = preComp;
                result.PreCompNeg = preCompNeg;
                result.Twice = twiceP;
                return result;
            }

            private bool CheckExisting(WNafPreCompInfo existingWNaf, int reqPreCompLen, bool includeNegated)
            {
                return existingWNaf != null
                    && CheckTable(existingWNaf.PreComp, reqPreCompLen)
                    && (!includeNegated || CheckTable(existingWNaf.PreCompNeg, reqPreCompLen));
            }

            private bool CheckTable(ECPoint[] table, int reqLen)
            {
                return table != null && table.Length >= reqLen;
            }
        }
    }
}
