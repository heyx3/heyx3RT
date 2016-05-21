using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace RTGui
{
	public class OpenFileBrowser : ManagedWindow
	{
		private static void GUIWindowCallback(int id)
		{
			OpenFileBrowser data = Get<OpenFileBrowser>(id);
		
			//Show the "parent directory" button.
			if (data.CurrentDir.Parent != null &&
				GUILayout.Button("..", Gui.Instance.Style_FileBrowser_Files))
			{
				data.CurrentFile = null;
				data.CurrentDir = data.CurrentDir.Parent;
			}

			//Show the buttons for each choosable file in this directory.
			for (int i = 0; i < data.ChoosableFilesInDir.Count; ++i)
			{
				if (GUILayout.Button(data.ChoosableFilesInDir[i].Name,
									 Gui.Instance.Style_FileBrowser_Files))
				{
					data.CurrentFile = data.ChoosableFilesInDir[i];
				}
			}

			GUILayout.Label("Current file: " + data.CurrentFile.Name,
							Gui.Instance.Style_FileBrowser_Text);

			//Cancel/Select buttons.
			GUILayout.BeginHorizontal();
			if (Input.GetKeyDown(KeyCode.Escape) ||
				GUILayout.Button("Cancel", Gui.Instance.Style_FileBrowser_Buttons))
			{
				data.OnFileChosen(null);
			}
			if (data.CurrentFile != null &&
				GUILayout.Button("Select", Gui.Instance.Style_FileBrowser_Buttons))
			{
				data.OnFileChosen(data.CurrentFile);
			}
			GUILayout.EndHorizontal();
		}


		public List<FileInfo> ChoosableFilesInDir { get; private set; }


		public string[] SupportedExtensions;

		public FileInfo CurrentFile;
		public DirectoryInfo CurrentDir;

		/// <summary>
		/// If the "cancel" button was clicked, "null" is passed into this action.
		/// </summary>
		public Action<FileInfo> OnFileChosen;


		public OpenFileBrowser(string startPath, Rect startWindowPos, GUIContent windowTitle,
							   Action<FileInfo> onFileChosen,
							   params string[] supportedExtensions)
			: base(windowTitle, startWindowPos, GUIWindowCallback, true)
		{
			SupportedExtensions = supportedExtensions;
		
			CurrentFile = null;
			CurrentDir = new DirectoryInfo(startPath);

			OnFileChosen = onFileChosen;

			Refresh();
		}


		public void Refresh()
		{
			CurrentDir.Refresh();

			ChoosableFilesInDir = new List<FileInfo>();
			foreach (string extension in SupportedExtensions)
				ChoosableFilesInDir.AddRange(CurrentDir.GetFiles("*" + extension,
											 SearchOption.TopDirectoryOnly));
		}
	}
}