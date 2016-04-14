using System.Xml;
using System.Collections;
using UnityEngine;


namespace RT
{
	public abstract class RTShape : MonoBehaviour
	{
		protected const string TypeName_Sphere = "Sphere",
		                       TypeName_Plane = "Plane",
		                       TypeName_Mesh = "Mesh";

		public static RTShape FromXML(GameObject toAttachTo, XmlElement shapeEl)
		{
			RTShape shpe = null;

			string typeName = shapeEl.GetAttribute("Type");

			//TODO: Finish.
			switch (typeName)
			{
				case TypeName_Sphere:
				    //shpe = toAttachTo.AddComponent<RTShape_Sphere>();
					break;
				case TypeName_Plane:
				    shpe = toAttachTo.AddComponent<RTShape_Plane>();
					break;
				case TypeName_Mesh:
				    shpe = toAttachTo.AddComponent<RTShape_Mesh>();
					break;

				default:
					Debug.LogError("Unknown shape type \"" + typeName + "\"");
				break;
			}

			if (shpe != null)
			{
				XmlElement trEl = XmlUtil.FindElement(shapeEl, "Transform");
				if (trEl == null)
				{
					Debug.LogError("Couldn't find \"Transform\" element");
				}
				shpe.ReadTranform(trEl);

				shpe.ReadCustomData(shapeEl);
			}

			return shpe;
		}

		
		public abstract string TypeName { get; }
		

		public void DoGUI()
		{
			DoMyGUI();
		}
		protected virtual void DoMyGUI() { }
		
		public abstract Mesh GetUnityMesh();


		public void WriteData(XmlElement rootNode, int myIndex)
		{
			XmlElement el = rootNode.OwnerDocument.CreateElement("Shape" + myIndex);
			rootNode.AppendChild(el);

			XmlElement trEl = rootNode.OwnerDocument.CreateElement("Transform");
			WriteTransform(trEl);
			el.AppendChild(trEl);

			XmlAttribute typeAttr = rootNode.OwnerDocument.CreateAttribute("Type");
			typeAttr.Value = TypeName;
			el.Attributes.Append(typeAttr);
			
			WriteCustomData(el);
		}

		private void WriteTransform(XmlElement trEl)
		{
			Transform tr = transform;

			Quaternion rot = tr.rotation;
			Vector3 rotAxis;
			float rotAngle;
			rot.ToAngleAxis(out rotAngle, out rotAxis);

			XmlUtil.SetAttr(trEl, "RotAngle", rotAngle.ToString());
			XmlUtil.SetAttr(trEl, "RotAxis", XmlUtil.ToString(rotAxis));
			XmlUtil.SetAttr(trEl, "Scale", XmlUtil.ToString(tr.localScale));
			XmlUtil.SetAttr(trEl, "Pos", XmlUtil.ToString(tr.position));
		}
		private void ReadTranform(XmlElement trEl)
		{
			string rotAngleS = trEl.GetAttribute("RotAngle"),
			       rotAxisS = trEl.GetAttribute("RotAxis"),
			       scaleS = trEl.GetAttribute("Scale"),
			       posS = trEl.GetAttribute("Pos");
			
			float rotAngle = 0.0f;
			Vector3 rotAxis = Vector3.up,
			scale = Vector3.one,
			pos = new Vector3();
			if (!float.TryParse(rotAngleS, out rotAngle) ||
			    !XmlUtil.FromString(rotAxisS, ref rotAxis) ||
			    !XmlUtil.FromString(scaleS, ref scale) ||
			    !XmlUtil.FromString(posS, ref pos))
			{
				return;
			}
			
			Transform tr = transform;
			tr.position = pos;
			tr.rotation = Quaternion.AngleAxis(rotAngle, rotAxis);
			tr.localScale = scale;
		}
		
		protected virtual void WriteCustomData(XmlElement parentNode) { }
		protected virtual void ReadCustomData(XmlElement parentNode) { }
	}
}