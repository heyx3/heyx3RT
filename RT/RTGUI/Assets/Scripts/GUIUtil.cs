using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


public static class GUIUtil
{
	public static bool IsValidNumber(string str, bool signed, bool canHaveDecimal)
	{
		bool foundDecimal = false;
		for (int i = 0; i < str.Length; ++i)
		{
			if (str[i] == '-')
			{
				if (!signed || i > 0)
				{
					return false;
				}
			}
			else if (str[i] == '.')
			{
				if (foundDecimal || !canHaveDecimal)
				{
					return false;
				}
			}
			else if (str[i] < '0' || str[i] > '9')
			{
				return false;
			}
		}

		return true;
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
			current.x = TextEditor(current.x, ref currentX, textBoxStyle);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(y, labelStyle);
			current.y = TextEditor(current.y, ref currentY, textBoxStyle);
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		{
			GUILayout.Label(z, labelStyle);
			current.z = TextEditor(current.z, ref currentZ, textBoxStyle);
		}
		GUILayout.EndHorizontal();

		return current;
	}
	

	public static T TextEditor<T>(T current, ref string currentStr, GUIStyle textBoxStyle,
								  Func<string, bool> isParsable, Func<string, T> parser)
	{
		currentStr = GUILayout.TextField(currentStr, textBoxStyle);

		T temp;
		if (isParsable(currentStr))
			return parser(currentStr);

		return current;
	}
	
	public static int TextEditor(int current, ref string currentStr, GUIStyle textBoxStyle)
	{
		return TextEditor(current, ref currentStr, textBoxStyle,
						  (s) => IsValidNumber(s, true, false), int.Parse);
	}
	public static uint TextEditor(uint current, ref string currentStr, GUIStyle textBoxStyle)
	{
		return TextEditor(current, ref currentStr, textBoxStyle,
						  (s) => IsValidNumber(s, false, false), uint.Parse);
	}
	public static float TextEditor(float current, ref string currentStr, GUIStyle textBoxStyle)
	{
		return TextEditor(current, ref currentStr, textBoxStyle,
						  (s) => IsValidNumber(s, true, true), float.Parse);
	}
}