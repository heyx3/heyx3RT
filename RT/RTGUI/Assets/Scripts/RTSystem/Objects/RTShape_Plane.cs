using UnityEngine;


namespace RT
{
	[ExecuteInEditMode]
	public class RTShape_Plane : RTShape
	{
		public bool IsOneSided = true;

		[SerializeField]
		[HideInInspector]
		private PlaneFollower follower = null;


		public override string TypeName { get { return TypeName_Plane; } }
		public override UnityEngine.Mesh UnityMesh { get { return RTSystem.Instance.Shape_Plane; } }


		public override void Awake()
		{
			base.Awake();
		}
		private void Update()
		{
			if (follower == null || follower.MyTr.parent != transform)
			{
				follower = new GameObject("Plane Backside").AddComponent<PlaneFollower>();
				follower.MyTr.parent = transform;
			}
		}

		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);
			writer.Bool(IsOneSided, "IsOneSided");
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);
			IsOneSided = reader.Bool("IsOneSided");
		}
	}
}