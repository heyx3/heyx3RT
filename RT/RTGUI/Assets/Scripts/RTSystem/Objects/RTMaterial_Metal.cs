using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace RT
{
	[ExecuteInEditMode]
	public class RTMaterial_Metal : RTMaterial
	{
		public override string TypeName { get { return TypeName_Metal; } }
		protected override string GraphSerializationName { get { return "Albedo_Roughness"; } }


		public MaterialValue.MV_Base Albedo { get { return Graph.GetRootNode(0); }
											  set { Graph.ConnectInput(null, 0, value); } }
		public MaterialValue.MV_Base Roughness { get { return Graph.GetRootNode(1); }
												 set { Graph.ConnectInput(null, 1, value); } }

		
		protected override void InitGraph()
		{
			var albedo = MaterialValue.MV_Constant.MakeFloat(1.0f, true, 0.0f, 1.0f,
															 MaterialValue.OutputSizes.OneOrThree,
															 true);
			Graph.AddNode(albedo);
			Graph.ConnectInput(null, 0, albedo);

			var roughness = MaterialValue.MV_Constant.MakeFloat(0.15f, true, 0.0f, 1.0f,
																MaterialValue.OutputSizes.One, true);
			Graph.AddNode(roughness);
			Graph.ConnectInput(null, 1, roughness);
		}
		
		protected override string GenerateShader(string shaderName, MaterialValue.Graph tempGraph,
												 List<MaterialValue.MV_Base> outTopLevelMVs)
		{
			try
			{
				MaterialValue.MV_Base albedo, metallic, smoothness;

				albedo = tempGraph.GetRootNode(0);
				var constant1 = MaterialValue.MV_Constant.MakeFloat(1.0f);
				metallic = constant1;
				smoothness = MaterialValue.MV_Arithmetic.Subtract(constant1, tempGraph.GetRootNode(1));

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
			return (rootNodeIndex == 0 ? "Albedo (rgb)" : "Roughness (scalar)");
		}
	}
}