using UnityEngine;


namespace RT
{
	public class RTShape_Plane : RTShape
	{
		public override string TypeName { get { return TypeName_Plane; } }
		public override UnityEngine.Mesh UnityMesh { get { return RTSystem.Instance.Shape_Plane; } }


		public bool IsOneSided = false;


		protected override void DoGUI()
		{
			base.DoGUI();

			IsOneSided = GUILayout.Toggle(IsOneSided, "Is one-sided?");
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteBool(IsOneSided, "IsOneSided");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			IsOneSided = reader.ReadBool("IsOneSided");
		}
	}
}