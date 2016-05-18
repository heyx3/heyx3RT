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

		
		public GUIStyle Style_MaterialValue_Text, Style_MaterialValue_Button,
						Style_MaterialValue_Slider, Style_MaterialValue_SliderThumb,
						Style_MaterialValue_Texture;
		public GUIStyle Style_FileBrowser_Files, Style_FileBrowser_Buttons, Style_FileBrowser_Text;
		public GUIStyle Style_RGB_Slider, Style_RGB_SliderThumb;

		public Vector2 MaxTexPreviewSize = new Vector2(64.0f, 64.0f);

		public float MaterialValueTabSize = 10.0f;
	}
}
