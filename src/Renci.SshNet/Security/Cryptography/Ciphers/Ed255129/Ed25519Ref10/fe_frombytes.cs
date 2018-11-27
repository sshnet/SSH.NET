using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class FieldOperations
	{
		private static Int64 load_3(byte[] data, int offset)
		{
			uint result;
			result = (uint)data[offset + 0];
			result |= (uint)data[offset + 1] << 8;
			result |= (uint)data[offset + 2] << 16;
			return (Int64)(UInt64)result;
		}

		private static Int64 load_4(byte[] data, int offset)
		{
			uint result;
			result = (uint)data[offset + 0];
			result |= (uint)data[offset + 1] << 8;
			result |= (uint)data[offset + 2] << 16;
			result |= (uint)data[offset + 3] << 24;
			return (Int64)(UInt64)result;
		}

		//	Ignores top bit of h.
		internal static void fe_frombytes(out FieldElement h, byte[] data, int offset)
		{
			Int64 h0 = load_4(data, offset);
			Int64 h1 = load_3(data, offset + 4) << 6;
			Int64 h2 = load_3(data, offset + 7) << 5;
			Int64 h3 = load_3(data, offset + 10) << 3;
			Int64 h4 = load_3(data, offset + 13) << 2;
			Int64 h5 = load_4(data, offset + 16);
			Int64 h6 = load_3(data, offset + 20) << 7;
			Int64 h7 = load_3(data, offset + 23) << 5;
			Int64 h8 = load_3(data, offset + 26) << 4;
			Int64 h9 = (load_3(data, offset + 29) & 8388607) << 2;
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

		// does NOT ignore top bit
		internal static void fe_frombytes2(out FieldElement h, byte[] data, int offset)
		{
			Int64 h0 = load_4(data, offset);
			Int64 h1 = load_3(data, offset + 4) << 6;
			Int64 h2 = load_3(data, offset + 7) << 5;
			Int64 h3 = load_3(data, offset + 10) << 3;
			Int64 h4 = load_3(data, offset + 13) << 2;
			Int64 h5 = load_4(data, offset + 16);
			Int64 h6 = load_3(data, offset + 20) << 7;
			Int64 h7 = load_3(data, offset + 23) << 5;
			Int64 h8 = load_3(data, offset + 26) << 4;
			Int64 h9 = load_3(data, offset + 29) << 2;
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
