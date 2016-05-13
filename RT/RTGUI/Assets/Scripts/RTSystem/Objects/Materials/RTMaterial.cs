using System.Xml;
using System.Collections;
using UnityEngine;

namespace RT
{
	[UnityEngine.DisallowMultipleComponent]
	[RequireComponent(typeof(MeshRenderer))]
	public abstract class RTMat : MonoBehaviour, RTSerializer.ISerializable
	{
		public static void Write(RTMat mat, RTSerializer.Writer writer, string name)
		{
			writer.WriteString(mat.TypeName, name + "Type");
			writer.WriteDataStructure(mat, name + "Value");
		}
		public static RTMat Read(GameObject owner, RTSerializer.Reader reader, string name)
		{
			string typeName = reader.ReadString(name + "Type");
			RTMat mat = null;
			switch (typeName)
			{
				case TypeName_Lambert:
					mat = owner.AddComponent<RTMat_Lambert>();
					break;
				case TypeName_Metal:
					mat = owner.AddComponent<RTMat_Metal>();
					break;

				default:
					throw new RTSerializer.SerializerException("Unknown RTMat type \"" +
															   typeName + "\"");
			}

			reader.ReadDataStructure(ref mat, name + "Value");
			return mat;
		}


		protected const string TypeName_Lambert = "Lambert",
		                       TypeName_Metal = "Metal";


		public MeshRenderer Renderer { get; private set; }


		public abstract string TypeName { get; }
		public abstract Material UnityMat { get; }


		public abstract void DoGUI();
		public abstract void SetMaterialParams(Material mat);

		public virtual void WriteData(RTSerializer.Writer writer) { }
		public virtual void ReadData(RTSerializer.Reader reader) { }


		private void Awake()
		{
			Renderer = GetComponent<MeshRenderer>();
		}
		private void Start()
		{
			Renderer.material = UnityMat;
		}
		private void Update()
		{
			SetMaterialParams(Renderer.material);
		}
	}
}