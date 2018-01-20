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
		protected override string GraphSerializationName { get { return "Albedo_Emissive"; } }

		
		public MaterialValue.MV_Base Albedo { get { return Graph.GetRootNode(0); }
											  set { Graph.ConnectInput(null, 0, value); } }
		public MaterialValue.MV_Base Emissive { get { return Graph.GetRootNode(1); }
												set { Graph.ConnectInput(null, 1, value); } }


		protected override void InitGraph()
		{
			Albedo = MaterialValue.MV_Constant.MakeRGB(Color.white);
			Emissive = MaterialValue.MV_Constant.MakeRGB(Color.black);
		}

		
		protected override string GenerateShader(string shaderName, MaterialValue.Graph tempGraph,
												 List<MaterialValue.MV_Base> outTopLevelMVs)
		{
			try
			{
				MaterialValue.MV_Base albedo, metallic, smoothness, emissive;

				albedo = tempGraph.GetRootNode(0);
				metallic = MaterialValue.MV_Constant.MakeFloat(0.0f);
				smoothness = MaterialValue.MV_Constant.MakeFloat(0.5f);
				emissive = tempGraph.GetRootNode(1);

				tempGraph.AddNode(albedo);
				tempGraph.AddNode(metallic);
				tempGraph.AddNode(smoothness);
				tempGraph.AddNode(emissive);

				return MaterialValue.ShaderGenerator.GenerateShader_PBR(shaderName,
																		tempGraph.UniqueNodeIDs,
																	    albedo, metallic, smoothness,
																		emissive);
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
				case 1: return "Emissive (rgb)";
				default: throw new NotImplementedException(rootNodeIndex.ToString());
			}
		}
	}
}