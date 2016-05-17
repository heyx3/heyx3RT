using System;
using System.Xml;
using UnityEngine;


namespace RT
{
	public class RTSkyMat_SimpleColor : RTSkyMat
	{
		public override string TypeName { get { return TypeName_SimpleColor; } }
		public override Material UnityMat { get { return RTSystem.Instance.SkyMat_SolidColor; } }


		public MaterialValue Color = new MV_Constant(true, new uint[] { 1, 3 }, new Vector3(0.75f, 0.75f, 1.0f));


		public override void DoGUI()
		{
			GUILayout.Label("Color");
			Color.OnGUI(RTGui.Instance.MaterialValueTabSize);
		}

		public override void SetMaterialParams(Material m)
		{
			Color.SetMaterialParams(m, null, "_Color");
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			MaterialValue.Write(Color, writer, "Color");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			Color = MaterialValue.Read(reader, "Color");
		}
	}
}