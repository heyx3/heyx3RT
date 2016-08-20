using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


namespace RT
{
	public class RTMaterial_Metal : RTMaterial
	{
		public override string TypeName { get { return TypeName_Metal; } }

		public override IEnumerable<KeyValuePair<string, MaterialValue.MV_Base>> Outputs
		{
			get
			{
				yield return new KeyValuePair<string, MaterialValue.MV_Base>("Albedo", Albedo);
				yield return new KeyValuePair<string, MaterialValue.MV_Base>("Roughness", Roughness);
			}
		}


		public MaterialValue.MV_Base Albedo = MaterialValue.MV_Constant.MakeFloat(1.0f),
									 Roughness = MaterialValue.MV_Constant.MakeFloat(0.5f);


		protected override void GetUnityMaterialOutputs(out MaterialValue.MV_Base albedo,
														out MaterialValue.MV_Base metallic,
														out MaterialValue.MV_Base smoothness)
		{
			albedo = Albedo;
			metallic = MaterialValue.MV_Constant.MakeFloat(1.0f);
			smoothness = MaterialValue.MV_Arithmetic.Subtract(MaterialValue.MV_Constant.MakeFloat(1.0f),
															  Roughness);
		}

		public override void WriteData(Serialization.DataWriter writer)
		{
			base.WriteData(writer);
			MaterialValue.MV_Base.Serialize(Albedo, "Albedo", writer);
			MaterialValue.MV_Base.Serialize(Roughness, "Roughness", writer);
		}
		public override void ReadData(Serialization.DataReader reader)
		{
			base.ReadData(reader);
			Albedo = MaterialValue.MV_Base.Deserialize("Albedo", reader);
			Roughness = MaterialValue.MV_Base.Deserialize("Roughness", reader);
		}
	}
}