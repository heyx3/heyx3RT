using System;
using System.Xml;
using UnityEngine;


namespace RT
{
	public class RTSkyMat_VerticalGradient : RTSkyMat
	{
		public override string TypeName { get { return TypeName_VerticalGradient; } }
		public override Material UnityMat { get { return RTSystem.Instance.SkyMat_VerticalGradient; } }


		public MaterialValue TopColor = MV_Constant.MakeRGB(0.95f, 0.95f, 1.0f),
							 BottomColor = MV_Constant.MakeRGB(0.5f, 0.5f, 0.75f),
							 SkyDir = MV_Constant.MakeVec3(Vector3.up);

		private RTGui.MaterialValueGui topColorGUI, bottomColorGUI, skyDirGUI;


		protected override void Start()
		{
			base.Start();

			topColorGUI = new RTGui.MaterialValueGui("Top Color",
													 RTGui.Gui.Instance.Style_Button,
													 RTGui.Gui.Instance.Style_Text);
			bottomColorGUI = new RTGui.MaterialValueGui("Bottom Color",
														RTGui.Gui.Instance.Style_Button,
														RTGui.Gui.Instance.Style_Text);
			skyDirGUI = new RTGui.MaterialValueGui("Sky Direction",
												   RTGui.Gui.Instance.Style_Button,
												   RTGui.Gui.Instance.Style_Text);
		}

		public override void DoGUI()
		{
			TopColor = topColorGUI.DoGUI(TopColor, false);
			BottomColor = bottomColorGUI.DoGUI(BottomColor, false);
			SkyDir = skyDirGUI.DoGUI(SkyDir, false);
		}
		public override void SetMaterialParams(Material m)
		{
			TopColor.SetMaterialParams(m, null, "_TopCol");
			BottomColor.SetMaterialParams(m, null, "_BottomCol");
			SkyDir.SetMaterialParams(m, null, "_SkyDir");
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			MaterialValue.Write(BottomColor, writer, "BottomCol");
			MaterialValue.Write(TopColor, writer, "TopCol");
			MaterialValue.Write(SkyDir, writer, "SkyDir");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			BottomColor = MaterialValue.Read(reader, "BottomCol");
			TopColor = MaterialValue.Read(reader, "TopCol");
			SkyDir = MaterialValue.Read(reader, "SkyDir");
		}
	}
}