using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class GroupOperations
	{
		/*
		r = p + q
		*/
		public static void ge_madd(out GroupElementP1P1 r, ref  GroupElementP3 p, ref GroupElementPreComp q)
		{
			FieldElement t0;

			/* qhasm: enter ge_madd */

			/* qhasm: fe X1 */

			/* qhasm: fe Y1 */

			/* qhasm: fe Z1 */

			/* qhasm: fe T1 */

			/* qhasm: fe ypx2 */

			/* qhasm: fe ymx2 */

			/* qhasm: fe xy2d2 */

			/* qhasm: fe X3 */

			/* qhasm: fe Y3 */

			/* qhasm: fe Z3 */

			/* qhasm: fe T3 */

			/* qhasm: fe YpX1 */

			/* qhasm: fe YmX1 */

			/* qhasm: fe A */

			/* qhasm: fe B */

			/* qhasm: fe C */

			/* qhasm: fe D */

			/* qhasm: YpX1 = Y1+X1 */
			/* asm 1: fe_add(>YpX1=fe#1,<Y1=fe#12,<X1=fe#11); */
			/* asm 2: fe_add(>YpX1=r.X,<Y1=p.Y,<X1=p.X); */
			FieldOperations.fe_add(out r.X, ref p.Y, ref p.X);

			/* qhasm: YmX1 = Y1-X1 */
			/* asm 1: fe_sub(>YmX1=fe#2,<Y1=fe#12,<X1=fe#11); */
			/* asm 2: fe_sub(>YmX1=r.Y,<Y1=p.Y,<X1=p.X); */
			FieldOperations.fe_sub(out r.Y, ref p.Y, ref p.X);

			/* qhasm: A = YpX1*ypx2 */
			/* asm 1: fe_mul(>A=fe#3,<YpX1=fe#1,<ypx2=fe#15); */
			/* asm 2: fe_mul(>A=r.Z,<YpX1=r.X,<ypx2=q.yplusx); */
			FieldOperations.fe_mul(out r.Z, ref r.X, ref q.yplusx);

			/* qhasm: B = YmX1*ymx2 */
			/* asm 1: fe_mul(>B=fe#2,<YmX1=fe#2,<ymx2=fe#16); */
			/* asm 2: fe_mul(>B=r.Y,<YmX1=r.Y,<ymx2=q.yminusx); */
			FieldOperations.fe_mul(out r.Y, ref r.Y, ref q.yminusx);

			/* qhasm: C = xy2d2*T1 */
			/* asm 1: fe_mul(>C=fe#4,<xy2d2=fe#17,<T1=fe#14); */
			/* asm 2: fe_mul(>C=r.T,<xy2d2=q.xy2d,<T1=p.T); */
			FieldOperations.fe_mul(out r.T, ref q.xy2d, ref p.T);

			/* qhasm: D = 2*Z1 */
			/* asm 1: fe_add(>D=fe#5,<Z1=fe#13,<Z1=fe#13); */
			/* asm 2: fe_add(>D=t0,<Z1=p.Z,<Z1=p.Z); */
			FieldOperations.fe_add(out t0, ref p.Z, ref p.Z);

			/* qhasm: X3 = A-B */
			/* asm 1: fe_sub(>X3=fe#1,<A=fe#3,<B=fe#2); */
			/* asm 2: fe_sub(>X3=r.X,<A=r.Z,<B=r.Y); */
			FieldOperations.fe_sub(out r.X, ref r.Z, ref r.Y);

			/* qhasm: Y3 = A+B */
			/* asm 1: fe_add(>Y3=fe#2,<A=fe#3,<B=fe#2); */
			/* asm 2: fe_add(>Y3=r.Y,<A=r.Z,<B=r.Y); */
			FieldOperations.fe_add(out r.Y, ref r.Z, ref r.Y);

			/* qhasm: Z3 = D+C */
			/* asm 1: fe_add(>Z3=fe#3,<D=fe#5,<C=fe#4); */
			/* asm 2: fe_add(>Z3=r.Z,<D=t0,<C=r.T); */
			FieldOperations.fe_add(out r.Z, ref t0, ref r.T);

			/* qhasm: T3 = D-C */
			/* asm 1: fe_sub(>T3=fe#4,<D=fe#5,<C=fe#4); */
			/* asm 2: fe_sub(>T3=r.T,<D=t0,<C=r.T); */
			FieldOperations.fe_sub(out r.T, ref t0, ref r.T);

			/* qhasm: return */

		}

	}
}