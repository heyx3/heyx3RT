using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace RTGui
{
	public class FileBrowser
	{
		private static void GUIWindowCallback(int id)
		{
			FileBrowser data = browsers[id];
		
			//Show the "parent directory" button.
			if (data.CurrentDir.Parent != null && GUILayout.Button("..", data.FileChoicesStyle))
			{
				data.CurrentFile = null;
				data.CurrentDir = data.CurrentDir.Parent;
			}

			//Show the buttons for each choosable file in this directory.
			foreach (string extension in data.SupportedExtensions)
			{
				foreach (FileInfo f in data.CurrentDir.GetFiles("*" + extension,
																SearchOption.TopDirectoryOnly))
				{
					if (GUILayout.Button(f.Name, data.FileChoicesStyle))
					{
						data.CurrentFile = f;
					}
				}
			}

			GUILayout.Label("Current file: " + data.CurrentFile.Name, data.TextStyle);

			//Cancel/Select buttons.
			GUILayout.BeginHorizontal();
			if (Input.GetKeyDown(KeyCode.Escape) ||
				GUILayout.Button("Cancel", data.ControlButtonsStyle))
			{
				data.OnFileChosen(null);
			}
			if (data.CurrentFile != null && GUILayout.Button("Select", data.ControlButtonsStyle))
			{
				data.OnFileChosen(data.CurrentFile);
			}
			GUILayout.EndHorizontal();
		}


		private static int nextID = 0;
		private static Dictionary<int, FileBrowser> browsers = new Dictionary<int, FileBrowser>();


		public List<FileInfo> ChoosableFilesInDir { get; private set; }
		
		public GUIStyle FileChoicesStyle, ControlButtonsStyle, TextStyle;
		public GUIContent WindowContent;

		public string[] SupportedExtensions;

		public FileInfo CurrentFile;
		public DirectoryInfo CurrentDir;
		public Rect CurrentWindowPos;

		/// <summary>
		/// If the "cancel" button was clicked, "null" is passed into this action.
		/// </summary>
		public Action<FileInfo> OnFileChosen;
		
		private int ID;


		public FileBrowser(string startPath, Rect startWindowPos, GUIContent windowContent,
						   Action<FileInfo> onFileChosen,
						   GUIStyle fileChoicesStyle, GUIStyle controlButtonsStyle, GUIStyle textStyle,
						   params string[] supportedExtensions)
		{
			ID = nextID;
			unchecked { nextID += 1; }
		
			SupportedExtensions = supportedExtensions;

			FileChoicesStyle = fileChoicesStyle;
			ControlButtonsStyle = controlButtonsStyle;
			TextStyle = textStyle;
			WindowContent = windowContent;
		
			CurrentFile = null;
			CurrentDir = new DirectoryInfo(startPath);
			CurrentWindowPos = startWindowPos;

			OnFileChosen = onFileChosen;

			Refresh();
		}


		public void Refresh()
		{
			ChoosableFilesInDir = new List<FileInfo>();
			foreach (string extension in SupportedExtensions)
				ChoosableFilesInDir.AddRange(CurrentDir.GetFiles("*" + extension,
											 SearchOption.TopDirectoryOnly));
		}
		public void Release()
		{
			bool b = browsers.Remove(ID);
			UnityEngine.Assertions.Assert.IsTrue(b);
		}

		public void DoGUI()
		{
			CurrentWindowPos = GUILayout.Window(ID, CurrentWindowPos,
												GUIWindowCallback, WindowContent);
		}
	}
}