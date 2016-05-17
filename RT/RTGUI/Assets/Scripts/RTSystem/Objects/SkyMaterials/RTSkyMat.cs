using System.Xml;
using System.Collections;
using UnityEngine;


namespace RT
{
	[DisallowMultipleComponent]
	public abstract class RTSkyMat : MonoBehaviour, RTSerializer.ISerializable
	{
		public static void Write(RTSkyMat mat, RTSerializer.Writer writer, string name)
		{
			writer.WriteString(mat.TypeName, name + "Type");
			writer.WriteDataStructure(mat, name + "Value");
		}
		public static RTSkyMat Read(GameObject owner, RTSerializer.Reader reader, string name)
		{
			string typeName = reader.ReadString(name + "Type");
			RTSkyMat mat = null;
			switch (typeName)
			{
				case TypeName_SimpleColor:
					mat = owner.AddComponent<RTSkyMat_SimpleColor>();
					break;
				case TypeName_VerticalGradient:
					mat = owner.AddComponent<RTSkyMat_VerticalGradient>();
					break;
					
				default:
					throw new RTSerializer.SerializerException("Unknown RTSkyMat type \"" +
															   typeName + "\n");
			}

			reader.ReadDataStructure(mat, name + "Value");
			return mat;
		}


		protected const string TypeName_SimpleColor = "SimpleColor",
		                       TypeName_VerticalGradient = "VerticalGradient";


		public MeshRenderer Renderer { get; private set; }
		

		public abstract string TypeName { get; }
		public abstract Material UnityMat { get; }
		
		
		public abstract void DoGUI();
		public abstract void SetMaterialParams(Material m);

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