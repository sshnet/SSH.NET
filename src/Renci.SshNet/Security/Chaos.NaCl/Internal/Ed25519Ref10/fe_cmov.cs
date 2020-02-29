using System;

namespace Renci.SshNet.Security.Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class FieldOperations
	{
		/*
		Replace (f,g) with (g,g) if b == 1;
		replace (f,g) with (f,g) if b == 0.

		Preconditions: b in {0,1}.
		*/

		//void fe_cmov(fe f,const fe g,unsigned int b)
		internal static void fe_cmov(ref FieldElement f, ref FieldElement g, int b)
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
			Int32 g0 = g.x0;
			Int32 g1 = g.x1;
			Int32 g2 = g.x2;
			Int32 g3 = g.x3;
			Int32 g4 = g.x4;
			Int32 g5 = g.x5;
			Int32 g6 = g.x6;
			Int32 g7 = g.x7;
			Int32 g8 = g.x8;
			Int32 g9 = g.x9;
			Int32 x0 = f0 ^ g0;
			Int32 x1 = f1 ^ g1;
			Int32 x2 = f2 ^ g2;
			Int32 x3 = f3 ^ g3;
			Int32 x4 = f4 ^ g4;
			Int32 x5 = f5 ^ g5;
			Int32 x6 = f6 ^ g6;
			Int32 x7 = f7 ^ g7;
			Int32 x8 = f8 ^ g8;
			Int32 x9 = f9 ^ g9;
			b = -b;
			x0 &= b;
			x1 &= b;
			x2 &= b;
			x3 &= b;
			x4 &= b;
			x5 &= b;
			x6 &= b;
			x7 &= b;
			x8 &= b;
			x9 &= b;
			f.x0 = f0 ^ x0;
			f.x1 = f1 ^ x1;
			f.x2 = f2 ^ x2;
			f.x3 = f3 ^ x3;
			f.x4 = f4 ^ x4;
			f.x5 = f5 ^ x5;
			f.x6 = f6 ^ x6;
			f.x7 = f7 ^ x7;
			f.x8 = f8 ^ x8;
			f.x9 = f9 ^ x9;
		}
	}
}