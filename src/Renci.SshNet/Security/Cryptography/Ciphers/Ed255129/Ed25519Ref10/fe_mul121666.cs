using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class FieldOperations
	{

		/*
		h = f * 121666
		Can overlap h with f.

		Preconditions:
		   |f| bounded by 1.1*2^26,1.1*2^25,1.1*2^26,1.1*2^25,etc.

		Postconditions:
		   |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
		*/

		public static void fe_mul121666(out FieldElement h, ref FieldElement f)
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
			Int64 h0 = f0 * (Int64)121666;
			Int64 h1 = f1 * (Int64)121666;
			Int64 h2 = f2 * (Int64)121666;
			Int64 h3 = f3 * (Int64)121666;
			Int64 h4 = f4 * (Int64)121666;
			Int64 h5 = f5 * (Int64)121666;
			Int64 h6 = f6 * (Int64)121666;
			Int64 h7 = f7 * (Int64)121666;
			Int64 h8 = f8 * (Int64)121666;
			Int64 h9 = f9 * (Int64)121666;
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

			carry9 = (h9 + (Int64)(1 << 24)) >> 25; h0 += carry9 * 19; h9 -= carry9 << 25;
			carry1 = (h1 + (Int64)(1 << 24)) >> 25; h2 += carry1; h1 -= carry1 << 25;
			carry3 = (h3 + (Int64)(1 << 24)) >> 25; h4 += carry3; h3 -= carry3 << 25;
			carry5 = (h5 + (Int64)(1 << 24)) >> 25; h6 += carry5; h5 -= carry5 << 25;
			carry7 = (h7 + (Int64)(1 << 24)) >> 25; h8 += carry7; h7 -= carry7 << 25;

			carry0 = (h0 + (Int64)(1 << 25)) >> 26; h1 += carry0; h0 -= carry0 << 26;
			carry2 = (h2 + (Int64)(1 << 25)) >> 26; h3 += carry2; h2 -= carry2 << 26;
			carry4 = (h4 + (Int64)(1 << 25)) >> 26; h5 += carry4; h4 -= carry4 << 26;
			carry6 = (h6 + (Int64)(1 << 25)) >> 26; h7 += carry6; h6 -= carry6 << 26;
			carry8 = (h8 + (Int64)(1 << 25)) >> 26; h9 += carry8; h8 -= carry8 << 26;

			h.x0 = (int)h0;
			h.x1 = (int)h1;
			h.x2 = (int)h2;
			h.x3 = (int)h3;
			h.x4 = (int)h4;
			h.x5 = (int)h5;
			h.x6 = (int)h6;
			h.x7 = (int)h7;
			h.x8 = (int)h8;
			h.x9 = (int)h9;
		}
	}
}