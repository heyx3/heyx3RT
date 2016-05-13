using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;


namespace RT
{
	public class RTMat_Metal : RTMat
	{
		public override string TypeName { get { return TypeName_Metal; } }
		public override Material UnityMat { get { return RTSystem.Instance.Mat_Metal; } }


		//TODO: Use MaterialValues here and in Lambert.

		public Vector3 Albedo = Vector3.one;
		public float Roughness = 0.2f;


		public override void DoGUI()
		{
			GUILayout.Label("Albedo");
			Albedo = GUIUtil.RGBEditor(Albedo, 10.0f);

			GUILayout.EndHorizontal();
				GUILayout.Label("Roughness");
				Roughness = GUILayout.HorizontalSlider(Roughness, 0.0f, 1.0f);
			GUILayout.BeginHorizontal();
		}

		public override void SetMaterialParams(Material mat)
		{
			mat.color = Albedo.ToCol();
			mat.SetFloat("_Shininess", 1.0f - Roughness);
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteVector3(Albedo, "Albedo");
			writer.WriteFloat(Roughness, "Roughness");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			reader.ReadVector3((v) => Albedo = v, "Albedo");
			Roughness = reader.ReadFloat("Roughness");
		}
	}
}