using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace RT
{
	public class RTMaterial_Lambert : RTMaterial
	{
		public override string TypeName { get { return TypeName_Lambert; } }


		public Vector3 Albedo = Vector3.one;


		public override void DoGUI()
		{
#if UNITY_EDITOR
			Albedo = EditorGUILayout.ColorField(Albedo.ToCol()).ToV3();
#endif
		}
		
		public override Material GetUnityMat()
		{
			return RTSystem.Instance.Mat_Lambert;
		}
		public override void SetUnityMatParams(Material m)
		{
			m.color = Albedo.ToCol();
		}

		protected override void ReadCustomData (XmlElement parentNode)
		{
			foreach (XmlElement el in parentNode.ChildNodes.OfType<XmlElement>())
			{
				if (el.Name == "Albedo")
				{
					if (!XmlUtil.FromString(el.GetAttribute("Value"), ref Albedo))
					{
						Debug.LogError("Couldn't parse albedo for Lambert mat");
					}
				}
			}
		}
		protected override void WriteCustomData (XmlElement parentNode)
		{
			XmlElement albedoEl = parentNode.OwnerDocument.CreateElement("Albedo");
			XmlUtil.SetAttr(albedoEl, "Value", XmlUtil.ToString(Albedo));
			
			parentNode.AppendChild(albedoEl);
		}
	}
}