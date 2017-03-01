using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


//TODO: Use Glass shader from StandardAssets folder.

namespace RT
{
	[ExecuteInEditMode]
	public class RTMaterial_Dielectric : RTMaterial
	{
		public override string TypeName { get { return TypeName_Dielectric; } }
		protected override string GraphSerializationName { get { return "IndexOfRefraction"; } }


		public MaterialValue.MV_Base IndexOfRefraction { get { return Graph.GetRootNode(0); }
														 set { Graph.ConnectInput(null, 0, value); } }

		
		protected override void InitGraph()
		{
			var IoR = MaterialValue.MV_Constant.MakeFloat(1.0f, true, 1.0f, 10.0f,
														  MaterialValue.OutputSizes.One,
														  true);
			Graph.AddNode(IoR);
			Graph.ConnectInput(null, 0, IoR);
		}


		protected override void GetUnityMaterialOutputs(MaterialValue.Graph tempGraph,
														out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness)
		{
			albedo = tempGraph.GetRootNode(0);
			metallic = MaterialValue.MV_Constant.MakeFloat(0.5f);
			smoothness = MaterialValue.MV_Constant.MakeFloat(0.1f);
		}
		
		public override string GetRootNodeDisplayName(int rootNodeIndex)
		{
			return "Index of Refraction (scalar)";
		}
	}
}