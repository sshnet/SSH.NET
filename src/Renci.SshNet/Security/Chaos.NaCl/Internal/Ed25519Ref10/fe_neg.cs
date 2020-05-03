using System;

namespace Renci.SshNet.Security.Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class FieldOperations
	{
		/*
		h = -f

		Preconditions:
		   |f| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.

		Postconditions:
		   |h| bounded by 1.1*2^25,1.1*2^24,1.1*2^25,1.1*2^24,etc.
		*/
		internal static void fe_neg(out FieldElement h, ref FieldElement f)
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
			Int32 h0 = -f0;
			Int32 h1 = -f1;
			Int32 h2 = -f2;
			Int32 h3 = -f3;
			Int32 h4 = -f4;
			Int32 h5 = -f5;
			Int32 h6 = -f6;
			Int32 h7 = -f7;
			Int32 h8 = -f8;
			Int32 h9 = -f9;
			h.x0 = h0;
			h.x1 = h1;
			h.x2 = h2;
			h.x3 = h3;
			h.x4 = h4;
			h.x5 = h5;
			h.x6 = h6;
			h.x7 = h7;
			h.x8 = h8;
			h.x9 = h9;
		}
	}
}