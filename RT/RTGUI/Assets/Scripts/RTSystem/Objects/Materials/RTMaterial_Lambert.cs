using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;



namespace RT
{
	public class RTMat_Lambert : RTMat
	{
		public override string TypeName { get { return TypeName_Lambert; } }
		public override Material UnityMat { get { return RTSystem.Instance.Mat_Lambert; } }


		public Vector3 Color = Vector3.one;


		public override void DoGUI()
		{
			GUILayout.Label("Color");
			Color = GUIUtil.RGBEditor(Color, 10.0f);
		}

		public override void SetMaterialParams(Material mat)
		{
			mat.color = Color.ToCol();
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteVector3(Color, "Color");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			reader.ReadVector3((v) => Color = v, "Color");
		}
	}
}