using System;

namespace Renci.SshNet.Security.Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class GroupOperations
	{
		/*
		r = p
		*/
		internal static void ge_p3_to_p2(out GroupElementP2 r, ref GroupElementP3 p)
		{
			r.X = p.X;
			r.Y = p.Y;
			r.Z = p.Z;
		}
	}
}