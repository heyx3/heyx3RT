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
				case TypeName_MeshFile:
					shpe = owner.AddComponent<RTShape_MeshFile>();
					break;

				default: throw new RTSerializer.SerializerException("Unknown RTShape type \"" +
																	typeName + "\"");
			}

			reader.ReadDataStructure(shpe, name + "Value");
			return shpe;
		}

		protected static RTGui.Gui Gui { get { return RTGui.Gui.Instance; } }


		protected const string TypeName_Sphere = "Sphere",
		                       TypeName_Plane = "Plane",
		                       TypeName_MeshFile = "MeshFile";


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
			AddCollider();
		}
		private void OnMouseDown()
		{
			RTLogic.Controller.Instance.OnShapeClicked(this);
		}


		protected virtual void AddCollider() { gameObject.AddComponent<MeshCollider>(); }

		string posX = null, posY, posZ, scaleX, scaleY, scaleZ;
		public virtual void DoGUI()
		{
			if (posX == null)
			{
				posX = Tr.position.x.ToString();
				posY = Tr.position.y.ToString();
				posZ = Tr.position.z.ToString();
				scaleX = Tr.localScale.x.ToString();
				scaleY = Tr.localScale.y.ToString();
				scaleZ = Tr.localScale.z.ToString();
			}


			GUILayout.Label("Position", Gui.Style_Text);
			GUIUtil.StartTab(Gui.TabSize);
			Tr.position = GUIUtil.Vec3Editor(Tr.position, Gui.Style_Text, Gui.Style_TextBox,
											 ref posX, ref posY, ref posZ);
			GUIUtil.EndTab();

			GUILayout.Label("Scale", Gui.Style_Text);
			GUIUtil.StartTab(Gui.TabSize);
			Tr.localScale = GUIUtil.Vec3Editor(Tr.localScale, Gui.Style_Text, Gui.Style_TextBox,
											   ref scaleX, ref scaleY, ref scaleZ);
			GUIUtil.EndTab();

			GUILayout.Label("Rotation", Gui.Style_Text);
			GUIUtil.StartTab(Gui.TabSize);
			Tr.eulerAngles = GUIUtil.Vec3Editor(Tr.eulerAngles, -180.0f, 180.0f,
												Gui.Style_Text, Gui.Style_Slider, Gui.Style_SliderThumb);
			GUIUtil.EndTab();
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