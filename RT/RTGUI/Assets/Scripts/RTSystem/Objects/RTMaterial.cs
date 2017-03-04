using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using RT.MaterialValue;

namespace RT
{
	[ExecuteInEditMode]
	public abstract class RTMaterial : RTBaseMaterial, Serialization.ISerializableRT
	{
		protected const string TypeName_Lambert = "Lambert",
		                       TypeName_Metal = "Metal",
							   TypeName_Dielectric = "Dielectric";


		public static void Serialize(RTMaterial mat, string name, Serialization.DataWriter writer)
		{
			writer.String(mat.TypeName, name + "Type");
			writer.Structure(mat, name + "Value");
		}
		public static RTMaterial Deserialize(GameObject toAttachTo, Serialization.DataReader reader, string name)
		{
			RTMaterial mat = toAttachTo.GetComponent<RTMaterial>();
			if (mat != null)
				DestroyImmediate(mat);

			string typeName = reader.String(name + "Type");
			switch (typeName)
			{
				case TypeName_Lambert:
					mat = toAttachTo.AddComponent<RTMaterial_Lambert>();
					break;
				case TypeName_Metal:
					mat = toAttachTo.AddComponent<RTMaterial_Metal>();
					break;
				case TypeName_Dielectric:
					mat = toAttachTo.AddComponent<RTMaterial_Dielectric>();
					break;
					
				default:
					throw new Serialization.DataReader.ReadException("Unknown material type: " + typeName);
			}

			reader.Structure(mat, name + "Value");

			return mat;
		}
		
		public abstract string TypeName { get; }


		private Vector3 lastPos = Vector3.zero;
		private Vector3 lastScale = Vector3.one;
		private Quaternion lastRot = Quaternion.identity;
		private void Update()
		{
			Transform tr = transform;
			if (tr.position != lastPos || tr.lossyScale != lastScale || tr.rotation != lastRot)
			{
				ShaderGenerator.SetMaterialParams(tr, GetComponent<Renderer>().sharedMaterial, 
												  GraphCopy.UniqueNodeIDs, OutTopLevelMVs.ToArray());

				//Remember this position/scale/rotation.
				lastPos = tr.position;
				lastScale = tr.lossyScale;
				lastRot = tr.rotation;
			}
		}
	}
}