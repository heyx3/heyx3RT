using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RT
{
	public class Tracer : MonoBehaviour, RTSerializer.ISerializable
	{
		public static Tracer Instance
		{
			get { if (trcer == null) trcer = FindObjectOfType<Tracer>(); return trcer; }
		}
		private static Tracer trcer = null;
		

		private static GameObject CreateObj() { return CreateObj(Vector3.zero, Vector3.one, Quaternion.identity); }
		private static GameObject CreateObj(Vector3 pos, Vector3 scale, Quaternion rot)
		{
			GameObject go = new GameObject("Object");
			
			Transform tr = go.transform;
			tr.position = pos;
			tr.localScale = scale;
			tr.rotation = rot;

			go.AddComponent<MeshFilter>();
			go.AddComponent<MeshRenderer>();

			return go;
		}


		public List<GameObject> Objects;
		public RTSkyMat SkyMaterial;


		public void WriteData(RTSerializer.Writer writer)
		{
			RTSkyMat.Write(SkyMaterial, writer, "SkyMaterial");

			ShapeAndMat sm = new ShapeAndMat();
			writer.WriteList(Objects,
							 (go, name, wr) =>
							 {
								 sm.Obj = go;
								 wr.WriteDataStructure(sm, name);
							 },
							 "Objects");
		}
		public void ReadData(RTSerializer.Reader reader)
		{
			SkyMaterial = RTSkyMat.Read(CreateObj(), reader, "SkyMaterial");

			ShapeAndMat sm = new ShapeAndMat();
			reader.ReadList(Objects,
							(name, rd) =>
							{
								sm.Obj = CreateObj();
								rd.ReadDataStructure(sm, name);
								return sm.Obj;
							},
							"Objects");
		}


		private class ShapeAndMat : RTSerializer.ISerializable
		{
			public GameObject Obj;

			public ShapeAndMat(GameObject obj = null) { Obj = obj; }

			public void WriteData(RTSerializer.Writer writer)
			{
				RTShape.Write(Obj.GetComponent<RTShape>(), writer, "Shape");
				RTMat.Write(Obj.GetComponent<RTMat>(), writer, "Material");
			}
			public void ReadData(RTSerializer.Reader reader)
			{
				Obj = CreateObj();
				RTShape.Read(Obj, reader, "Shape");
				RTMat.Read(Obj, reader, "Material");
			}
		}
	}
}