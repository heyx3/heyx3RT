using System.Xml;
using System.Collections;
using UnityEngine;


namespace RT
{
	//TODO: Fix SkyMaterial and its children.


	[DisallowMultipleComponent]
	public abstract class RTSkyMaterial : MonoBehaviour
	{
		protected const string TypeName_SimpleColor = "SimpleColor",
		                       TypeName_VerticalGradient = "VerticalGradient";
		
		public static RTSkyMaterial FromXML(GameObject toAttachTo, XmlElement matEl)
		{
			RTSkyMaterial mat = null;
			
			string typeName = matEl.GetAttribute("Type");
			
			switch (typeName)
			{
			case TypeName_SimpleColor:
				mat = toAttachTo.AddComponent<RTSkyMaterial_SimpleColor>();
				break;
			case TypeName_VerticalGradient:
				mat = toAttachTo.AddComponent<RTSkyMaterial_VerticalGradient>();
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
		
		
		public void WriteData(XmlElement rootNode)
		{
			XmlElement el = rootNode.OwnerDocument.CreateElement("SkyMaterial");
			rootNode.AppendChild(el);
			XmlUtil.SetAttr(el, "Type", TypeName);
			
			WriteCustomData(el);
		}
		
		protected abstract void WriteCustomData(XmlElement parentNode);
		protected abstract void ReadCustomData(XmlElement parentNode);
	}
}