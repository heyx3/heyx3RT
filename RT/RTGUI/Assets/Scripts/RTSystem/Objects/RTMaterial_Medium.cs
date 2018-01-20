using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using RT.MaterialValue;

namespace RT
{
	[ExecuteInEditMode]
	public class RTMaterial_Medium : RTMaterial
	{
		public override string TypeName {  get { return TypeName_Medium; } }
		protected override string GraphSerializationName { get { return "Albedo"; } }


		public MV_Base Albedo { get { return Graph.GetRootNode(0); }
								set { Graph.ConnectInput(null, 0, value); } }


		protected override void InitGraph()
		{
			Albedo = MaterialValue.MV_Constant.MakeRGB(Color.white);
		}


		protected override string GenerateShader(string shaderName, Graph tempGraph, List<MV_Base> outTopLevelMVs)
		{
			try
			{
				MV_Base color = tempGraph.GetRootNode(0);

				MV_Base density = null;
				var myShape = GetComponent<RTShape_ConstantMedium>();
				if (myShape is RTShape_ConstantMedium)
					density = MV_Constant.MakeFloat(((RTShape_ConstantMedium)myShape).Density);
				else
					density = MV_Constant.MakeFloat(1.0f);

				tempGraph.AddNode(color);
				tempGraph.AddNode(density);

				return ShaderGenerator.GenerateShader_Fog(shaderName, tempGraph.UniqueNodeIDs,
														  color, density);
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to create shader \"" + shaderName + "\": " +
							       e.Message + "\n" + e.StackTrace);
				return null;
			}
		}

		public override string GetRootNodeDisplayName(int rootNodeIndex)
		{
			return "Albedo (rgb)";
		}
	}
}
