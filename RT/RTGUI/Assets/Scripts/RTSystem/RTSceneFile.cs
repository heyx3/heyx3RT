using System;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;


namespace RT
{
	public static class RTSceneFile
	{
		public static void ToFile(string filePath, List<RTShape> shapes,
		                          List<RTMaterial> materials, RTSkyMaterial skyMat)
		{
			XmlDocument doc = new XmlDocument();
			XmlElement rootEl = doc.CreateElement("Scene");
			doc.AppendChild(rootEl);

			XmlUtil.SetAttr(rootEl, "NShapes", shapes.Count.ToString());
			if (shapes.Count != materials.Count)
			{
				Debug.LogError("The number of shapes and materials must match!");
				return;
			}

			for (int i = 0; i < shapes.Count; ++i)
			{
				shapes[i].WriteData(rootEl, i);
				materials[i].WriteData(rootEl, i);
			}
			skyMat.WriteData(rootEl);

			//Try to save the filie.
			try
			{
				doc.Save(filePath);
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to save file: " + e.Message);
			}
		}
		public static void FromFile(string filePath, List<RTShape> outShapes,
		                            List<RTMaterial> outMaterials, out RTSkyMaterial outSkyMat)
		{
			outSkyMat = null;
			outShapes.Clear();
			outMaterials.Clear();

			XmlDocument doc = new XmlDocument();
			try
			{
				doc.Load(filePath);
			}
			catch (Exception e)
			{
				Debug.LogError("Unable to load file: " + e.Message);
				return;
			}

			XmlElement rootEl = XmlUtil.FindElement(doc, "Scene");
			if (rootEl == null)
			{
				Debug.LogError("Couldn't find the root Scene element in the file");
				return;
			}

			string str;
			
			str = rootEl.GetAttribute("NShapes");
			int nShapes = 0;
			if (str == null || !int.TryParse(str, out nShapes))
			{
				Debug.LogError("Missing or invalid 'NShapes' attribute in root Scene element");
				return;
			}


			//Read in the shapes/materials.
			for (int i = 0; i < nShapes; ++i)
			{
				GameObject go = new GameObject();

				//Shape.
				XmlElement el = XmlUtil.FindElement(rootEl, "Shape" + i);
				if (el == null)
				{
					Debug.LogError("Can't find element Shape" + i);
					return;
				}
				outShapes.Add(RTShape.FromXML(go, el));

				go.name = outShapes[i].TypeName;
				
				//Material.
				el = XmlUtil.FindElement(rootEl, "Material" + i);
				if (el == null)
				{
					Debug.LogError("Can't find element Material" + i);
					return;
				}
				outMaterials.Add(RTMaterial.FromXML(go, el));
			}

			//Sky material.
			XmlElement skyMatEl = XmlUtil.FindElement(rootEl, "SkyMaterial");
			if (skyMatEl == null)
			{
				Debug.LogError("Couldn't find SkyMaterial element");
				return;
			}
			GameObject skymatGO = new GameObject("Sky Material");
			outSkyMat = RTSkyMaterial.FromXML(skymatGO, skyMatEl);
		}
	}
}