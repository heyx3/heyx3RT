using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace RT
{
	[ExecuteInEditMode]
	public class RTMaterial_Lambert : RTMaterial
	{
		public override string TypeName { get { return TypeName_Lambert; } }

		public override IEnumerable<KeyValuePair<string, MaterialValue.MV_Base>> Outputs
		{
			get
			{
				yield return new KeyValuePair<string, MaterialValue.MV_Base>("Albedo", Albedo);
			}
		}


		public MaterialValue.MV_Base Albedo = MaterialValue.MV_Constant.MakeFloat(1.0f);


		protected override void GetUnityMaterialOutputs(out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness,
														HashSet<MaterialValue.MV_Base> toDelete)
		{
			albedo = Albedo;
			metallic = MaterialValue.MV_Constant.MakeFloat(0.0f);
			smoothness = MaterialValue.MV_Constant.MakeFloat(0.5f);

			toDelete.Add(metallic);
			toDelete.Add(smoothness);
		}

		
		private static readonly string Name_Albedo = "Albedo (rgb)";
		public override void GetMVs(Dictionary<string, MaterialValue.MV_Base> outVals)
		{
			outVals.Add(Name_Albedo, Albedo);
		}
		public override void SetMVs(Dictionary<string, MaterialValue.MV_Base> newVals)
		{
			Albedo = newVals[Name_Albedo];
		}
		
		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);

			var graph = new MaterialValue.Graph(new List<MaterialValue.MV_Base>() { Albedo });
			writer.Structure(graph, "Color");
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);

			var graph = new MaterialValue.Graph();
			reader.Structure(graph, "Color");

			Albedo = graph.RootValues[0];
		}
	}
}