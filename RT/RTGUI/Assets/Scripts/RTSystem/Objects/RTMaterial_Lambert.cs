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
		protected override string GraphSerializationName { get { return "Color"; } }


		public MaterialValue.MV_Base Albedo { get { return Graph.GetRootNode(0); }
											  set { Graph.ConnectInput(null, 0, value); } }

		
		protected override void InitGraph()
		{
			var albedo = MaterialValue.MV_Constant.MakeFloat(1.0f, true, 0.0f, 1.0f,
															 MaterialValue.OutputSizes.OneOrThree,
															 true);
			Graph.AddNode(albedo);
			Graph.ConnectInput(null, 0, albedo);
		}

		
		protected override string GenerateShader(string shaderName, MaterialValue.Graph tempGraph,
												 List<MaterialValue.MV_Base> outTopLevelMVs)
		{
			try
			{
				MaterialValue.MV_Base albedo, metallic, smoothness;

				albedo = tempGraph.GetRootNode(0);
				metallic = MaterialValue.MV_Constant.MakeFloat(0.0f);
				smoothness = MaterialValue.MV_Constant.MakeFloat(0.5f);

				tempGraph.AddNode(albedo);
				tempGraph.AddNode(metallic);
				tempGraph.AddNode(smoothness);

				return MaterialValue.ShaderGenerator.GenerateShader(shaderName, tempGraph.UniqueNodeIDs,
																	albedo, metallic, smoothness);
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