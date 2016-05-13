using System;
using System.Collections;
using UnityEngine;


namespace RT
{
	[UnityEngine.DisallowMultipleComponent]
	[RequireComponent(typeof(MeshFilter))]
	public abstract class RTShape : MonoBehaviour, RTSerializer.ISerializable
	{
		public static void Write(RTShape shape, RTSerializer.Writer writer, string name)
		{
			writer.WriteString(shape.TypeName, name + "Type");
			writer.WriteDataStructure(shape, name + "Value");
		}
		public static RTShape Read(GameObject owner, RTSerializer.Reader reader, string name)
		{
			string typeName = reader.ReadString(name + "Type");
			RTShape shpe = null;
			switch (typeName)
			{
				case TypeName_Sphere:
					shpe = owner.AddComponent<RTShape_Sphere>();
					break;
				case TypeName_Plane:
					shpe = owner.AddComponent<RTShape_Plane>();
					break;
				case TypeName_Mesh:
					shpe = owner.AddComponent<RTShape_Mesh>();
					break;

				default: throw new RTSerializer.SerializerException("Unknown RTShape type \"" +
																	typeName + "\"");
			}

			reader.ReadDataStructure(ref shpe, name + "Value");
			return shpe;
		}


		protected const string TypeName_Sphere = "Sphere",
		                       TypeName_Plane = "Plane",
		                       TypeName_Mesh = "Mesh";


		public Transform Tr { get; private set; }
		public MeshFilter MeshFlt { get; private set; }

		public abstract string TypeName { get; }
		public abstract Mesh UnityMesh { get; }
		

		private void Awake()
		{
			Tr = transform;
			MeshFlt = GetComponent<MeshFilter>();
		}
		private void Start()
		{
			MeshFlt.mesh = UnityMesh;
		}
		

		protected virtual void DoGUI()
		{
			//TODO: Editor for Transform properties.
		}

		public virtual void WriteData(RTSerializer.Writer writer)
		{
			writer.WriteTransform(Tr, "Transform");
		}
		public virtual void ReadData(RTSerializer.Reader reader)
		{
			reader.ReadTransform(Tr, "Transform");
		}
	}
}