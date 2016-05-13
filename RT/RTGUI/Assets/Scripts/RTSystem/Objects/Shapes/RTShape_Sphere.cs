using UnityEngine;


namespace RT
{
	public class RTShape_Sphere : RTShape
	{
		public override string TypeName { get { return TypeName_Sphere; } }
		public override UnityEngine.Mesh UnityMesh {  get { return RTSystem.Instance.Shape_Sphere; } }


		/// <summary>
		/// The axis this sphere's UV's are wrapped around.
		/// 0 = X, 1 = Y, 2 = Z.
		/// </summary>
		public uint WrapAxis = 1;


		protected override void DoGUI()
		{
			base.DoGUI();

			GUILayout.BeginHorizontal();
				GUILayout.Label("Wrap Axis");
				WrapAxis = (uint)GUILayout.HorizontalSlider(WrapAxis, -0.5f, 2.5f);
			GUILayout.EndHorizontal();
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteUInt(WrapAxis, "WrapAxis");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			WrapAxis = reader.ReadUInt("WrapAxis");
		}
	}
}