using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RT.MaterialValue;

namespace RT
{
	public class RTSkyMaterial_VerticalGradient : RTSkyMaterial
	{
		public override string TypeName { get { return TypeName_VerticalGradient; } }

		public override IEnumerable<KeyValuePair<string, MaterialValue.MV_Base>> Outputs
		{
			get
			{
				yield return new KeyValuePair<string, MV_Base>("Bottom Color", BottomColor);
				yield return new KeyValuePair<string, MV_Base>("Top Color", TopColor);
				yield return new KeyValuePair<string, MV_Base>("Sky Direction", SkyDir);
			}
		}

		
		public MV_Base BottomColor = MV_Constant.MakeRGB(UnityEngine.Color.black),
					   TopColor = MV_Constant.MakeRGB(UnityEngine.Color.cyan),
					   SkyDir = MV_Constant.MakeVec3(0.0f, 1.0f, 0.0f,
													 0.0f, 1.0f, MaterialValue.OutputSizes.One);


		protected override void GetUnityMaterialOutputs(out MV_Base outRGB)
		{
			MV_Base dotRaySky = MV_Simple2.Dot(MV_Simple1.Normalize(SkyDir),
											   MV_Inputs.RayDir);
			MV_Base drs0To1 = MV_Arithmetic.Add(MV_Constant.MakeFloat(0.5f),
												MV_Arithmetic.Multiply(MV_Constant.MakeFloat(0.5f),
																	   dotRaySky));
			outRGB = MV_Simple3.Lerp(BottomColor, TopColor, drs0To1);
		}
		
		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);
			MV_Base.Serialize(BottomColor, "BottomCol", writer);
			MV_Base.Serialize(TopColor, "TopCol", writer);
			MV_Base.Serialize(SkyDir, "SkyDir", writer);
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);
			BottomColor = MV_Base.Deserialize("BottomCol", reader);
			TopColor = MV_Base.Deserialize("TopCol", reader);
			SkyDir = MV_Base.Deserialize("SkyDir", reader);
		}
	}
}