using System;
using System.Xml;
using UnityEngine;


namespace RT
{
	public class RTSkyMat_SimpleColor : RTSkyMat
	{
		public override string TypeName { get { return TypeName_SimpleColor; } }
		public override Material UnityMat { get { return RTSystem.Instance.SkyMat_SolidColor; } }


		public MaterialValue Color = MV_Constant.MakeRGB(0.75f, 0.75f, 1.0f);

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