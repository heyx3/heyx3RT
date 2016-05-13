using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


public static class GUIUtil
{
	public static Color ToCol(this Vector3 rgb, float a = 1.0f) { return new Color(rgb.x, rgb.y, rgb.z, a); }
	
	public static Vector3 ToRGB(this Color col) { return new Vector3(col.r, col.g, col.b); }
	public static Vector4 ToRGBA(this Color col) { return new Vector4(col.r, col.g, col.b, col.a); }


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


	public class FileBrowserData
	{
		public static FileBrowserData Get(int id) { return browsers[id]; }
		public static bool Release(int id) { return browsers.Remove(id); }

		private static int nextID = 0;
		private static Dictionary<int, FileBrowserData> browsers = new Dictionary<int, FileBrowserData>();
		

		public int ID { get; private set; }

		public string[] SupportedExtensions;
		public GUIStyle FileChoicesStyle, ControlButtonsStyle;

		public FileInfo CurrentFile;
		public DirectoryInfo CurrentDir;
		public Rect CurrentWindowPos;

		public Action<FileInfo> OnFileChosen;


		public FileBrowserData(string startPath, Rect startWindowPos,
							   Action<FileInfo> onFileChosen,
							   GUIStyle fileChoicesStyle, GUIStyle controlButtonsStyle,
							   params string[] supportedExtensions)
		{
			ID = nextID;
			unchecked { nextID += 1; }

			SupportedExtensions = supportedExtensions;
			FileChoicesStyle = fileChoicesStyle;
			ControlButtonsStyle = controlButtonsStyle;

			CurrentFile = null;
			CurrentDir = new DirectoryInfo(startPath);
			CurrentWindowPos = startWindowPos;

			OnFileChosen = onFileChosen;
		}
	}

	public static void FileBrowserWindowCallback(int id)
	{
		FileBrowserData data = FileBrowserData.Get(id);
		
		//Show the "parent directory" button.
		if (data.CurrentDir.Parent != null && GUILayout.Button("..", data.FileChoicesStyle))
		{
			data.CurrentFile = null;
			data.CurrentDir = data.CurrentDir.Parent;
		}

		//Show the buttons for each choosable file in this directory.
		foreach (string extension in data.SupportedExtensions)
			foreach (FileInfo f in data.CurrentDir.GetFiles("*" + extension,
															SearchOption.TopDirectoryOnly))
				if (GUILayout.Button(f.Name, data.FileChoicesStyle))
					data.CurrentFile = f;

		GUILayout.Label("Current file: " + data.CurrentFile.Name);

		//Cancel/Select buttons.
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Cancel", data.ControlButtonsStyle))
			data.OnFileChosen(null);
		if (data.CurrentFile != null && GUILayout.Button("Select", data.ControlButtonsStyle))
			data.OnFileChosen(data.CurrentFile);
		GUILayout.EndHorizontal();
	}
}