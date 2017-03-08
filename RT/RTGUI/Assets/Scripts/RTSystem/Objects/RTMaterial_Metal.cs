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
		protected override string GraphSerializationName { get { return "Albedo_Roughness_Emissive"; } }


		public MaterialValue.MV_Base Albedo { get { return Graph.GetRootNode(0); }
											  set { Graph.ConnectInput(null, 0, value); } }
		public MaterialValue.MV_Base Roughness { get { return Graph.GetRootNode(1); }
												 set { Graph.ConnectInput(null, 1, value); } }
		public MaterialValue.MV_Base Emissive { get { return Graph.GetRootNode(2); }
												set { Graph.ConnectInput(null, 2, value); } }

		
		protected override void InitGraph()
		{
			var albedo = MaterialValue.MV_Constant.MakeRGB(Color.white);
			Graph.AddNode(albedo);
			Graph.ConnectInput(null, 0, albedo);

			var roughness = MaterialValue.MV_Constant.MakeFloat(0.15f, true, 0.0f, 1.0f,
																MaterialValue.OutputSizes.One, true);
			Graph.AddNode(roughness);
			Graph.ConnectInput(null, 1, roughness);

			var emissive = MaterialValue.MV_Constant.MakeRGB(Color.black);
			Graph.AddNode(emissive);
			Graph.ConnectInput(null, 2, emissive);
		}
		
		protected override string GenerateShader(string shaderName, MaterialValue.Graph tempGraph,
												 List<MaterialValue.MV_Base> outTopLevelMVs)
		{
			try
			{
				MaterialValue.MV_Base albedo, metallic, smoothness, emissive;

				albedo = tempGraph.GetRootNode(0);
				emissive = tempGraph.GetRootNode(2);

				var constant1 = MaterialValue.MV_Constant.MakeFloat(1.0f);
				metallic = constant1;
				smoothness = MaterialValue.MV_Arithmetic.Subtract(constant1, tempGraph.GetRootNode(1));

				tempGraph.AddNode(albedo);
				tempGraph.AddNode(metallic);
				tempGraph.AddNode(smoothness);
				tempGraph.AddNode(emissive);

				outTopLevelMVs.Add(albedo);
				outTopLevelMVs.Add(metallic);
				outTopLevelMVs.Add(smoothness);
				outTopLevelMVs.Add(emissive);

				return MaterialValue.ShaderGenerator.GenerateShader(shaderName, tempGraph.UniqueNodeIDs,
																	albedo, metallic, smoothness, emissive);
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
			switch (rootNodeIndex)
			{
				case 0: return "Albedo (rgb)";
				case 1: return "Roughness (scalar)";
				case 2: return "Emissive (rgb)";
				default: throw new NotImplementedException(rootNodeIndex.ToString());
			}
		}
	}
}