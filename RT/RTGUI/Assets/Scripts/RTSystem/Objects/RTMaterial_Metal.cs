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


		public MaterialValue.MV_Base Albedo { get { return Graph.RootValues[0]; }
											  set { Graph.RootValues[0] = value; } }
		public MaterialValue.MV_Base Roughness { get { return Graph.RootValues[1]; }
												 set { Graph.RootValues[1] = value; } }


		public override void Start()
		{
			//Albedo.
			Graph.RootValues.Add(MaterialValue.MV_Constant.MakeFloat(1.0f));
			//Roughness.
			Graph.RootValues.Add(MaterialValue.MV_Constant.MakeFloat(0.5f));

			base.Start();
		}

		protected override void GetUnityMaterialOutputs(out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness,
														HashSet<MaterialValue.MV_Base> toDelete)
		{
			albedo = Albedo;

			metallic = MaterialValue.MV_Constant.MakeFloat(1.0f);
			toDelete.Add(metallic);

			var constant1 = MaterialValue.MV_Constant.MakeFloat(1.0f);
			smoothness = MaterialValue.MV_Arithmetic.Subtract(constant1, Roughness);
			toDelete.Add(constant1);
			toDelete.Add(smoothness);
		}
		
		public override string GetRootNodeDisplayName(int rootNodeIndex)
		{
			return (rootNodeIndex == 0 ? "Albedo (rgb)" : "Roughness (scalar)");
		}
	}
}