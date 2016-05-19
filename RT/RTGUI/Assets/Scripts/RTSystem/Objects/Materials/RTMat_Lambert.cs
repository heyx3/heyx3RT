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


		public MaterialValue Color = new MV_Constant(true, new uint[] { 1, 3 }, 1.0f);

		private RTGui.MaterialValueGui colorGUI;


		protected override void Start()
		{
			base.Start();

			colorGUI = new RTGui.MaterialValueGui("Color",
												  RTGui.Gui.Instance.Style_Button,
												  RTGui.Gui.Instance.Style_Text);
		}

		public override void DoGUI()
		{
			Color = colorGUI.DoGUI(Color, false);
		}

		public override void SetMaterialParams(Material mat)
		{
			mat.SetFloat("_Metallic", 0.0f);
			Color.SetMaterialParams(mat, "_MainTex", "_Color");
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