using System.Xml;
using System.Collections;
using UnityEngine;

namespace RT
{
	//TODO: Fix Material/SkyMaterial.

	public abstract class RTMaterial : MonoBehaviour
	{
		protected const string TypeName_Lambert = "Lambert",
		                       TypeName_Metal = "Metal";

		public static RTMaterial FromXML(GameObject toAttachTo, XmlElement matEl)
		{
			RTMaterial mat = null;

			string typeName = matEl.GetAttribute("Type");

			switch (typeName)
			{
				case TypeName_Lambert:
				    mat = toAttachTo.AddComponent<RTMaterial_Lambert>();
				    break;
				case TypeName_Metal:
				    mat = toAttachTo.AddComponent<RTMaterial_Metal>();
				    break;

			    default:
				    Debug.LogError("Unknown material type \"" + typeName + "\"");
				    return null;
			}

			if (mat != null)
			{
				mat.ReadCustomData(matEl);
			}

			return mat;
		}


		public abstract string TypeName { get; }


		public abstract void DoGUI();

		public abstract Material GetUnityMat();
		public abstract void SetUnityMatParams(Material m);


		public void WriteData(XmlElement rootNode, int myIndex)
		{
			XmlElement el = rootNode.OwnerDocument.CreateElement("Material" + myIndex);
			rootNode.AppendChild(el);
			XmlUtil.SetAttr(el, "Type", TypeName);

			WriteCustomData(el);
		}

		protected abstract void WriteCustomData(XmlElement parentNode);
		protected abstract void ReadCustomData(XmlElement parentNode);
	}
}