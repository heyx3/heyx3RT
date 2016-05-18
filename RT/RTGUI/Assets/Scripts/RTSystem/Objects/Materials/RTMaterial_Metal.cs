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


		public MaterialValue Albedo = new MV_Constant(true, new uint[] { 1, 3 }, 1.0f);
		public MaterialValue Roughness = new MV_Constant(true, new uint[] { 1 }, 0.2f);


		public override void DoGUI()
		{
			GUILayout.Label("Albedo");
			Albedo.OnGUI();

			GUILayout.Label("Roughness");
			Roughness.OnGUI();
		}

		public override void SetMaterialParams(Material mat)
		{
			mat.SetFloat("_Metallic", 1.0f);

			Albedo.SetMaterialParams(mat, "_MainTex", "_Color");
			Roughness.SetMaterialParams(mat, null, "_Glossiness");
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			MaterialValue.Write(Albedo, writer, "Albedo");
			MaterialValue.Write(Roughness, writer, "Roughness");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			Albedo = MaterialValue.Read(reader, "Albedo");
			Roughness = MaterialValue.Read(reader, "Roughness");
		}
	}
}