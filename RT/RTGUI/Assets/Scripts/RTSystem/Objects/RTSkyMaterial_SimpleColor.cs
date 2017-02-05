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

		public MaterialValue.MV_Base Color { get { return Graph.RootValues[0]; }
											 set { Graph.RootValues[0] = value; } }


		public override void Awake()
		{
            base.Awake();

			Graph.RootValues.Add(MaterialValue.MV_Constant.MakeRGB(UnityEngine.Color.cyan));
		}

		protected override void GetUnityMaterialOutputs(out MaterialValue.MV_Base outRGB,
														HashSet<MaterialValue.MV_Base> toDelete)
		{
			outRGB = Color;
		}

		public override string GetRootNodeDisplayName(int rootNodeIndex)
		{
			return "Color (rgb)";
		}
	}
}