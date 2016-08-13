using System.Xml;
using System.Collections.Generic;
using UnityEngine;


namespace RT
{
	//TODO: Add custom inspectors?

	[ExecuteInEditMode]
	public abstract class RTShape : MonoBehaviour, Serialization.ISerializableRT
	{
		public static HashSet<RTShape> Shapes = new HashSet<RTShape>();

		protected const string TypeName_Sphere = "Sphere",
		                       TypeName_Plane = "Plane",
		                       TypeName_Mesh = "Mesh";


		public static void Serialize(Serialization.DataWriter writer)
		{

		}
		public static RTShape Deserialize(GameObject toAttachTo, Serialization.DataReader reader, string name)
		{
			RTShape shpe = null;

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

				default:
					throw new Serialization.DataReader.ReadException("Unknown shape type: " + typeName);
			}

			reader.Structure(shpe, name + "Value");

			return shpe;
		}


		public RTMaterial Mat { get { return GetComponent<RTMaterial>(); } }

		public abstract string TypeName { get; }
		public abstract Mesh UnityMesh { get; }


		protected virtual void Awake()
		{
			Shapes.Add(this);

			MeshFilter mf = GetComponent<MeshFilter>();
			if (mf == null)
				mf = gameObject.AddComponent<MeshFilter>();
			mf.sharedMesh = UnityMesh;
		}
		protected virtual void OnDestroy()
		{
			Shapes.Remove(this);
		}
		
		protected virtual void WriteData(Serialization.DataWriter writer)
		{
			TransformSerializationWrapper trnsf = new TransformSerializationWrapper(transform);
			writer.Structure(trnsf, "Transform");
		}
		protected virtual void ReadData(Serialization.DataReader reader)
		{
			TransformSerializationWrapper trnsf = new TransformSerializationWrapper(transform);
			reader.Structure(trnsf, "Transform");
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