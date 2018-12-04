using System;

namespace Renci.SshNet.Security.Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class GroupOperations
	{
		internal static void ge_p3_0(out GroupElementP3 h)
		{
			FieldOperations.fe_0(out h.X);
			FieldOperations.fe_1(out h.Y);
			FieldOperations.fe_1(out h.Z);
			FieldOperations.fe_0(out  h.T);
		}
	}
}