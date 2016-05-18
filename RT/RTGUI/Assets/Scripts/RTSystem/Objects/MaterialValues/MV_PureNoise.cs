using System;
using System.Collections.Generic;
using UnityEngine;


namespace RT
{
	public class MV_PureNoise : MaterialValue
	{
		public byte NChannels;


		public override string TypeName { get { return TypeName_PureNoise; } }


		public MV_PureNoise(byte nChannels) { NChannels = nChannels; }


		protected override void DoGUI()
		{
			base.DoGUI();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Number of Channels:", Gui.Style_MaterialValue_Text);
			NChannels = (byte)GUILayout.HorizontalSlider((float)NChannels, 1.0f, 4.0f,
														 Gui.Style_MaterialValue_Slider,
														 Gui.Style_MaterialValue_SliderThumb);
			GUILayout.EndHorizontal();
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);
			writer.WriteByte(NChannels, "Dimensions");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);
			NChannels = reader.ReadByte("Dimensions");
		}
	}
}