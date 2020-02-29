using System;
using System.Collections.Generic;

namespace Renci.SshNet.Security.Chaos.NaCl.Internal.Salsa
{
    internal static class SalsaCore
    {
        internal static void HSalsa(out Array16<UInt32> output, ref Array16<UInt32> input, int rounds)
        {
            InternalAssert.Assert(rounds % 2 == 0, "Number of salsa rounds must be even");

            int doubleRounds = rounds / 2;

            UInt32 x0 = input.x0;
            UInt32 x1 = input.x1;
            UInt32 x2 = input.x2;
            UInt32 x3 = input.x3;
            UInt32 x4 = input.x4;
            UInt32 x5 = input.x5;
            UInt32 x6 = input.x6;
            UInt32 x7 = input.x7;
            UInt32 x8 = input.x8;
            UInt32 x9 = input.x9;
            UInt32 x10 = input.x10;
            UInt32 x11 = input.x11;
            UInt32 x12 = input.x12;
            UInt32 x13 = input.x13;
            UInt32 x14 = input.x14;
            UInt32 x15 = input.x15;

            unchecked
            {
                for (int i = 0; i < doubleRounds; i++)
                {
                    UInt32 y;

                    // row 0
                    y = x0 + x12;
                    x4 ^= (y << 7) | (y >> (32 - 7));
                    y = x4 + x0;
                    x8 ^= (y << 9) | (y >> (32 - 9));
                    y = x8 + x4;
                    x12 ^= (y << 13) | (y >> (32 - 13));
                    y = x12 + x8;
                    x0 ^= (y << 18) | (y >> (32 - 18));

                    // row 1
                    y = x5 + x1;
                    x9 ^= (y << 7) | (y >> (32 - 7));
                    y = x9 + x5;
                    x13 ^= (y << 9) | (y >> (32 - 9));
                    y = x13 + x9;
                    x1 ^= (y << 13) | (y >> (32 - 13));
                    y = x1 + x13;
                    x5 ^= (y << 18) | (y >> (32 - 18));

                    // row 2
                    y = x10 + x6;
                    x14 ^= (y << 7) | (y >> (32 - 7));
                    y = x14 + x10;
                    x2 ^= (y << 9) | (y >> (32 - 9));
                    y = x2 + x14;
                    x6 ^= (y << 13) | (y >> (32 - 13));
                    y = x6 + x2;
                    x10 ^= (y << 18) | (y >> (32 - 18));

                    // row 3
                    y = x15 + x11;
                    x3 ^= (y << 7) | (y >> (32 - 7));
                    y = x3 + x15;
                    x7 ^= (y << 9) | (y >> (32 - 9));
                    y = x7 + x3;
                    x11 ^= (y << 13) | (y >> (32 - 13));
                    y = x11 + x7;
                    x15 ^= (y << 18) | (y >> (32 - 18));

                    // column 0
                    y = x0 + x3;
                    x1 ^= (y << 7) | (y >> (32 - 7));
                    y = x1 + x0;
                    x2 ^= (y << 9) | (y >> (32 - 9));
                    y = x2 + x1;
                    x3 ^= (y << 13) | (y >> (32 - 13));
                    y = x3 + x2;
                    x0 ^= (y << 18) | (y >> (32 - 18));

                    // column 1
                    y = x5 + x4;
                    x6 ^= (y << 7) | (y >> (32 - 7));
                    y = x6 + x5;
                    x7 ^= (y << 9) | (y >> (32 - 9));
                    y = x7 + x6;
                    x4 ^= (y << 13) | (y >> (32 - 13));
                    y = x4 + x7;
                    x5 ^= (y << 18) | (y >> (32 - 18));

                    // column 2
                    y = x10 + x9;
                    x11 ^= (y << 7) | (y >> (32 - 7));
                    y = x11 + x10;
                    x8 ^= (y << 9) | (y >> (32 - 9));
                    y = x8 + x11;
                    x9 ^= (y << 13) | (y >> (32 - 13));
                    y = x9 + x8;
                    x10 ^= (y << 18) | (y >> (32 - 18));

                    // column 3
                    y = x15 + x14;
                    x12 ^= (y << 7) | (y >> (32 - 7));
                    y = x12 + x15;
                    x13 ^= (y << 9) | (y >> (32 - 9));
                    y = x13 + x12;
                    x14 ^= (y << 13) | (y >> (32 - 13));
                    y = x14 + x13;
                    x15 ^= (y << 18) | (y >> (32 - 18));
                }
            }

            output.x0 = x0;
            output.x1 = x1;
            output.x2 = x2;
            output.x3 = x3;
            output.x4 = x4;
            output.x5 = x5;
            output.x6 = x6;
            output.x7 = x7;
            output.x8 = x8;
            output.x9 = x9;
            output.x10 = x10;
            output.x11 = x11;
            output.x12 = x12;
            output.x13 = x13;
            output.x14 = x14;
            output.x15 = x15;
        }

        internal static void Salsa(out Array16<UInt32> output, ref Array16<UInt32> input, int rounds)
        {
            Array16<UInt32> temp;
            HSalsa(out temp, ref input, rounds);
            unchecked
            {
                output.x0 = temp.x0 + input.x0;
                output.x1 = temp.x1 + input.x1;
                output.x2 = temp.x2 + input.x2;
                output.x3 = temp.x3 + input.x3;
                output.x4 = temp.x4 + input.x4;
                output.x5 = temp.x5 + input.x5;
                output.x6 = temp.x6 + input.x6;
                output.x7 = temp.x7 + input.x7;
                output.x8 = temp.x8 + input.x8;
                output.x9 = temp.x9 + input.x9;
                output.x10 = temp.x10 + input.x10;
                output.x11 = temp.x11 + input.x11;
                output.x12 = temp.x12 + input.x12;
                output.x13 = temp.x13 + input.x13;
                output.x14 = temp.x14 + input.x14;
                output.x15 = temp.x15 + input.x15;
            }
        }

        /*internal static void SalsaCore(int[] output, int outputOffset, int[] input, int inputOffset, int rounds)
        {
            if (rounds % 2 != 0)
                throw new ArgumentException("rounds must be even");
        }


static void store_littleendian(unsigned char *x,uint32 u)
{
  x[0] = u; u >>= 8;
  x[1] = u; u >>= 8;
  x[2] = u; u >>= 8;
  x[3] = u;
}

        internal static void HSalsaCore(int[] output, int outputOffset, int[] input, int inputOffset, int rounds)
        {
            if (rounds % 2 != 0)
                throw new ArgumentException("rounds must be even");
            static uint32 rotate(uint32 u,int c)
{
  return (u << c) | (u >> (32 - c));
}



int crypto_core(
        unsigned char *out,
  const unsigned char *in,
  const unsigned char *k,
  const unsigned char *c
)
{
  uint32 x0, x1, x2, x3, x4, x5, x6, x7, x8, x9, x10, x11, x12, x13, x14, x15;
  int i;

  x0 = load_littleendian(c + 0);
  x1 = load_littleendian(k + 0);
  x2 = load_littleendian(k + 4);
  x3 = load_littleendian(k + 8);
  x4 = load_littleendian(k + 12);
  x5 = load_littleendian(c + 4);
  x6 = load_littleendian(in + 0);
  x7 = load_littleendian(in + 4);
  x8 = load_littleendian(in + 8);
  x9 = load_littleendian(in + 12);
  x10 = load_littleendian(c + 8);
  x11 = load_littleendian(k + 16);
  x12 = load_littleendian(k + 20);
  x13 = load_littleendian(k + 24);
  x14 = load_littleendian(k + 28);
  x15 = load_littleendian(c + 12);

  for (i = ROUNDS;i > 0;i -= 2) {
     x4 ^= rotate( x0+x12, 7);
     x8 ^= rotate( x4+ x0, 9);
    x12 ^= rotate( x8+ x4,13);
     x0 ^= rotate(x12+ x8,18);
     x9 ^= rotate( x5+ x1, 7);
    x13 ^= rotate( x9+ x5, 9);
     x1 ^= rotate(x13+ x9,13);
     x5 ^= rotate( x1+x13,18);
    x14 ^= rotate(x10+ x6, 7);
     x2 ^= rotate(x14+x10, 9);
     x6 ^= rotate( x2+x14,13);
    x10 ^= rotate( x6+ x2,18);
     x3 ^= rotate(x15+x11, 7);
     x7 ^= rotate( x3+x15, 9);
    x11 ^= rotate( x7+ x3,13);
    x15 ^= rotate(x11+ x7,18);
     x1 ^= rotate( x0+ x3, 7);
     x2 ^= rotate( x1+ x0, 9);
     x3 ^= rotate( x2+ x1,13);
     x0 ^= rotate( x3+ x2,18);
     x6 ^= rotate( x5+ x4, 7);
     x7 ^= rotate( x6+ x5, 9);
     x4 ^= rotate( x7+ x6,13);
     x5 ^= rotate( x4+ x7,18);
    x11 ^= rotate(x10+ x9, 7);
     x8 ^= rotate(x11+x10, 9);
     x9 ^= rotate( x8+x11,13);
    x10 ^= rotate( x9+ x8,18);
    x12 ^= rotate(x15+x14, 7);
    x13 ^= rotate(x12+x15, 9);
    x14 ^= rotate(x13+x12,13);
    x15 ^= rotate(x14+x13,18);
  }

  store_littleendian(out + 0,x0);
  store_littleendian(out + 4,x5);
  store_littleendian(out + 8,x10);
  store_littleendian(out + 12,x15);
  store_littleendian(out + 16,x6);
  store_littleendian(out + 20,x7);
  store_littleendian(out + 24,x8);
  store_littleendian(out + 28,x9);

  return 0;
}*/

    }
}
