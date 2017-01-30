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


		public MaterialValue.MV_Base Albedo { get { return Graph.RootValues[0]; }
											  set { Graph.RootValues[0] = value; } }


		public override void Start()
		{
			//Albedo.
			Graph.RootValues.Add(MaterialValue.MV_Constant.MakeFloat(1.0f));

			base.Start();
		}

		protected override void GetUnityMaterialOutputs(out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness,
														HashSet<MaterialValue.MV_Base> toDelete)
		{
			albedo = Albedo;
			metallic = MaterialValue.MV_Constant.MakeFloat(0.0f);
			smoothness = MaterialValue.MV_Constant.MakeFloat(0.5f);

			toDelete.Add(metallic);
			toDelete.Add(smoothness);
		}
		
		public override string GetRootNodeDisplayName(int rootNodeIndex)
		{
			return "Albedo (rgb)";
		}
	}
}