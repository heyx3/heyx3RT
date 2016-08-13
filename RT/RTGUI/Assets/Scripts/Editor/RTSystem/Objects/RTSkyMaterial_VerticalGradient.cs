using System;
using System.Xml;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RT
{
	public class RTSkyMaterial_VerticalGradient : RTSkyMaterial
	{
		public override string TypeName { get { return TypeName_VerticalGradient; } }

		public Vector3 BottomColor, TopColor;


		public override void DoGUI()
		{
#if UNITY_EDITOR
			BottomColor = EditorGUILayout.ColorField("Bottom Color", BottomColor.ToCol()).ToV3();
			TopColor = EditorGUILayout.ColorField("Top Color", TopColor.ToCol()).ToV3();
#endif
		}

		public override Material GetUnityMat()
		{
			return RTSystem.Instance.SkyMat_SolidColor;
		}
		public override void SetUnityMatParams(Material m)
		{
			m.SetColor("_BottomCol", BottomColor.ToCol());
			m.SetColor("_TopCol", TopColor.ToCol());
		}

		protected override void ReadCustomData(XmlElement parentNode)
		{
			XmlElement colEl = XmlUtil.FindElement(parentNode, "BottomColor");
			if (colEl == null)
			{
				Debug.LogError("Couldn't find 'BottomColor' child element in sky material");
			}
			else
			{
				if (!XmlUtil.FromString(colEl.GetAttribute("Value"), ref BottomColor))
				{
					Debug.LogError("Couldn't parse 'BottomColor' value in sky material");
				}
				else
				{
					colEl = XmlUtil.FindElement(parentNode, "TopColor");
					if (colEl == null)
					{
						Debug.LogError("Couldn't find 'TopColor' child element in sky material");
					}
					else
					{
						if (!XmlUtil.FromString(colEl.GetAttribute("Value"), ref TopColor))
						{
							Debug.LogError("Couldn't parse 'TopColor' value in sky material");
						}
					}
				}
			}
		}
		protected override void WriteCustomData(XmlElement parentNode)
		{
			XmlElement colEl = parentNode.OwnerDocument.CreateElement("BottomColor");
			XmlUtil.SetAttr(colEl, "Value", XmlUtil.ToString(BottomColor));
			parentNode.AppendChild(colEl);

			colEl = parentNode.OwnerDocument.CreateElement("TopColor");
			XmlUtil.SetAttr(colEl, "Value", XmlUtil.ToString(TopColor));
			parentNode.AppendChild(colEl);
		}
	}
}