using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace RT
{
	public class RTMaterial_Metal : RTMaterial
	{
		public override string TypeName { get { return TypeName_Metal; } }


		public Vector3 Albedo = Vector3.one;
		public float Roughness = 0.2f;


		public override void DoGUI()
		{
#if UNITY_EDITOR
			Albedo = EditorGUILayout.ColorField(Albedo.ToCol()).ToV3();
			Roughness = EditorGUILayout.Slider("Roughness:", Roughness, 0.0f, 1.0f);
#endif
		}

		public override Material GetUnityMat()
		{
			return RTSystem.Instance.Mat_Metal;
		}
		public override void SetUnityMatParams(Material m)
		{
			m.color = Albedo.ToCol();
			m.SetFloat("_Shininess", 1.0f - Roughness);
		}

		protected override void ReadCustomData (XmlElement parentNode)
		{
			foreach (XmlElement el in parentNode.ChildNodes.OfType<XmlElement>())
			{
				if (el.Name == "Albedo")
				{
					if (!XmlUtil.FromString(el.GetAttribute("Value"), ref Albedo))
					{
						Debug.LogError("Couldn't parse albedo for Metal mat");
					}
				}
				else if (el.Name == "Roughness")
				{
					if (!float.TryParse(el.GetAttribute("Value"), out Roughness))
					{
						Debug.LogError("Couldn't parse roughness for Metal mat");
					}
				}
			}
		}
		protected override void WriteCustomData (XmlElement parentNode)
		{
			XmlElement albedoEl = parentNode.OwnerDocument.CreateElement("Albedo");
			XmlUtil.SetAttr(albedoEl, "Value", XmlUtil.ToString(Albedo));

			XmlElement roughnessEl = parentNode.OwnerDocument.CreateElement("Roughness");
			XmlUtil.SetAttr(roughnessEl, "Value", Roughness.ToString());
			
			parentNode.AppendChild(albedoEl);
		}
	}
}