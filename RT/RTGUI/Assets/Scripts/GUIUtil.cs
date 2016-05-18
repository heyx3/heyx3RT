using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


public static class GUIUtil
{
	public static Vector3 RGBEditor(Vector3 current, float tabSize,
									GUIStyle labelStyle, GUIStyle sliderBarStyle, GUIStyle sliderThumbStyle)
	{
		GUILayout.BeginHorizontal();
			GUILayout.Space(tabSize);
			GUILayout.Label("R", labelStyle);
			current.x = GUILayout.HorizontalSlider(current.x, 0.0f, 1.0f,
												   sliderBarStyle, sliderThumbStyle);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
			GUILayout.Space(tabSize);
			GUILayout.Label("G", labelStyle);
			current.y = GUILayout.HorizontalSlider(current.y, 0.0f, 1.0f,
												   sliderBarStyle, sliderThumbStyle);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
			GUILayout.Space(tabSize);
			GUILayout.Label("B", labelStyle);
			current.z = GUILayout.HorizontalSlider(current.z, 0.0f, 1.0f,
												   sliderBarStyle, sliderThumbStyle);
		GUILayout.EndHorizontal();

		return current;
	}
}