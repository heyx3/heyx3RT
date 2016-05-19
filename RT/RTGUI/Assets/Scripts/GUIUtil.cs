using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


public static class GUIUtil
{
	public static Vector3 Vec3Editor(Vector3 current, float min, float max,
									 GUIStyle labelStyle,
									 GUIStyle sliderBarStyle, GUIStyle sliderThumbStyle,
									 string x = "X", string y = "Y", string z = "Z")
	{
		return Vec3Editor(current, new Vector3(min, min, min), new Vector3(max, max, max),
						  labelStyle, sliderBarStyle, sliderThumbStyle, x, y, z);
	}
	public static Vector3 Vec3Editor(Vector3 current, Vector3 min, Vector3 max,
									 GUIStyle labelStyle,
									 GUIStyle sliderBarStyle, GUIStyle sliderThumbStyle,
									 string x = "X", string y = "Y", string z = "Z")
	{
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(x, labelStyle);
			current.x = GUILayout.HorizontalSlider(current.x, min.x, max.x,
												   sliderBarStyle, sliderThumbStyle);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(y, labelStyle);
			current.y = GUILayout.HorizontalSlider(current.y, min.y, max.y,
												   sliderBarStyle, sliderThumbStyle);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(z, labelStyle);
			current.z = GUILayout.HorizontalSlider(current.z, min.z, max.z,
												   sliderBarStyle, sliderThumbStyle);
		}
		GUILayout.EndHorizontal();

		return current;
	}
	public static Vector3 Vec3Editor(Vector3 current, GUIStyle labelStyle, GUIStyle textBoxStyle,
									 ref string currentX, ref string currentY, ref string currentZ,
									 string x = "X", string y = "Y", string z = "Z")
	{
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(x, labelStyle);
			current.x = FloatEditor(current.x, ref currentX, textBoxStyle);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(y, labelStyle);
			current.y = FloatEditor(current.y, ref currentY, textBoxStyle);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(z, labelStyle);
			current.z = FloatEditor(current.z, ref currentZ, textBoxStyle);
		}
		GUILayout.EndHorizontal();

		return current;
	}
	
	public static float FloatEditor(float current, ref string currentStr, GUIStyle textBoxStyle)
	{
		currentStr = GUILayout.TextField(currentStr, textBoxStyle);

		float temp;
		if (float.TryParse(currentStr, out temp))
			return temp;

		return current;
	}

	public static void StartTab(float space)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(space);
		GUILayout.BeginVertical();
	}
	public static void EndTab()
	{
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}
}