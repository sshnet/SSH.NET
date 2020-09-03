using System;

namespace Renci.SshNet.Security.Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class GroupOperations
	{
		/*
		r = 2 * p
		*/

		internal static void ge_p2_dbl(out GroupElementP1P1 r, ref GroupElementP2 p)
		{
			FieldElement t0;

			/* qhasm: enter ge_p2_dbl */

			/* qhasm: fe X1 */

			/* qhasm: fe Y1 */

			/* qhasm: fe Z1 */

			/* qhasm: fe A */

			/* qhasm: fe AA */

			/* qhasm: fe XX */

			/* qhasm: fe YY */

			/* qhasm: fe B */

			/* qhasm: fe X3 */

			/* qhasm: fe Y3 */

			/* qhasm: fe Z3 */

			/* qhasm: fe T3 */

			/* qhasm: XX=X1^2 */
			/* asm 1: fe_sq(>XX=fe#1,<X1=fe#11); */
			/* asm 2: fe_sq(>XX=r.X,<X1=p.X); */
			FieldOperations.fe_sq(out r.X, ref p.X);

			/* qhasm: YY=Y1^2 */
			/* asm 1: fe_sq(>YY=fe#3,<Y1=fe#12); */
			/* asm 2: fe_sq(>YY=r.Z,<Y1=p.Y); */
			FieldOperations.fe_sq(out r.Z, ref p.Y);

			/* qhasm: B=2*Z1^2 */
			/* asm 1: fe_sq2(>B=fe#4,<Z1=fe#13); */
			/* asm 2: fe_sq2(>B=r.T,<Z1=p.Z); */
			FieldOperations.fe_sq2(out r.T, ref p.Z);

			/* qhasm: A=X1+Y1 */
			/* asm 1: fe_add(>A=fe#2,<X1=fe#11,<Y1=fe#12); */
			/* asm 2: fe_add(>A=r.Y,<X1=p.X,<Y1=p.Y); */
			FieldOperations.fe_add(out r.Y, ref p.X, ref p.Y);

			/* qhasm: AA=A^2 */
			/* asm 1: fe_sq(>AA=fe#5,<A=fe#2); */
			/* asm 2: fe_sq(>AA=t0,<A=r.Y); */
			FieldOperations.fe_sq(out t0, ref r.Y);

			/* qhasm: Y3=YY+XX */
			/* asm 1: fe_add(>Y3=fe#2,<YY=fe#3,<XX=fe#1); */
			/* asm 2: fe_add(>Y3=r.Y,<YY=r.Z,<XX=r.X); */
			FieldOperations.fe_add(out r.Y, ref r.Z, ref r.X);

			/* qhasm: Z3=YY-XX */
			/* asm 1: fe_sub(>Z3=fe#3,<YY=fe#3,<XX=fe#1); */
			/* asm 2: fe_sub(>Z3=r.Z,<YY=r.Z,<XX=r.X); */
			FieldOperations.fe_sub(out r.Z, ref r.Z, ref r.X);

			/* qhasm: X3=AA-Y3 */
			/* asm 1: fe_sub(>X3=fe#1,<AA=fe#5,<Y3=fe#2); */
			/* asm 2: fe_sub(>X3=r.X,<AA=t0,<Y3=r.Y); */
			FieldOperations.fe_sub(out r.X, ref t0, ref r.Y);

			/* qhasm: T3=B-Z3 */
			/* asm 1: fe_sub(>T3=fe#4,<B=fe#4,<Z3=fe#3); */
			/* asm 2: fe_sub(>T3=r.T,<B=r.T,<Z3=r.Z); */
			FieldOperations.fe_sub(out r.T, ref r.T, ref r.Z);

			/* qhasm: return */

		}
	}
}