using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class GroupOperations
	{
		/*
		r = p
		*/
		public static void ge_p1p1_to_p2(out GroupElementP2 r, ref GroupElementP1P1 p)
		{
			FieldOperations.fe_mul(out r.X, ref p.X, ref p.T);
			FieldOperations.fe_mul(out r.Y, ref p.Y, ref p.Z);
			FieldOperations.fe_mul(out r.Z, ref p.Z, ref p.T);
		}

	}
}