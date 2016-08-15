using System;


namespace RT
{
	public class MV_SurfPos : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfPos; } }
	}
	public class MV_SurfNormal : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfNormal; } }
	}
	public class MV_SurfTangent : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfTangent; } }
	}
	public class MV_SurfBitangent : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfBitangent; } }
	}
	public class MV_SurfUV : MaterialValue
	{
		public override string TypeName { get { return TypeName_SurfUV; } }
	}
	public class MV_RayStartPos : MaterialValue
	{
		public override string TypeName { get { return TypeName_RayStartPos; } }
	}
	public class MV_RayDir : MaterialValue
	{
		public override string TypeName { get { return TypeName_RayDir; } }
	}
	public class MV_ShapePos : MaterialValue
	{
		public override string TypeName { get { return TypeName_ShapePos; } }
	}
	public class MV_ShapeScale : MaterialValue
	{
		public override string TypeName { get { return TypeName_ShapeScale; } }
	}
	public class MV_ShapeRot : MaterialValue
	{
		public override string TypeName { get { return TypeName_ShapeRot; } }
	}
}