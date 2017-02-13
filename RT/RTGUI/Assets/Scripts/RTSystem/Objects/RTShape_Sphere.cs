using RT.Serialization;
using UnityEngine;


namespace RT
{
	[ExecuteInEditMode]
	public class RTShape_Sphere : RTShape
	{
		public enum WrapAxes
		{
			X = 0,
			Y = 1,
			Z = 2,
		}

		public WrapAxes WrapAxis = WrapAxes.X;


		public override string TypeName { get { return TypeName_Sphere; } }
		public override UnityEngine.Mesh UnityMesh { get { return RTSystem.Instance.Shape_Sphere; } }

		public override void WriteData(DataWriter writer)
		{
			base.WriteData(writer);

			writer.Byte((byte)WrapAxis, "WrapAxis");
		}
		public override void ReadData(DataReader reader)
		{
			base.ReadData(reader);

			WrapAxis = (WrapAxes)reader.Byte("WrapAxis");
		}

		private void OnValidate()
		{
			//TODO: Rotate sphere to match wrapping axis.
		}
	}
}