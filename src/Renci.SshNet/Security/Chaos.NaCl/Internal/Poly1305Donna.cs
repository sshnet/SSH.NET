using System;
using System.Collections.Generic;

namespace Renci.SshNet.Security.Chaos.NaCl.Internal
{
    internal class Poly1305Donna
    {
        // written by floodyberry (Andrew M.)
        // original license: MIT or PUBLIC DOMAIN
        // https://github.com/floodyberry/poly1305-donna/blob/master/poly1305-donna-unrolled.c
        internal static void poly1305_auth(byte[] output, int outputOffset, byte[] m, int mStart, int mLength, ref Array8<UInt32> key)
        {
            UInt32 t0, t1, t2, t3;
            UInt32 h0, h1, h2, h3, h4;
            UInt32 r0, r1, r2, r3, r4;
            UInt32 s1, s2, s3, s4;
            UInt32 b, nb;
            int j;
            UInt64 tt0, tt1, tt2, tt3, tt4;
            UInt64 f0, f1, f2, f3;
            UInt32 g0, g1, g2, g3, g4;
            UInt64 c;

            /* clamp key */
            t0 = key.x0;
            t1 = key.x1;
            t2 = key.x2;
            t3 = key.x3;

            /* precompute multipliers */
            r0 = t0 & 0x3ffffff; t0 >>= 26; t0 |= t1 << 6;
            r1 = t0 & 0x3ffff03; t1 >>= 20; t1 |= t2 << 12;
            r2 = t1 & 0x3ffc0ff; t2 >>= 14; t2 |= t3 << 18;
            r3 = t2 & 0x3f03fff; t3 >>= 8;
            r4 = t3 & 0x00fffff;

            s1 = r1 * 5;
            s2 = r2 * 5;
            s3 = r3 * 5;
            s4 = r4 * 5;

            /* init state */
            h0 = 0;
            h1 = 0;
            h2 = 0;
            h3 = 0;
            h4 = 0;

            /* full blocks */
            if (mLength < 16)
                goto poly1305_donna_atmost15bytes;

        poly1305_donna_16bytes:
            mStart += 16;
            mLength -= 16;

            t0 = ByteIntegerConverter.LoadLittleEndian32(m, mStart - 16);
            t1 = ByteIntegerConverter.LoadLittleEndian32(m, mStart - 12);
            t2 = ByteIntegerConverter.LoadLittleEndian32(m, mStart - 8);
            t3 = ByteIntegerConverter.LoadLittleEndian32(m, mStart - 4);

            //todo: looks like these can be simplified a bit
            h0 += t0 & 0x3ffffff;
            h1 += (uint)(((((UInt64)t1 << 32) | t0) >> 26) & 0x3ffffff);
            h2 += (uint)(((((UInt64)t2 << 32) | t1) >> 20) & 0x3ffffff);
            h3 += (uint)(((((UInt64)t3 << 32) | t2) >> 14) & 0x3ffffff);
            h4 += (t3 >> 8) | (1 << 24);


        poly1305_donna_mul:
            tt0 = (ulong)h0 * r0 + (ulong)h1 * s4 + (ulong)h2 * s3 + (ulong)h3 * s2 + (ulong)h4 * s1;
            tt1 = (ulong)h0 * r1 + (ulong)h1 * r0 + (ulong)h2 * s4 + (ulong)h3 * s3 + (ulong)h4 * s2;
            tt2 = (ulong)h0 * r2 + (ulong)h1 * r1 + (ulong)h2 * r0 + (ulong)h3 * s4 + (ulong)h4 * s3;
            tt3 = (ulong)h0 * r3 + (ulong)h1 * r2 + (ulong)h2 * r1 + (ulong)h3 * r0 + (ulong)h4 * s4;
            tt4 = (ulong)h0 * r4 + (ulong)h1 * r3 + (ulong)h2 * r2 + (ulong)h3 * r1 + (ulong)h4 * r0;

            unchecked
            {
                h0 = (UInt32)tt0 & 0x3ffffff; c = (tt0 >> 26);
                tt1 += c; h1 = (UInt32)tt1 & 0x3ffffff; b = (UInt32)(tt1 >> 26);
                tt2 += b; h2 = (UInt32)tt2 & 0x3ffffff; b = (UInt32)(tt2 >> 26);
                tt3 += b; h3 = (UInt32)tt3 & 0x3ffffff; b = (UInt32)(tt3 >> 26);
                tt4 += b; h4 = (UInt32)tt4 & 0x3ffffff; b = (UInt32)(tt4 >> 26);
            }
            h0 += b * 5;

            if (mLength >= 16)
                goto poly1305_donna_16bytes;

    /* final bytes */
        poly1305_donna_atmost15bytes:
            if (mLength == 0)
                goto poly1305_donna_finish;

            byte[] mp = new byte[16];//todo remove allocation

            for (j = 0; j < mLength; j++)
                mp[j] = m[mStart + j];
            mp[j++] = 1;
            for (; j < 16; j++)
                mp[j] = 0;
            mLength = 0;

            t0 = ByteIntegerConverter.LoadLittleEndian32(mp, 0);
            t1 = ByteIntegerConverter.LoadLittleEndian32(mp, 4);
            t2 = ByteIntegerConverter.LoadLittleEndian32(mp, 8);
            t3 = ByteIntegerConverter.LoadLittleEndian32(mp, 12);
            CryptoBytes.Wipe(mp);

            h0 += t0 & 0x3ffffff;
            h1 += (uint)(((((UInt64)t1 << 32) | t0) >> 26) & 0x3ffffff);
            h2 += (uint)(((((UInt64)t2 << 32) | t1) >> 20) & 0x3ffffff);
            h3 += (uint)(((((UInt64)t3 << 32) | t2) >> 14) & 0x3ffffff);
            h4 += t3 >> 8;

            goto poly1305_donna_mul;

        poly1305_donna_finish:
            b = h0 >> 26; h0 = h0 & 0x3ffffff;
            h1 += b; b = h1 >> 26; h1 = h1 & 0x3ffffff;
            h2 += b; b = h2 >> 26; h2 = h2 & 0x3ffffff;
            h3 += b; b = h3 >> 26; h3 = h3 & 0x3ffffff;
            h4 += b; b = h4 >> 26; h4 = h4 & 0x3ffffff;
            h0 += b * 5;

            g0 = h0 + 5; b = g0 >> 26; g0 &= 0x3ffffff;
            g1 = h1 + b; b = g1 >> 26; g1 &= 0x3ffffff;
            g2 = h2 + b; b = g2 >> 26; g2 &= 0x3ffffff;
            g3 = h3 + b; b = g3 >> 26; g3 &= 0x3ffffff;
            g4 = unchecked(h4 + b - (1 << 26));

            b = (g4 >> 31) - 1;
            nb = ~b;
            h0 = (h0 & nb) | (g0 & b);
            h1 = (h1 & nb) | (g1 & b);
            h2 = (h2 & nb) | (g2 & b);
            h3 = (h3 & nb) | (g3 & b);
            h4 = (h4 & nb) | (g4 & b);

            f0 = ((h0) | (h1 << 26)) + (UInt64)key.x4;
            f1 = ((h1 >> 6) | (h2 << 20)) + (UInt64)key.x5;
            f2 = ((h2 >> 12) | (h3 << 14)) + (UInt64)key.x6;
            f3 = ((h3 >> 18) | (h4 << 8)) + (UInt64)key.x7;

            unchecked
            {
                ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 0, (uint)f0); f1 += (f0 >> 32);
                ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 4, (uint)f1); f2 += (f1 >> 32);
                ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 8, (uint)f2); f3 += (f2 >> 32);
                ByteIntegerConverter.StoreLittleEndian32(output, outputOffset + 12, (uint)f3);
            }
        }
    }
}
