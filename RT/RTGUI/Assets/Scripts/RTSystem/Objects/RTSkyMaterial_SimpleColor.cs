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
		protected override string GraphSerializationName { get { return "Color"; } }

		public MaterialValue.MV_Base Color { get { return Graph.GetRootNode(0); }
											 set { Graph.ConnectInput(null, 0, value); } }

		
		protected override void InitGraph()
		{
			var col = MaterialValue.MV_Constant.MakeRGB(new UnityEngine.Color(0.5f, 0.5f, 1.0f));
			Graph.AddNode(col);
			Graph.ConnectInput(null, 0, col);
		}

		protected override void GetUnityMaterialOutputs(out MaterialValue.MV_Base outRGB)
		{
			outRGB = Color;
		}

		public override string GetRootNodeDisplayName(int rootNodeIndex)
		{
			return "Color (rgb)";
		}
	}
}