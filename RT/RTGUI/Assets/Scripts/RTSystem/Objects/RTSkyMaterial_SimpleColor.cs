using System;
using System.Xml;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RT
{
	public class RTSkyMaterial_SimpleColor : RTSkyMaterial
	{
		public override string TypeName { get { return TypeName_SimpleColor; } }

		public Vector3 Color;


		public override void DoGUI()
		{
#if UNITY_EDITOR
			Color = EditorGUILayout.ColorField(this.Color.ToCol()).ToV3();
#endif
		}

		public override Material GetUnityMat()
		{
			return RTSystem.Instance.SkyMat_SolidColor;
		}
		public override void SetUnityMatParams(Material m)
		{
			m.color = this.Color.ToCol();
		}

		protected override void ReadCustomData(XmlElement parentNode)
		{
			XmlElement colEl = XmlUtil.FindElement(parentNode, "Color");
			if (colEl == null)
			{
				Debug.LogError("Couldn't find 'Color' child element in sky material");
			}
			else
			{
				if (!XmlUtil.FromString(colEl.GetAttribute("Value"), ref Color))
				{
					Debug.LogError("Couldn't parse 'Color' value in sky material");
				}
			}
		}
		protected override void WriteCustomData(XmlElement parentNode)
		{
			XmlElement colEl = parentNode.OwnerDocument.CreateElement("Color");
			XmlUtil.SetAttr(colEl, "Value", XmlUtil.ToString(this.Color));
			parentNode.AppendChild(colEl);
		}
	}
}