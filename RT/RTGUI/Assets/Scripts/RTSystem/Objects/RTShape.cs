using System.Xml;
using System.Collections.Generic;
using UnityEngine;


namespace RT
{
	[ExecuteInEditMode]
	public abstract class RTShape : MonoBehaviour, Serialization.ISerializableRT
	{
		protected const string TypeName_Sphere = "Sphere",
		                       TypeName_Plane = "Plane",
		                       TypeName_Mesh = "Mesh",
							   TypeName_ConstantMedium = "ConstantMedium";


		public static void Serialize(RTShape shpe, string name, Serialization.DataWriter writer)
		{
			writer.String(shpe.TypeName, name + "Type");
			writer.Structure(shpe, name + "Value");
		}
		public static RTShape Deserialize(GameObject toAttachTo, Serialization.DataReader reader, string name)
		{
			RTShape shpe = toAttachTo.GetComponent<RTShape>();
			if (shpe != null)
				DestroyImmediate(shpe);

			string typeName = reader.String(name + "Type");
			switch (typeName)
			{
				case TypeName_Sphere:
				    shpe = toAttachTo.AddComponent<RTShape_Sphere>();
					break;
				case TypeName_Plane:
				    shpe = toAttachTo.AddComponent<RTShape_Plane>();
					break;
				case TypeName_Mesh:
				    shpe = toAttachTo.AddComponent<RTShape_Mesh>();
					break;
				case TypeName_ConstantMedium:
					shpe = toAttachTo.AddComponent<RTShape_ConstantMedium>();
					break;

				default:
					throw new Serialization.DataReader.ReadException("Unknown shape type: " + typeName);
			}

			reader.Structure(shpe, name + "Value");

			return shpe;
		}


		public RTMaterial Mat { get { return GetComponent<RTMaterial>(); } }

		public abstract string TypeName { get; }
		public abstract Mesh UnityMesh { get; }


		public virtual void Awake()
		{
			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf == null)
				mf = gameObject.AddComponent<MeshFilter>();
			mf.sharedMesh = UnityMesh;

			gameObject.isStatic = true;
		}
		
		public virtual void WriteData(Serialization.DataWriter writer)
		{
			TransformSerializationWrapper trnsf = new TransformSerializationWrapper(transform);
			writer.Structure(trnsf, "Transform");
			writer.String(gameObject.name, "GameObjName");
		}
		public virtual void ReadData(Serialization.DataReader reader)
		{
			TransformSerializationWrapper trnsf = new TransformSerializationWrapper(transform);
			reader.Structure(trnsf, "Transform");
			gameObject.name = reader.String("GameObjName");
		}
		
		#region Helper class for serialization
		private class TransformSerializationWrapper : Serialization.ISerializableRT
		{
			public Transform Tr;
			public TransformSerializationWrapper(Transform tr) { Tr = tr; }
			public void WriteData(Serialization.DataWriter writer)
			{
				writer.Vec3f(Tr.position, "Pos");
				writer.Vec3f(Tr.lossyScale, "Scale");
				writer.Quaternion(Tr.rotation, "QuaternionRot");
			}
			public void ReadData(Serialization.DataReader reader)
			{
				Tr.position = reader.Vec3f("Pos");
				Tr.localScale = reader.Vec3f("Scale");
				Tr.rotation = reader.Quaternion("QuaternionRot");
			}
		}
		#endregion
	}
}