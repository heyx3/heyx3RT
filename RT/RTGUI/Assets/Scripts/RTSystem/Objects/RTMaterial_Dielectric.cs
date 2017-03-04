using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

using RT.MaterialValue;


//TODO: Use Glass shader from StandardAssets folder.

namespace RT
{
	[ExecuteInEditMode]
	public class RTMaterial_Dielectric : RTMaterial
	{
		public override string TypeName { get { return TypeName_Dielectric; } }
		protected override string GraphSerializationName { get { return "IndexOfRefraction"; } }


		public MV_Base IndexOfRefraction { get { return Graph.GetRootNode(0); }
										   set { Graph.ConnectInput(null, 0, value); } }

		
		protected override void InitGraph()
		{
			var IoR = MV_Constant.MakeFloat(1.0f, true, 1.0f, 10.0f,
											OutputSizes.One, true);
			Graph.AddNode(IoR);
			Graph.ConnectInput(null, 0, IoR);
		}

		
		protected override string GenerateShader(string shaderName, Graph tempGraph,
												 List<MV_Base> outTopLevelMVs)
		{
			try
			{
				MV_Constant one = MV_Constant.MakeFloat(1.0f);
				MV_Base albedo, refractStrength;

				albedo = one;

				//Make the distortion amount equal to:
				//    lerp(0, 128, 1 - (1 / refractionIndex))
				var t = MV_Arithmetic.Subtract(one, MV_Arithmetic.Divide(one, tempGraph.GetRootNode(0)));
				refractStrength = MV_Simple3.Lerp(MV_Constant.MakeFloat(0.0f),
												  MV_Constant.MakeFloat(128.0f),
												  t);

				outTopLevelMVs.Add(albedo);
				outTopLevelMVs.Add(refractStrength);

				tempGraph.AddNode(albedo);
				tempGraph.AddNode(refractStrength);

				return ShaderGenerator.GenerateShader(shaderName, tempGraph.UniqueNodeIDs,
													  albedo, refractStrength);
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
			return "Index of Refraction (scalar)";
		}
	}
}