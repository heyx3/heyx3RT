using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace RT
{
	[ExecuteInEditMode]
	public class RTSkyMaterial_SimpleColor : RTSkyMaterial
	{
		public override string TypeName { get { return TypeName_SimpleColor; } }

		public override IEnumerable<KeyValuePair<string, MaterialValue.MV_Base>> Outputs
		{
			get
			{
				yield return new KeyValuePair<string, MaterialValue.MV_Base>("Color", Color);
			}
		}


		public MaterialValue.MV_Base Color = MaterialValue.MV_Constant.MakeRGB(UnityEngine.Color.cyan);


		protected override void GetUnityMaterialOutputs(out MaterialValue.MV_Base outRGB,
														HashSet<MaterialValue.MV_Base> toDelete)
		{
			outRGB = Color;
		}

		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);

			var graph = new MaterialValue.Graph(new List<MaterialValue.MV_Base>() { Color });
			writer.Structure(graph, "Color");
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);

			var graph = new MaterialValue.Graph();
			reader.Structure(graph, "Color");

			Color = graph.RootValues[0];
		}
	}
}