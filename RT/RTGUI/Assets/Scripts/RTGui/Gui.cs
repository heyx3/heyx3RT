using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace RTGui
{
	public class Gui : MonoBehaviour
	{
		public static Gui Instance
		{
			get { if (rtSys == null) rtSys = FindObjectOfType<Gui>(); return rtSys; }
		}
		private static Gui rtSys = null;


		public GUIStyle Style_Text, Style_Button,
						Style_Slider, Style_SliderThumb,
					    Style_TextBox, Style_SelectionGrid;
		public GUIStyle Style_Window;
		public GUIStyle Style_MValTexture;

		public Vector2 MaxTexPreviewSize = new Vector2(64.0f, 64.0f);

		public float TabSize = 10.0f;
	}
}
