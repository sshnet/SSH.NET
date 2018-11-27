using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
	internal static partial class GroupOperations
	{
		private static void slide(sbyte[] r, byte[] a)
		{
			int i;
			int b;
			int k;

			for (i = 0; i < 256; ++i)
				r[i] = (sbyte)(1 & (a[i >> 3] >> (i & 7)));

			for (i = 0; i < 256; ++i)
				if (r[i] != 0)
				{
					for (b = 1; b <= 6 && i + b < 256; ++b)
					{
						if (r[i + b] != 0)
						{
							if (r[i] + (r[i + b] << b) <= 15)
							{
								r[i] += (sbyte)(r[i + b] << b); r[i + b] = 0;
							}
							else if (r[i] - (r[i + b] << b) >= -15)
							{
								r[i] -= (sbyte)(r[i + b] << b);
								for (k = i + b; k < 256; ++k)
								{
									if (r[k] == 0)
									{
										r[k] = 1;
										break;
									}
									r[k] = 0;
								}
							}
							else
								break;
						}
					}
				}

		}

		/*
		r = a * A + b * B
		where a = a[0]+256*a[1]+...+256^31 a[31].
		and b = b[0]+256*b[1]+...+256^31 b[31].
		B is the Ed25519 base point (x,4/5) with x positive.
		*/

		public static void ge_double_scalarmult_vartime(out GroupElementP2 r, byte[] a, ref GroupElementP3 A, byte[] b)
		{
			GroupElementPreComp[] Bi = LookupTables.Base2;
            // todo: Perhaps remove these allocations?
			sbyte[] aslide = new sbyte[256];
			sbyte[] bslide = new sbyte[256];
			GroupElementCached[] Ai = new GroupElementCached[8]; /* A,3A,5A,7A,9A,11A,13A,15A */
			GroupElementP1P1 t;
			GroupElementP3 u;
			GroupElementP3 A2;
			int i;

			slide(aslide, a);
			slide(bslide, b);

			ge_p3_to_cached(out Ai[0], ref A);
			ge_p3_dbl(out t, ref A); ge_p1p1_to_p3(out A2, ref t);
			ge_add(out t, ref A2, ref Ai[0]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out Ai[1], ref u);
			ge_add(out t, ref A2, ref Ai[1]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out Ai[2], ref u);
			ge_add(out t, ref A2, ref Ai[2]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out Ai[3], ref u);
			ge_add(out t, ref A2, ref Ai[3]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out Ai[4], ref u);
			ge_add(out t, ref A2, ref Ai[4]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out Ai[5], ref u);
			ge_add(out t, ref A2, ref Ai[5]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out Ai[6], ref u);
			ge_add(out t, ref A2, ref Ai[6]); ge_p1p1_to_p3(out u, ref t); ge_p3_to_cached(out Ai[7], ref u);

			ge_p2_0(out r);

			for (i = 255; i >= 0; --i)
			{
				if ((aslide[i] != 0) || (bslide[i] != 0)) break;
			}

			for (; i >= 0; --i)
			{
				ge_p2_dbl(out t, ref r);

				if (aslide[i] > 0)
				{
					ge_p1p1_to_p3(out u, ref t);
					ge_add(out t, ref u, ref Ai[aslide[i] / 2]);
				}
				else if (aslide[i] < 0)
				{
					ge_p1p1_to_p3(out u, ref t);
					ge_sub(out t, ref u, ref Ai[(-aslide[i]) / 2]);
				}

				if (bslide[i] > 0)
				{
					ge_p1p1_to_p3(out u, ref t);
					ge_madd(out t, ref u, ref Bi[bslide[i] / 2]);
				}
				else if (bslide[i] < 0)
				{
					ge_p1p1_to_p3(out u, ref t);
					ge_msub(out t, ref u, ref Bi[(-bslide[i]) / 2]);
				}

				ge_p1p1_to_p2(out r, ref t);
			}
		}

	}
}