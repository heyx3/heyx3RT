namespace RT
{
	public class RTShape_Sphere : RTShape
	{
		public override string TypeName { get { return TypeName_Sphere; } }

		public override UnityEngine.Mesh GetUnityMesh()
		{
			return RTSystem.Instance.Shape_Sphere;
		}
	}
}