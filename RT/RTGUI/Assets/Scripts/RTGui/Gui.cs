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
		public GUIStyle Style_MValTexture;
		public GUIStyle Style_FileBrowser_Files, Style_FileBrowser_Buttons,
						Style_FileBrowser_Text, Style_FileBrowser_TextBox;

		public Vector2 MaxTexPreviewSize = new Vector2(64.0f, 64.0f);

		public float TabSize = 10.0f;
	}
}
