using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


//TODO: When serializing scene and material graphs, set indentation to 0 or 1.

namespace RT
{
	public class ShapeAndMat : Serialization.ISerializableRT
	{
		public GameObject Obj;

		public ShapeAndMat(GameObject obj = null) { Obj = obj; }

		public void WriteData(Serialization.DataWriter writer)
		{
			RTShape.Serialize(Obj.GetComponent<RTShape>(), "Shape", writer);
			RTMaterial.Serialize(Obj.GetComponent<RTMaterial>(), "Material", writer);
		}
		public void ReadData(Serialization.DataReader reader)
		{
			RTShape.Deserialize(Obj, reader, "Shape");
			RTMaterial.Deserialize(Obj, reader, "Material");
		}
	}


	public class Scene : Serialization.ISerializableRT
	{
		public void WriteData(Serialization.DataWriter writer)
		{
			RTSkyMaterial.Serialize(RTSkyMaterial.Instance, "SkyMaterial", writer);

			var objs = UnityEngine.Object.FindObjectsOfType<RTShape>()
										 .Select(shpe => new ShapeAndMat(shpe.gameObject))
										 .ToList();
			writer.List(objs, "Objects", (wr, val, name) => wr.Structure(val, name));
		}
		public void ReadData(Serialization.DataReader reader)
		{
			//Clear out any previous Sky Material, and make sure we have a GameObject for the new one.
			GameObject skyMatO;
			RTSkyMaterial skyMat = RTSkyMaterial.Instance;
			if (skyMat != null)
			{
				skyMatO = skyMat.gameObject;
				UnityEngine.Object.DestroyImmediate(skyMat);
			}
			else
			{
				skyMatO = new GameObject("Sky");
			}

			RTSkyMaterial.Deserialize(skyMatO, "SkyMaterial", reader);


			reader.List("Objects", (Serialization.DataReader rd, ref ShapeAndMat val, string name) =>
								       rd.Structure(new ShapeAndMat(new GameObject(name)), name));
		}
	}
}