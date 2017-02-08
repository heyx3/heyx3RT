using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;


public static class GlobalTextures
{
	public static Texture2D WhitePixel
	{
		get
		{
			if (whitePixel == null)
				whitePixel = AssetDatabase.LoadAssetAtPath<Texture2D>("Textures/builtin/WhitePixel.png");
			return whitePixel;
		}
	}
	private static Texture2D whitePixel = null;
}