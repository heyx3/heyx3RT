using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public static class RTExtensions
{
	public static Color ToCol(this Vector3 rgb, float a = 1.0f) { return new Color(rgb.x, rgb.y, rgb.z, a); }
	
	public static Vector3 ToRGB(this Color col) { return new Vector3(col.r, col.g, col.b); }
	public static Vector4 ToRGBA(this Color col) { return new Vector4(col.r, col.g, col.b, col.a); }

	public static Color Invert(this Color col) { return new Color(1.0f - col.r, 1.0f - col.g, 1.0f - col.b, 1.0f - col.a); }


	public static int IndexOf<T>(this IList<T> list, T toFind)
		where T : IEquatable<T>
	{
		int i;
		for (i = 0; i < list.Count; ++i)
			if (list[i].Equals(toFind))
				return i;
		return -1;
	}
}