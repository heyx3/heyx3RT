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

		private RTGui.MaterialValueGui albedoGUI, roughnessGUI;


		protected override void Start()
		{
			base.Start();
			
			albedoGUI = new RTGui.MaterialValueGui("Albedo",
												   RTGui.Gui.Instance.Style_Button,
												   RTGui.Gui.Instance.Style_Text);
			roughnessGUI = new RTGui.MaterialValueGui("Roughness",
													  RTGui.Gui.Instance.Style_Button,
													  RTGui.Gui.Instance.Style_Text);
		}

		public override void DoGUI()
		{
			Albedo = albedoGUI.DoGUI(Albedo, false);
			Roughness = roughnessGUI.DoGUI(Roughness, false);
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