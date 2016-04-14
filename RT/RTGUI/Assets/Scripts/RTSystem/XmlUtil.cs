using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;


namespace RT
{
	public static class XmlUtil
	{
		public static string ToString(Vector3 v)
		{
			return v.x.ToString() + " " + v.y + " " + v.z;
		}
		public static bool FromString(string vectorStr, ref Vector3 outV)
		{
			string[] spl = vectorStr.Split(' ');

			return spl.Length == 3 &&
				   float.TryParse(spl[0], out outV.x) &&
				   float.TryParse(spl[1], out outV.y) &&
				   float.TryParse(spl[2], out outV.z);
		}

		public static void SetAttr(XmlElement el, string attributeName, string attributeValue)
		{
			XmlAttribute attr = el.OwnerDocument.CreateAttribute(attributeName);
			attr.Value = attributeValue;
			el.Attributes.Append(attr);
		}

		public static XmlElement FindElement(XmlNode parent, string elName)
		{
			foreach (XmlElement el in parent.ChildNodes.OfType<XmlElement>())
			{
				if (el.Name == elName)
					return el;
			}
			return null;
		}
		public static XmlElement FindSiblingElement(XmlElement el, string nextElName)
		{
			XmlNode n = el.NextSibling;
			while (n != null && !(n is XmlElement) && n.Name != nextElName)
				n = n.NextSibling;

			if (n == null)
			{
				return null;
			}
			else
			{
				return (XmlElement)n;
			}
		}

		
		public static Color ToCol(this Vector3 v) { return new Color(v.x, v.y, v.z); }
		public static Vector3 ToV3(this UnityEngine.Color c) { return new Vector3(c.r, c.g, c.b); }
	}
}