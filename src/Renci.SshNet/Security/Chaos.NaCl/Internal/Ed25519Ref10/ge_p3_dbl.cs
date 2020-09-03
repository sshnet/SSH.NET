using System;

namespace Renci.SshNet.Security.Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class GroupOperations
	{
		/*
		r = 2 * p
		*/
		internal static void ge_p3_dbl(out GroupElementP1P1 r, ref GroupElementP3 p)
		{
			GroupElementP2 q;
			ge_p3_to_p2(out q, ref p);
			ge_p2_dbl(out r, ref q);
		}
	}
}