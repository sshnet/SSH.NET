using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class FieldOperations
	{
		/*
		h = f * f
		Can overlap h with f.

		Preconditions:
		   |f| bounded by 1.65*2^26,1.65*2^25,1.65*2^26,1.65*2^25,etc.

		Postconditions:
		   |h| bounded by 1.01*2^25,1.01*2^24,1.01*2^25,1.01*2^24,etc.
		*/

		/*
		See fe_mul.c for discussion of implementation strategy.
		*/
		internal static void fe_sq(out FieldElement h, ref FieldElement f)
		{
			Int32 f0 = f.x0;
			Int32 f1 = f.x1;
			Int32 f2 = f.x2;
			Int32 f3 = f.x3;
			Int32 f4 = f.x4;
			Int32 f5 = f.x5;
			Int32 f6 = f.x6;
			Int32 f7 = f.x7;
			Int32 f8 = f.x8;
			Int32 f9 = f.x9;
			Int32 f0_2 = 2 * f0;
			Int32 f1_2 = 2 * f1;
			Int32 f2_2 = 2 * f2;
			Int32 f3_2 = 2 * f3;
			Int32 f4_2 = 2 * f4;
			Int32 f5_2 = 2 * f5;
			Int32 f6_2 = 2 * f6;
			Int32 f7_2 = 2 * f7;
			Int32 f5_38 = 38 * f5; /* 1.959375*2^30 */
			Int32 f6_19 = 19 * f6; /* 1.959375*2^30 */
			Int32 f7_38 = 38 * f7; /* 1.959375*2^30 */
			Int32 f8_19 = 19 * f8; /* 1.959375*2^30 */
			Int32 f9_38 = 38 * f9; /* 1.959375*2^30 */
			Int64 f0f0 = f0 * (Int64)f0;
			Int64 f0f1_2 = f0_2 * (Int64)f1;
			Int64 f0f2_2 = f0_2 * (Int64)f2;
			Int64 f0f3_2 = f0_2 * (Int64)f3;
			Int64 f0f4_2 = f0_2 * (Int64)f4;
			Int64 f0f5_2 = f0_2 * (Int64)f5;
			Int64 f0f6_2 = f0_2 * (Int64)f6;
			Int64 f0f7_2 = f0_2 * (Int64)f7;
			Int64 f0f8_2 = f0_2 * (Int64)f8;
			Int64 f0f9_2 = f0_2 * (Int64)f9;
			Int64 f1f1_2 = f1_2 * (Int64)f1;
			Int64 f1f2_2 = f1_2 * (Int64)f2;
			Int64 f1f3_4 = f1_2 * (Int64)f3_2;
			Int64 f1f4_2 = f1_2 * (Int64)f4;
			Int64 f1f5_4 = f1_2 * (Int64)f5_2;
			Int64 f1f6_2 = f1_2 * (Int64)f6;
			Int64 f1f7_4 = f1_2 * (Int64)f7_2;
			Int64 f1f8_2 = f1_2 * (Int64)f8;
			Int64 f1f9_76 = f1_2 * (Int64)f9_38;
			Int64 f2f2 = f2 * (Int64)f2;
			Int64 f2f3_2 = f2_2 * (Int64)f3;
			Int64 f2f4_2 = f2_2 * (Int64)f4;
			Int64 f2f5_2 = f2_2 * (Int64)f5;
			Int64 f2f6_2 = f2_2 * (Int64)f6;
			Int64 f2f7_2 = f2_2 * (Int64)f7;
			Int64 f2f8_38 = f2_2 * (Int64)f8_19;
			Int64 f2f9_38 = f2 * (Int64)f9_38;
			Int64 f3f3_2 = f3_2 * (Int64)f3;
			Int64 f3f4_2 = f3_2 * (Int64)f4;
			Int64 f3f5_4 = f3_2 * (Int64)f5_2;
			Int64 f3f6_2 = f3_2 * (Int64)f6;
			Int64 f3f7_76 = f3_2 * (Int64)f7_38;
			Int64 f3f8_38 = f3_2 * (Int64)f8_19;
			Int64 f3f9_76 = f3_2 * (Int64)f9_38;
			Int64 f4f4 = f4 * (Int64)f4;
			Int64 f4f5_2 = f4_2 * (Int64)f5;
			Int64 f4f6_38 = f4_2 * (Int64)f6_19;
			Int64 f4f7_38 = f4 * (Int64)f7_38;
			Int64 f4f8_38 = f4_2 * (Int64)f8_19;
			Int64 f4f9_38 = f4 * (Int64)f9_38;
			Int64 f5f5_38 = f5 * (Int64)f5_38;
			Int64 f5f6_38 = f5_2 * (Int64)f6_19;
			Int64 f5f7_76 = f5_2 * (Int64)f7_38;
			Int64 f5f8_38 = f5_2 * (Int64)f8_19;
			Int64 f5f9_76 = f5_2 * (Int64)f9_38;
			Int64 f6f6_19 = f6 * (Int64)f6_19;
			Int64 f6f7_38 = f6 * (Int64)f7_38;
			Int64 f6f8_38 = f6_2 * (Int64)f8_19;
			Int64 f6f9_38 = f6 * (Int64)f9_38;
			Int64 f7f7_38 = f7 * (Int64)f7_38;
			Int64 f7f8_38 = f7_2 * (Int64)f8_19;
			Int64 f7f9_76 = f7_2 * (Int64)f9_38;
			Int64 f8f8_19 = f8 * (Int64)f8_19;
			Int64 f8f9_38 = f8 * (Int64)f9_38;
			Int64 f9f9_38 = f9 * (Int64)f9_38;
			Int64 h0 = f0f0 + f1f9_76 + f2f8_38 + f3f7_76 + f4f6_38 + f5f5_38;
			Int64 h1 = f0f1_2 + f2f9_38 + f3f8_38 + f4f7_38 + f5f6_38;
			Int64 h2 = f0f2_2 + f1f1_2 + f3f9_76 + f4f8_38 + f5f7_76 + f6f6_19;
			Int64 h3 = f0f3_2 + f1f2_2 + f4f9_38 + f5f8_38 + f6f7_38;
			Int64 h4 = f0f4_2 + f1f3_4 + f2f2 + f5f9_76 + f6f8_38 + f7f7_38;
			Int64 h5 = f0f5_2 + f1f4_2 + f2f3_2 + f6f9_38 + f7f8_38;
			Int64 h6 = f0f6_2 + f1f5_4 + f2f4_2 + f3f3_2 + f7f9_76 + f8f8_19;
			Int64 h7 = f0f7_2 + f1f6_2 + f2f5_2 + f3f4_2 + f8f9_38;
			Int64 h8 = f0f8_2 + f1f7_4 + f2f6_2 + f3f5_4 + f4f4 + f9f9_38;
			Int64 h9 = f0f9_2 + f1f8_2 + f2f7_2 + f3f6_2 + f4f5_2;
			Int64 carry0;
			Int64 carry1;
			Int64 carry2;
			Int64 carry3;
			Int64 carry4;
			Int64 carry5;
			Int64 carry6;
			Int64 carry7;
			Int64 carry8;
			Int64 carry9;

			carry0 = (h0 + (Int64)(1 << 25)) >> 26; h1 += carry0; h0 -= carry0 << 26;
			carry4 = (h4 + (Int64)(1 << 25)) >> 26; h5 += carry4; h4 -= carry4 << 26;

			carry1 = (h1 + (Int64)(1 << 24)) >> 25; h2 += carry1; h1 -= carry1 << 25;
			carry5 = (h5 + (Int64)(1 << 24)) >> 25; h6 += carry5; h5 -= carry5 << 25;

			carry2 = (h2 + (Int64)(1 << 25)) >> 26; h3 += carry2; h2 -= carry2 << 26;
			carry6 = (h6 + (Int64)(1 << 25)) >> 26; h7 += carry6; h6 -= carry6 << 26;

			carry3 = (h3 + (Int64)(1 << 24)) >> 25; h4 += carry3; h3 -= carry3 << 25;
			carry7 = (h7 + (Int64)(1 << 24)) >> 25; h8 += carry7; h7 -= carry7 << 25;

			carry4 = (h4 + (Int64)(1 << 25)) >> 26; h5 += carry4; h4 -= carry4 << 26;
			carry8 = (h8 + (Int64)(1 << 25)) >> 26; h9 += carry8; h8 -= carry8 << 26;

			carry9 = (h9 + (Int64)(1 << 24)) >> 25; h0 += carry9 * 19; h9 -= carry9 << 25;

			carry0 = (h0 + (Int64)(1 << 25)) >> 26; h1 += carry0; h0 -= carry0 << 26;

			h.x0 = (Int32)h0;
			h.x1 = (Int32)h1;
			h.x2 = (Int32)h2;
			h.x3 = (Int32)h3;
			h.x4 = (Int32)h4;
			h.x5 = (Int32)h5;
			h.x6 = (Int32)h6;
			h.x7 = (Int32)h7;
			h.x8 = (Int32)h8;
			h.x9 = (Int32)h9;
		}
	}
}