using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RT.MaterialValue;

namespace RT
{
	[ExecuteInEditMode]
	public class RTSkyMaterial_VerticalGradient : RTSkyMaterial
	{
		public override string TypeName { get { return TypeName_VerticalGradient; } }
		protected override string GraphSerializationName { get { return "BottomCol_TopCol_SkyDir"; } }


		public MV_Base BottomColor { get { return Graph.RootValues[0]; }
									 set { Graph.RootValues[0] = value; } }
		public MV_Base TopColor { get { return Graph.RootValues[1]; }
								  set { Graph.RootValues[1] = value; } }
		public MV_Base SkyDir { get { return Graph.RootValues[2]; }
								set { Graph.RootValues[2] = value; } }


		protected override void GetUnityMaterialOutputs(out MV_Base outRGB, HashSet<MV_Base> toDelete)
		{
			MV_Base normalizedSkyDir = MV_Simple1.Normalize(SkyDir),
					dotRaySky = MV_Simple2.Dot(normalizedSkyDir, MV_Inputs.RayDir),
					constantHalf = MV_Constant.MakeFloat(0.5f),
					constantHalf2 = MV_Constant.MakeFloat(0.5f),
					halfRaySky = MV_Arithmetic.Multiply(constantHalf, dotRaySky),
					dotRaySkyMapped = MV_Arithmetic.Add(constantHalf2, halfRaySky);

			toDelete.Add(normalizedSkyDir);
			toDelete.Add(dotRaySky);
			toDelete.Add(constantHalf);
			toDelete.Add(constantHalf2);
			toDelete.Add(halfRaySky);
			toDelete.Add(dotRaySkyMapped);

			outRGB = MV_Simple3.Lerp(BottomColor, TopColor, dotRaySkyMapped);
			toDelete.Add(outRGB);
		}

		protected override void InitGraph()
		{
			//Bottom color.
			Graph.RootValues.Add(MaterialValue.MV_Constant.MakeRGB(new Color(0.2f, 0.4f, 0.8f)));
			//Top color.
			Graph.RootValues.Add(MaterialValue.MV_Constant.MakeRGB(new Color(0.7f, 0.7f, 1.0f)));
			//Sky dir.
			Graph.RootValues.Add(MV_Constant.MakeVec3(0.0f, 1.0f, 0.0f,
													  0.0f, 1.0f, OutputSizes.Three));
		}

		public override string GetRootNodeDisplayName(int rootNodeIndex)
		{
			return (rootNodeIndex == 0 ?
						"Bottom Color (rgb)" :
						(rootNodeIndex == 1 ?
							"Top Color (rgb)" :
							"Sky Direction (xyz)"));
		}
	}
}