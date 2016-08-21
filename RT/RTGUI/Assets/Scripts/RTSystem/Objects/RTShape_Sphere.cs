using UnityEngine;


namespace RT
{
	[ExecuteInEditMode]
	public class RTShape_Sphere : RTShape
	{
		public override string TypeName { get { return TypeName_Sphere; } }
		public override UnityEngine.Mesh UnityMesh { get { return RTSystem.Instance.Shape_Sphere; } }
	}
}