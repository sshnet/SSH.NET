using System;

namespace Chaos.NaCl.Internal.Ed25519Ref10
{
	/*
	ge means group element.

	Here the group is the set of pairs (x,y) of field elements (see fe.h)
	satisfying -x^2 + y^2 = 1 + d x^2y^2
	where d = -121665/121666.

	Representations:
	  ge_p2 (projective): (X:Y:Z) satisfying x=X/Z, y=Y/Z
	  ge_p3 (extended): (X:Y:Z:T) satisfying x=X/Z, y=Y/Z, XY=ZT
	  ge_p1p1 (completed): ((X:Z),(Y:T)) satisfying x=X/Z, y=Y/T
	  ge_precomp (Duif): (y+x,y-x,2dxy)
	*/

	internal struct GroupElementP2
	{
		public FieldElement X;
		public FieldElement Y;
		public FieldElement Z;
	} ;

	internal struct GroupElementP3
	{
		public FieldElement X;
		public FieldElement Y;
		public FieldElement Z;
		public FieldElement T;
	} ;

	internal struct GroupElementP1P1
	{
		public FieldElement X;
		public FieldElement Y;
		public FieldElement Z;
		public FieldElement T;
	} ;

	internal struct GroupElementPreComp
	{
		public FieldElement yplusx;
		public FieldElement yminusx;
		public FieldElement xy2d;

		public GroupElementPreComp(FieldElement yplusx, FieldElement yminusx, FieldElement xy2d)
		{
			this.yplusx = yplusx;
			this.yminusx = yminusx;
			this.xy2d = xy2d;
		}
	} ;

	internal struct GroupElementCached
	{
		public FieldElement YplusX;
		public FieldElement YminusX;
		public FieldElement Z;
		public FieldElement T2d;
	} ;
}
