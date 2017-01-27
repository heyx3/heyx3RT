using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RT.MaterialValue;

namespace RT
{
	[ExecuteInEditMode]
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


		protected override void GetUnityMaterialOutputs(out MV_Base outRGB, HashSet<MV_Base> toDelete)
		{
			MV_Base normalizedSkyDir = MV_Simple1.Normalize(SkyDir),
					dotRaySky = MV_Simple2.Dot(normalizedSkyDir, MV_Inputs.RayDir),
					constantHalf = MV_Constant.MakeFloat(0.5f),
					constantHalf2 = MV_Constant.MakeFloat(0.5f),
					halfRaySky = MV_Arithmetic.Multiply(constantHalf, dotRaySky),
					dotRaySkyMapped = MV_Arithmetic.Add(constantHalf2, halfRaySky);

			toDelete.Add(normalizedSkyDir);
			toDelete.Add(dotRaySky);
			toDelete.Add(constantHalf);
			toDelete.Add(constantHalf2);
			toDelete.Add(halfRaySky);
			toDelete.Add(dotRaySkyMapped);

			outRGB = MV_Simple3.Lerp(BottomColor, TopColor, dotRaySkyMapped);
			toDelete.Add(outRGB);
		}
		
		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);

			var graph = new MaterialValue.Graph(new List<MV_Base>() { BottomColor, TopColor, SkyDir });
			writer.Structure(graph, "BottomCol_TopCol_SkyDir");
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);

			var graph = new MaterialValue.Graph();
			reader.Structure(graph, "BottomCol_TopCol_SkyDir");

			BottomColor = graph.RootValues[0];
			TopColor = graph.RootValues[1];
			SkyDir = graph.RootValues[2];
		}
	}
}