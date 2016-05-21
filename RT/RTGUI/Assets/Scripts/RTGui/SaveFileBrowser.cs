using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace RTGui
{
	public class SaveFileBrowser : ManagedWindow
	{
		private static void GUIWindowCallback(int id)
		{
			SaveFileBrowser data = Get<SaveFileBrowser>(id);
		
			//Show the "parent directory" button.
			if (data.CurrentDir.Parent != null &&
				GUILayout.Button("..", Gui.Instance.Style_FileBrowser_Files))
			{
				data.CurrentDir = data.CurrentDir.Parent;
			}

			//Show the buttons for each file in this directory.
			for (int i = 0; i < data.FilesInDir.Count; ++i)
			{
				if (GUILayout.Button(data.FilesInDir[i].Name,
									 Gui.Instance.Style_FileBrowser_Files))
				{
					data.FileName = data.FilesInDir[i].Name;
				}
			}

			data.FileName = GUILayout.TextField(data.FileName,
												Gui.Instance.Style_FileBrowser_TextBox);

			//Cancel/Select buttons.
			GUILayout.BeginHorizontal();
			if (Input.GetKeyDown(KeyCode.Escape) ||
				GUILayout.Button("Cancel", Gui.Instance.Style_FileBrowser_Buttons))
			{
				data.OnFileChosen(null);
			}
			if (GUILayout.Button("Select", Gui.Instance.Style_FileBrowser_Buttons))
			{
				data.OnFileChosen(new FileInfo(Path.Combine(data.CurrentDir.FullName, data.FileName)));
			}
			GUILayout.EndHorizontal();
		}


		public List<FileInfo> FilesInDir { get; private set; }

		public string FileName;
		public string Extension;

		public DirectoryInfo CurrentDir;

		/// <summary>
		/// If the "cancel" button was clicked, "null" is passed into this action.
		/// </summary>
		public Action<FileInfo> OnFileChosen;


		public SaveFileBrowser(string startFileFullName, Rect startWindowPos,
							   GUIContent windowTitle,
							   Action<FileInfo> onFileChosen, string extension)
			: base(windowTitle, startWindowPos, GUIWindowCallback, true)
		{
			Extension = extension;
			OnFileChosen = onFileChosen;

			FileName = Path.GetFileName(startFileFullName);
			CurrentDir = new DirectoryInfo(Path.GetDirectoryName(startFileFullName));
			Refresh();
		}


		public void Refresh()
		{
			CurrentDir.Refresh();
			FilesInDir = new List<FileInfo>(CurrentDir.GetFiles("*" + Extension));
		}
	}
}