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

		public override IEnumerable<KeyValuePair<string, MaterialValue.MV_Base>> Outputs
		{
			get
			{
				yield return new KeyValuePair<string, MaterialValue.MV_Base>("Albedo", Albedo);
			}
		}


		public MaterialValue.MV_Base Albedo = MaterialValue.MV_Constant.MakeFloat(1.0f);


		protected override void GetUnityMaterialOutputs(out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness)
		{
			albedo = Albedo;
			metallic = MaterialValue.MV_Constant.MakeFloat(0.0f);
			smoothness = MaterialValue.MV_Constant.MakeFloat(0.5f);
		}

		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);
			MaterialValue.MV_Base.Serialize(Albedo, "Color", writer);
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);
			Albedo = MaterialValue.MV_Base.Deserialize("Color", reader);
		}
	}
}