using System;
using System.Text;

using Renci.SshNet.Security.Org.BouncyCastle.Utilities;

namespace Renci.SshNet.Security.Org.BouncyCastle.Math.EC
{
    internal class LongArray
    {
        //private static long DEInterleave_MASK = 0x5555555555555555L;

        /*
         * This expands 8 bit indices into 16 bit contents (high bit 14), by inserting 0s between bits.
         * In a binary field, this operation is the same as squaring an 8 bit number.
         */
        private static readonly ushort[] INTERLEAVE2_TABLE = new ushort[]
        {
            0x0000, 0x0001, 0x0004, 0x0005, 0x0010, 0x0011, 0x0014, 0x0015,
            0x0040, 0x0041, 0x0044, 0x0045, 0x0050, 0x0051, 0x0054, 0x0055,
            0x0100, 0x0101, 0x0104, 0x0105, 0x0110, 0x0111, 0x0114, 0x0115,
            0x0140, 0x0141, 0x0144, 0x0145, 0x0150, 0x0151, 0x0154, 0x0155,
            0x0400, 0x0401, 0x0404, 0x0405, 0x0410, 0x0411, 0x0414, 0x0415,
            0x0440, 0x0441, 0x0444, 0x0445, 0x0450, 0x0451, 0x0454, 0x0455,
            0x0500, 0x0501, 0x0504, 0x0505, 0x0510, 0x0511, 0x0514, 0x0515,
            0x0540, 0x0541, 0x0544, 0x0545, 0x0550, 0x0551, 0x0554, 0x0555,
            0x1000, 0x1001, 0x1004, 0x1005, 0x1010, 0x1011, 0x1014, 0x1015,
            0x1040, 0x1041, 0x1044, 0x1045, 0x1050, 0x1051, 0x1054, 0x1055,
            0x1100, 0x1101, 0x1104, 0x1105, 0x1110, 0x1111, 0x1114, 0x1115,
            0x1140, 0x1141, 0x1144, 0x1145, 0x1150, 0x1151, 0x1154, 0x1155,
            0x1400, 0x1401, 0x1404, 0x1405, 0x1410, 0x1411, 0x1414, 0x1415,
            0x1440, 0x1441, 0x1444, 0x1445, 0x1450, 0x1451, 0x1454, 0x1455,
            0x1500, 0x1501, 0x1504, 0x1505, 0x1510, 0x1511, 0x1514, 0x1515,
            0x1540, 0x1541, 0x1544, 0x1545, 0x1550, 0x1551, 0x1554, 0x1555,
            0x4000, 0x4001, 0x4004, 0x4005, 0x4010, 0x4011, 0x4014, 0x4015,
            0x4040, 0x4041, 0x4044, 0x4045, 0x4050, 0x4051, 0x4054, 0x4055,
            0x4100, 0x4101, 0x4104, 0x4105, 0x4110, 0x4111, 0x4114, 0x4115,
            0x4140, 0x4141, 0x4144, 0x4145, 0x4150, 0x4151, 0x4154, 0x4155,
            0x4400, 0x4401, 0x4404, 0x4405, 0x4410, 0x4411, 0x4414, 0x4415,
            0x4440, 0x4441, 0x4444, 0x4445, 0x4450, 0x4451, 0x4454, 0x4455,
            0x4500, 0x4501, 0x4504, 0x4505, 0x4510, 0x4511, 0x4514, 0x4515,
            0x4540, 0x4541, 0x4544, 0x4545, 0x4550, 0x4551, 0x4554, 0x4555,
            0x5000, 0x5001, 0x5004, 0x5005, 0x5010, 0x5011, 0x5014, 0x5015,
            0x5040, 0x5041, 0x5044, 0x5045, 0x5050, 0x5051, 0x5054, 0x5055,
            0x5100, 0x5101, 0x5104, 0x5105, 0x5110, 0x5111, 0x5114, 0x5115,
            0x5140, 0x5141, 0x5144, 0x5145, 0x5150, 0x5151, 0x5154, 0x5155,
            0x5400, 0x5401, 0x5404, 0x5405, 0x5410, 0x5411, 0x5414, 0x5415,
            0x5440, 0x5441, 0x5444, 0x5445, 0x5450, 0x5451, 0x5454, 0x5455,
            0x5500, 0x5501, 0x5504, 0x5505, 0x5510, 0x5511, 0x5514, 0x5515,
            0x5540, 0x5541, 0x5544, 0x5545, 0x5550, 0x5551, 0x5554, 0x5555
        };

        // For toString(); must have length 64
        private const string ZEROES = "0000000000000000000000000000000000000000000000000000000000000000";

        internal static readonly byte[] BitLengths =
        {
            0, 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 4,
            5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
            6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
            6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8,
            8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8
        };

        // TODO make m fixed for the LongArray, and hence compute T once and for all

        private long[] m_ints;

        public LongArray(int intLen)
        {
            m_ints = new long[intLen];
        }

        public LongArray(long[] ints)
        {
            m_ints = ints;
        }

        public LongArray(long[] ints, int off, int len)
        {
            if (off == 0 && len == ints.Length)
            {
                m_ints = ints;
            }
            else
            {
                m_ints = new long[len];
                Array.Copy(ints, off, m_ints, 0, len);
            }
        }

        public LongArray(BigInteger bigInt)
        {
            if (bigInt == null || bigInt.SignValue < 0)
            {
                throw new ArgumentException("invalid F2m field value", "bigInt");
            }

            if (bigInt.SignValue == 0)
            {
                m_ints = new long[] { 0L };
                return;
            }

            byte[] barr = bigInt.ToByteArray();
            int barrLen = barr.Length;
            int barrStart = 0;
            if (barr[0] == 0)
            {
                // First byte is 0 to enforce highest (=sign) bit is zero.
                // In this case ignore barr[0].
                barrLen--;
                barrStart = 1;
            }
            int intLen = (barrLen + 7) / 8;
            m_ints = new long[intLen];

            int iarrJ = intLen - 1;
            int rem = barrLen % 8 + barrStart;
            long temp = 0;
            int barrI = barrStart;
            if (barrStart < rem)
            {
                for (; barrI < rem; barrI++)
                {
                    temp <<= 8;
                    uint barrBarrI = barr[barrI];
                    temp |= barrBarrI;
                }
                m_ints[iarrJ--] = temp;
            }

            for (; iarrJ >= 0; iarrJ--)
            {
                temp = 0;
                for (int i = 0; i < 8; i++)
                {
                    temp <<= 8;
                    uint barrBarrI = barr[barrI++];
                    temp |= barrBarrI;
                }
                m_ints[iarrJ] = temp;
            }
        }

        internal void CopyTo(long[] z, int zOff)
        {
            Array.Copy(m_ints, 0, z, zOff, m_ints.Length);
        }

        public bool IsOne()
        {
            long[] a = m_ints;
            if (a[0] != 1L)
            {
                return false;
            }
            for (int i = 1; i < a.Length; ++i)
            {
                if (a[i] != 0L)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsZero()
        {
            long[] a = m_ints;
            for (int i = 0; i < a.Length; ++i)
            {
                if (a[i] != 0L)
                {
                    return false;
                }
            }
            return true;
        }

        public int GetUsedLength()
        {
            return GetUsedLengthFrom(m_ints.Length);
        }

        public int GetUsedLengthFrom(int from)
        {
            long[] a = m_ints;
            from = System.Math.Min(from, a.Length);

            if (from < 1)
            {
                return 0;
            }

            // Check if first element will act as sentinel
            if (a[0] != 0)
            {
                while (a[--from] == 0)
                {
                }
                return from + 1;
            }

            do
            {
                if (a[--from] != 0)
                {
                    return from + 1;
                }
            }
            while (from > 0);

            return 0;
        }

        public int Degree()
        {
            int i = m_ints.Length;
            long w;
            do
            {
                if (i == 0)
                {
                    return 0;
                }
                w = m_ints[--i];
            }
            while (w == 0);

            return (i << 6) + BitLength(w);
        }

        private int DegreeFrom(int limit)
        {
            int i = (int)(((uint)limit + 62) >> 6);
            long w;
            do
            {
                if (i == 0)
                {
                    return 0;
                }
                w = m_ints[--i];
            }
            while (w == 0);

            return (i << 6) + BitLength(w);
        }

        private static int BitLength(long w)
        {
            int u = (int)((ulong)w >> 32), b;
            if (u == 0)
            {
                u = (int)w;
                b = 0;
            }
            else
            {
                b = 32;
            }

            int t = (int)((uint)u >> 16), k;
            if (t == 0)
            {
                t = (int)((uint)u >> 8);
                k = (t == 0) ? BitLengths[u] : 8 + BitLengths[t];
            }
            else
            {
                int v = (int)((uint)t >> 8);
                k = (v == 0) ? 16 + BitLengths[t] : 24 + BitLengths[v];
            }

            return b + k;
        }

        private long[] ResizedInts(int newLen)
        {
            long[] newInts = new long[newLen];
            Array.Copy(m_ints, 0, newInts, 0, System.Math.Min(m_ints.Length, newLen));
            return newInts;
        }

        public BigInteger ToBigInteger()
        {
            int usedLen = GetUsedLength();
            if (usedLen == 0)
            {
                return BigInteger.Zero;
            }

            long highestInt = m_ints[usedLen - 1];
            byte[] temp = new byte[8];
            int barrI = 0;
            bool trailingZeroBytesDone = false;
            for (int j = 7; j >= 0; j--)
            {
                byte thisByte = (byte)((ulong)highestInt >> (8 * j));
                if (trailingZeroBytesDone || (thisByte != 0))
                {
                    trailingZeroBytesDone = true;
                    temp[barrI++] = thisByte;
                }
            }

            int barrLen = 8 * (usedLen - 1) + barrI;
            byte[] barr = new byte[barrLen];
            for (int j = 0; j < barrI; j++)
            {
                barr[j] = temp[j];
            }
            // Highest value int is done now

            for (int iarrJ = usedLen - 2; iarrJ >= 0; iarrJ--)
            {
                long mi = m_ints[iarrJ];
                for (int j = 7; j >= 0; j--)
                {
                    barr[barrI++] = (byte)((ulong)mi >> (8 * j));
                }
            }
            return new BigInteger(1, barr);
        }

        private static long ShiftUp(long[] x, int xOff, long[] z, int zOff, int count, int shift)
        {
            int shiftInv = 64 - shift;
            long prev = 0;
            for (int i = 0; i < count; ++i)
            {
                long next = x[xOff + i];
                z[zOff + i] = (next << shift) | prev;
                prev = (long)((ulong)next >> shiftInv);
            }
            return prev;
        }

        public LongArray AddOne()
        {
            if (m_ints.Length == 0)
            {
                return new LongArray(new long[]{ 1L });
            }

            int resultLen = System.Math.Max(1, GetUsedLength());
            long[] ints = ResizedInts(resultLen);
            ints[0] ^= 1L;
            return new LongArray(ints);
        }

        private void AddShiftedByBitsSafe(LongArray other, int otherDegree, int bits)
        {
            int otherLen = (int)((uint)(otherDegree + 63) >> 6);

            int words = (int)((uint)bits >> 6);
            int shift = bits & 0x3F;

            if (shift == 0)
            {
                Add(m_ints, words, other.m_ints, 0, otherLen);
                return;
            }

            long carry = AddShiftedUp(m_ints, words, other.m_ints, 0, otherLen, shift);
            if (carry != 0L)
            {
                m_ints[otherLen + words] ^= carry;
            }
        }

        private static long AddShiftedUp(long[] x, int xOff, long[] y, int yOff, int count, int shift)
        {
            int shiftInv = 64 - shift;
            long prev = 0;
            for (int i = 0; i < count; ++i)
            {
                long next = y[yOff + i];
                x[xOff + i] ^= (next << shift) | prev;
                prev = (long)((ulong)next >> shiftInv);
            }
            return prev;
        }

        private static long AddShiftedDown(long[] x, int xOff, long[] y, int yOff, int count, int shift)
        {
            int shiftInv = 64 - shift;
            long prev = 0;
            int i = count;
            while (--i >= 0)
            {
                long next = y[yOff + i];
                x[xOff + i] ^= (long)((ulong)next >> shift) | prev;
                prev = next << shiftInv;
            }
            return prev;
        }

        public void AddShiftedByWords(LongArray other, int words)
        {
            int otherUsedLen = other.GetUsedLength();
            if (otherUsedLen == 0)
            {
                return;
            }

            int minLen = otherUsedLen + words;
            if (minLen > m_ints.Length)
            {
                m_ints = ResizedInts(minLen);
            }

            Add(m_ints, words, other.m_ints, 0, otherUsedLen);
        }

        private static void Add(long[] x, int xOff, long[] y, int yOff, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                x[xOff + i] ^= y[yOff + i];
            }
        }

        private static void Add(long[] x, int xOff, long[] y, int yOff, long[] z, int zOff, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                z[zOff + i] = x[xOff + i] ^ y[yOff + i];
            }
        }

        private static void AddBoth(long[] x, int xOff, long[] y1, int y1Off, long[] y2, int y2Off, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                x[xOff + i] ^= y1[y1Off + i] ^ y2[y2Off + i];
            }
        }

        private static void FlipWord(long[] buf, int off, int bit, long word)
        {
            int n = off + (int)((uint)bit >> 6);
            int shift = bit & 0x3F;
            if (shift == 0)
            {
                buf[n] ^= word;
            }
            else
            {
                buf[n] ^= word << shift;
                word = (long)((ulong)word >> (64 - shift));
                if (word != 0)
                {
                    buf[++n] ^= word;
                }
            }
        }

        public bool TestBitZero()
        {
            return m_ints.Length > 0 && (m_ints[0] & 1L) != 0;
        }

        private static bool TestBit(long[] buf, int off, int n)
        {
            // theInt = n / 64
            int theInt = (int)((uint)n >> 6);
            // theBit = n % 64
            int theBit = n & 0x3F;
            long tester = 1L << theBit;
            return (buf[off + theInt] & tester) != 0;
        }

        private static void FlipBit(long[] buf, int off, int n)
        {
            // theInt = n / 64
            int theInt = (int)((uint)n >> 6);
            // theBit = n % 64
            int theBit = n & 0x3F;
            long flipper = 1L << theBit;
            buf[off + theInt] ^= flipper;
        }

        private static void MultiplyWord(long a, long[] b, int bLen, long[] c, int cOff)
        {
            if ((a & 1L) != 0L)
            {
                Add(c, cOff, b, 0, bLen);
            }
            int k = 1;
            while ((a = (long)((ulong)a >> 1)) != 0L)
            {
                if ((a & 1L) != 0L)
                {
                    long carry = AddShiftedUp(c, cOff, b, 0, bLen, k);
                    if (carry != 0L)
                    {
                        c[cOff + bLen] ^= carry;
                    }
                }
                ++k;
            }
        }

        public LongArray ModMultiply(LongArray other, int m, int[] ks)
        {
            /*
             * Find out the degree of each argument and handle the zero cases
             */
            int aDeg = Degree();
            if (aDeg == 0)
            {
                return this;
            }
            int bDeg = other.Degree();
            if (bDeg == 0)
            {
                return other;
            }

            /*
             * Swap if necessary so that A is the smaller argument
             */
            LongArray A = this, B = other;
            if (aDeg > bDeg)
            {
                A = other; B = this;
                int tmp = aDeg; aDeg = bDeg; bDeg = tmp;
            }

            /*
             * Establish the word lengths of the arguments and result
             */
            int aLen = (int)((uint)(aDeg + 63) >> 6);
            int bLen = (int)((uint)(bDeg + 63) >> 6);
            int cLen = (int)((uint)(aDeg + bDeg + 62) >> 6);

            if (aLen == 1)
            {
                long a0 = A.m_ints[0];
                if (a0 == 1L)
                {
                    return B;
                }

                /*
                 * Fast path for small A, with performance dependent only on the number of set bits
                 */
                long[] c0 = new long[cLen];
                MultiplyWord(a0, B.m_ints, bLen, c0, 0);

                /*
                 * Reduce the raw answer against the reduction coefficients
                 */
                return ReduceResult(c0, 0, cLen, m, ks);
            }

            /*
             * Determine if B will get bigger during shifting
             */
            int bMax = (int)((uint)(bDeg + 7 + 63) >> 6);

            /*
             * Lookup table for the offset of each B in the tables
             */
            int[] ti = new int[16];

            /*
             * Precompute table of all 4-bit products of B
             */
            long[] T0 = new long[bMax << 4];
            int tOff = bMax;
            ti[1] = tOff;
            Array.Copy(B.m_ints, 0, T0, tOff, bLen);
            for (int i = 2; i < 16; ++i)
            {
                ti[i] = (tOff += bMax);
                if ((i & 1) == 0)
                {
                    ShiftUp(T0, (int)((uint)tOff >> 1), T0, tOff, bMax, 1);
                }
                else
                {
                    Add(T0, bMax, T0, tOff - bMax, T0, tOff, bMax);
                }
            }

            /*
             * Second table with all 4-bit products of B shifted 4 bits
             */
            long[] T1 = new long[T0.Length];
            ShiftUp(T0, 0, T1, 0, T0.Length, 4);
    //        ShiftUp(T0, bMax, T1, bMax, tOff, 4);

            long[] a = A.m_ints;
            long[] c = new long[cLen << 3];

            int MASK = 0xF;

            /*
             * Lopez-Dahab (Modified) algorithm
             */

            for (int aPos = 0; aPos < aLen; ++aPos)
            {
                long aVal = a[aPos];
                int cOff = aPos;
                for (;;)
                {
                    int u = (int)aVal & MASK;
                    aVal = (long)((ulong)aVal >> 4);
                    int v = (int)aVal & MASK;
                    AddBoth(c, cOff, T0, ti[u], T1, ti[v], bMax);
                    aVal = (long)((ulong)aVal >> 4);
                    if (aVal == 0L)
                    {
                        break;
                    }
                    cOff += cLen;
                }
            }

            {
                int cOff = c.Length;
                while ((cOff -= cLen) != 0)
                {
                    AddShiftedUp(c, cOff - cLen, c, cOff, cLen, 8);
                }
            }

            /*
             * Finally the raw answer is collected, reduce it against the reduction coefficients
             */
            return ReduceResult(c, 0, cLen, m, ks);
        }

        public LongArray Multiply(LongArray other, int m, int[] ks)
        {
            /*
             * Find out the degree of each argument and handle the zero cases
             */
            int aDeg = Degree();
            if (aDeg == 0)
            {
                return this;
            }
            int bDeg = other.Degree();
            if (bDeg == 0)
            {
                return other;
            }

            /*
             * Swap if necessary so that A is the smaller argument
             */
            LongArray A = this, B = other;
            if (aDeg > bDeg)
            {
                A = other; B = this;
                int tmp = aDeg; aDeg = bDeg; bDeg = tmp;
            }

            /*
             * Establish the word lengths of the arguments and result
             */
            int aLen = (int)((uint)(aDeg + 63) >> 6);
            int bLen = (int)((uint)(bDeg + 63) >> 6);
            int cLen = (int)((uint)(aDeg + bDeg + 62) >> 6);

            if (aLen == 1)
            {
                long a0 = A.m_ints[0];
                if (a0 == 1L)
                {
                    return B;
                }

                /*
                 * Fast path for small A, with performance dependent only on the number of set bits
                 */
                long[] c0 = new long[cLen];
                MultiplyWord(a0, B.m_ints, bLen, c0, 0);

                /*
                 * Reduce the raw answer against the reduction coefficients
                 */
                //return ReduceResult(c0, 0, cLen, m, ks);
                return new LongArray(c0, 0, cLen);
            }

            /*
             * Determine if B will get bigger during shifting
             */
            int bMax = (int)((uint)(bDeg + 7 + 63) >> 6);

            /*
             * Lookup table for the offset of each B in the tables
             */
            int[] ti = new int[16];

            /*
             * Precompute table of all 4-bit products of B
             */
            long[] T0 = new long[bMax << 4];
            int tOff = bMax;
            ti[1] = tOff;
            Array.Copy(B.m_ints, 0, T0, tOff, bLen);
            for (int i = 2; i < 16; ++i)
            {
                ti[i] = (tOff += bMax);
                if ((i & 1) == 0)
                {
                    ShiftUp(T0, (int)((uint)tOff >> 1), T0, tOff, bMax, 1);
                }
                else
                {
                    Add(T0, bMax, T0, tOff - bMax, T0, tOff, bMax);
                }
            }

            /*
             * Second table with all 4-bit products of B shifted 4 bits
             */
            long[] T1 = new long[T0.Length];
            ShiftUp(T0, 0, T1, 0, T0.Length, 4);
            //        ShiftUp(T0, bMax, T1, bMax, tOff, 4);

            long[] a = A.m_ints;
            long[] c = new long[cLen << 3];

            int MASK = 0xF;

            /*
             * Lopez-Dahab (Modified) algorithm
             */

            for (int aPos = 0; aPos < aLen; ++aPos)
            {
                long aVal = a[aPos];
                int cOff = aPos;
                for (; ; )
                {
                    int u = (int)aVal & MASK;
                    aVal = (long)((ulong)aVal >> 4);
                    int v = (int)aVal & MASK;
                    AddBoth(c, cOff, T0, ti[u], T1, ti[v], bMax);
                    aVal = (long)((ulong)aVal >> 4);
                    if (aVal == 0L)
                    {
                        break;
                    }
                    cOff += cLen;
                }
            }

            {
                int cOff = c.Length;
                while ((cOff -= cLen) != 0)
                {
                    AddShiftedUp(c, cOff - cLen, c, cOff, cLen, 8);
                }
            }

            /*
             * Finally the raw answer is collected, reduce it against the reduction coefficients
             */
            //return ReduceResult(c, 0, cLen, m, ks);
            return new LongArray(c, 0, cLen);
        }

        public void Reduce(int m, int[] ks)
        {
            long[] buf = m_ints;
            int rLen = ReduceInPlace(buf, 0, buf.Length, m, ks);
            if (rLen < buf.Length)
            {
                m_ints = new long[rLen];
                Array.Copy(buf, 0, m_ints, 0, rLen);
            }
        }

        private static LongArray ReduceResult(long[] buf, int off, int len, int m, int[] ks)
        {
            int rLen = ReduceInPlace(buf, off, len, m, ks);
            return new LongArray(buf, off, rLen);
        }

        private static int ReduceInPlace(long[] buf, int off, int len, int m, int[] ks)
        {
            int mLen = (m + 63) >> 6;
            if (len < mLen)
            {
                return len;
            }

            int numBits = System.Math.Min(len << 6, (m << 1) - 1); // TODO use actual degree?
            int excessBits = (len << 6) - numBits;
            while (excessBits >= 64)
            {
                --len;
                excessBits -= 64;
            }

            int kLen = ks.Length, kMax = ks[kLen - 1], kNext = kLen > 1 ? ks[kLen - 2] : 0;
            int wordWiseLimit = System.Math.Max(m, kMax + 64);
            int vectorableWords = (excessBits + System.Math.Min(numBits - wordWiseLimit, m - kNext)) >> 6;
            if (vectorableWords > 1)
            {
                int vectorWiseWords = len - vectorableWords;
                ReduceVectorWise(buf, off, len, vectorWiseWords, m, ks);
                while (len > vectorWiseWords)
                {
                    buf[off + --len] = 0L;
                }
                numBits = vectorWiseWords << 6;
            }

            if (numBits > wordWiseLimit)
            {
                ReduceWordWise(buf, off, len, wordWiseLimit, m, ks);
                numBits = wordWiseLimit;
            }

            if (numBits > m)
            {
                ReduceBitWise(buf, off, numBits, m, ks);
            }

            return mLen;
        }

        private static void ReduceBitWise(long[] buf, int off, int BitLength, int m, int[] ks)
        {
            while (--BitLength >= m)
            {
                if (TestBit(buf, off, BitLength))
                {
                    ReduceBit(buf, off, BitLength, m, ks);
                }
            }
        }

        private static void ReduceBit(long[] buf, int off, int bit, int m, int[] ks)
        {
            FlipBit(buf, off, bit);
            int n = bit - m;
            int j = ks.Length;
            while (--j >= 0)
            {
                FlipBit(buf, off, ks[j] + n);
            }
            FlipBit(buf, off, n);
        }

        private static void ReduceWordWise(long[] buf, int off, int len, int toBit, int m, int[] ks)
        {
            int toPos = (int)((uint)toBit >> 6);

            while (--len > toPos)
            {
                long word = buf[off + len];
                if (word != 0)
                {
                    buf[off + len] = 0;
                    ReduceWord(buf, off, (len << 6), word, m, ks);
                }
            }

            {
                int partial = toBit & 0x3F;
                long word = (long)((ulong)buf[off + toPos] >> partial);
                if (word != 0)
                {
                    buf[off + toPos] ^= word << partial;
                    ReduceWord(buf, off, toBit, word, m, ks);
                }
            }
        }

        private static void ReduceWord(long[] buf, int off, int bit, long word, int m, int[] ks)
        {
            int offset = bit - m;
            int j = ks.Length;
            while (--j >= 0)
            {
                FlipWord(buf, off, offset + ks[j], word);
            }
            FlipWord(buf, off, offset, word);
        }

        private static void ReduceVectorWise(long[] buf, int off, int len, int words, int m, int[] ks)
        {
            /*
             * NOTE: It's important we go from highest coefficient to lowest, because for the highest
             * one (only) we allow the ranges to partially overlap, and therefore any changes must take
             * effect for the subsequent lower coefficients.
             */
            int baseBit = (words << 6) - m;
            int j = ks.Length;
            while (--j >= 0)
            {
                FlipVector(buf, off, buf, off + words, len - words, baseBit + ks[j]);
            }
            FlipVector(buf, off, buf, off + words, len - words, baseBit);
        }

        private static void FlipVector(long[] x, int xOff, long[] y, int yOff, int yLen, int bits)
        {
            xOff += (int)((uint)bits >> 6);
            bits &= 0x3F;

            if (bits == 0)
            {
                Add(x, xOff, y, yOff, yLen);
            }
            else
            {
                long carry = AddShiftedDown(x, xOff + 1, y, yOff, yLen, 64 - bits);
                x[xOff] ^= carry;
            }
        }

        public LongArray ModSquare(int m, int[] ks)
        {
            int len = GetUsedLength();
            if (len == 0)
            {
                return this;
            }

            int _2len = len << 1;
            long[] r = new long[_2len];

            int pos = 0;
            while (pos < _2len)
            {
                long mi = m_ints[(uint)pos >> 1];
                r[pos++] = Interleave2_32to64((int)mi);
                r[pos++] = Interleave2_32to64((int)((ulong)mi >> 32));
            }

            return new LongArray(r, 0, ReduceInPlace(r, 0, r.Length, m, ks));
        }

        public LongArray ModSquareN(int n, int m, int[] ks)
        {
            int len = GetUsedLength();
            if (len == 0)
            {
                return this;
            }
    
            int mLen = (m + 63) >> 6;
            long[] r = new long[mLen << 1];
            Array.Copy(m_ints, 0, r, 0, len);
    
            while (--n >= 0)
            {
                SquareInPlace(r, len, m, ks);
                len = ReduceInPlace(r, 0, r.Length, m, ks);
            }
    
            return new LongArray(r, 0, len);
        }

        public LongArray Square(int m, int[] ks)
        {
            int len = GetUsedLength();
            if (len == 0)
            {
                return this;
            }

            int _2len = len << 1;
            long[] r = new long[_2len];

            int pos = 0;
            while (pos < _2len)
            {
                long mi = m_ints[(uint)pos >> 1];
                r[pos++] = Interleave2_32to64((int)mi);
                r[pos++] = Interleave2_32to64((int)((ulong)mi >> 32));
            }

            return new LongArray(r, 0, r.Length);
        }

        private static void SquareInPlace(long[] x, int xLen, int m, int[] ks)
        {
            int pos = xLen << 1;
            while (--xLen >= 0)
            {
                long xVal = x[xLen];
                x[--pos] = Interleave2_32to64((int)((ulong)xVal >> 32));
                x[--pos] = Interleave2_32to64((int)xVal);
            }
        }

        private static long Interleave2_32to64(int x)
        {
            int r00 = INTERLEAVE2_TABLE[x & 0xFF] | INTERLEAVE2_TABLE[((uint)x >> 8) & 0xFF] << 16;
            int r32 = INTERLEAVE2_TABLE[((uint)x >> 16) & 0xFF] | INTERLEAVE2_TABLE[(uint)x >> 24] << 16;
            return (r32 & 0xFFFFFFFFL) << 32 | (r00 & 0xFFFFFFFFL);
        }

        public LongArray ModInverse(int m, int[] ks)
        {
            /*
             * Inversion in F2m using the extended Euclidean algorithm
             * 
             * Input: A nonzero polynomial a(z) of degree at most m-1
             * Output: a(z)^(-1) mod f(z)
             */
            int uzDegree = Degree();
            if (uzDegree == 0)
            {
                throw new InvalidOperationException();
            }
            if (uzDegree == 1)
            {
                return this;
            }

            // u(z) := a(z)
            LongArray uz = (LongArray)Copy();

            int t = (m + 63) >> 6;

            // v(z) := f(z)
            LongArray vz = new LongArray(t);
            ReduceBit(vz.m_ints, 0, m, m, ks);

            // g1(z) := 1, g2(z) := 0
            LongArray g1z = new LongArray(t);
            g1z.m_ints[0] = 1L;
            LongArray g2z = new LongArray(t);

            int[] uvDeg = new int[]{ uzDegree, m + 1 };
            LongArray[] uv = new LongArray[]{ uz, vz };

            int[] ggDeg = new int[]{ 1, 0 };
            LongArray[] gg = new LongArray[]{ g1z, g2z };

            int b = 1;
            int duv1 = uvDeg[b];
            int dgg1 = ggDeg[b];
            int j = duv1 - uvDeg[1 - b];

            for (;;)
            {
                if (j < 0)
                {
                    j = -j;
                    uvDeg[b] = duv1;
                    ggDeg[b] = dgg1;
                    b = 1 - b;
                    duv1 = uvDeg[b];
                    dgg1 = ggDeg[b];
                }

                uv[b].AddShiftedByBitsSafe(uv[1 - b], uvDeg[1 - b], j);

                int duv2 = uv[b].DegreeFrom(duv1);
                if (duv2 == 0)
                {
                    return gg[1 - b];
                }

                {
                    int dgg2 = ggDeg[1 - b];
                    gg[b].AddShiftedByBitsSafe(gg[1 - b], dgg2, j);
                    dgg2 += j;

                    if (dgg2 > dgg1)
                    {
                        dgg1 = dgg2;
                    }
                    else if (dgg2 == dgg1)
                    {
                        dgg1 = gg[b].DegreeFrom(dgg1);
                    }
                }

                j += (duv2 - duv1);
                duv1 = duv2;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as LongArray);
        }

        public virtual bool Equals(LongArray other)
        {
            if (this == other)
                return true;
            if (null == other)
                return false;
            int usedLen = GetUsedLength();
            if (other.GetUsedLength() != usedLen)
            {
                return false;
            }
            for (int i = 0; i < usedLen; i++)
            {
                if (m_ints[i] != other.m_ints[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int usedLen = GetUsedLength();
            int hash = 1;
            for (int i = 0; i < usedLen; i++)
            {
                long mi = m_ints[i];
                hash *= 31;
                hash ^= (int)mi;
                hash *= 31;
                hash ^= (int)((ulong)mi >> 32);
            }
            return hash;
        }

        public LongArray Copy()
        {
            return new LongArray(Arrays.Clone(m_ints));
        }

        public override string ToString()
        {
            int i = GetUsedLength();
            if (i == 0)
            {
                return "0";
            }

            StringBuilder sb = new StringBuilder(Convert.ToString(m_ints[--i], 2));
            while (--i >= 0)
            {
                string s = Convert.ToString(m_ints[i], 2);

                // Add leading zeroes, except for highest significant word
                int len = s.Length;
                if (len < 64)
                {
                    sb.Append(ZEROES.Substring(len));
                }

                sb.Append(s);
            }
            return sb.ToString();
        }
    }
}
