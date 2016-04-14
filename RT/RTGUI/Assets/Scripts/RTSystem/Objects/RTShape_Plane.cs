namespace RT
{
	public class RTShape_Plane : RTShape
	{
		public override string TypeName { get { return TypeName_Plane; } }


		public override UnityEngine.Mesh GetUnityMesh()
		{
			return RTSystem.Instance.Shape_Plane;
		}
	}
}