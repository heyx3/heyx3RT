using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


namespace RT.CustomInspectors
{
	//TODO: A button to generate and show the ray-traced image right in the inspector.


	[Serializable]
	[CustomEditor(typeof(RTSystem))]
	public class Editor_RTSystem : Editor
	{
		public bool ShowSceneOptions = false;


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			RTSystem sys = (RTSystem)target;
			if (sys != RTSystem.Instance)
			{
				GUILayout.Label("There is another 'RTSystem' in this scene!");
				return;
			}

			GUILayout.Space(15.0f);
			
			//"Save scene" button.
			MyGUI.BeginCompact();
			if (GUILayout.Button("Save scene to file"))
			{
				string filePath = EditorUtility.SaveFilePanel("Choose where to save the scene",
															  Application.dataPath, "Scene", "json");
				if (filePath != null && filePath != "")
				{
					string err = sys.ToFile(filePath);
					if (err.Length > 0)
						EditorUtility.DisplayDialog("Error saving scene file", err, "OK");
				}
			}
			MyGUI.EndCompact();
			
			//"Load scene" button.
			MyGUI.BeginCompact();
			if (GUILayout.Button("Load scene from file"))
			{
				string filePath = EditorUtility.OpenFilePanel("Choose the scene to load",
															  Application.dataPath, "json");
				if (filePath != null && filePath != "")
				{
					string err = sys.FromFile(filePath);
					if (err.Length > 0)
						EditorUtility.DisplayDialog("Error loading scene file", err, "OK");
				}
			}
			MyGUI.EndCompact();

			GUILayout.Space(15.0f);
			
			//"Render scene" section.
			ShowSceneOptions = EditorGUILayout.Foldout(ShowSceneOptions, "Render scene to file");
			if (ShowSceneOptions)
			{
				MyGUI.BeginTab(15.0f);

				//Edit the various rendering options.

				int newI;
				float newF;

				newI = Math.Max(1, EditorGUILayout.IntField("Image width", sys.ImgSizeX));
				if (newI != sys.ImgSizeX)
				{
					Undo.RecordObject(sys, "Inspector");
					sys.ImgSizeX = newI;
				}

				newI = Math.Max(1, EditorGUILayout.IntField("Image height", sys.ImgSizeY));
				if (newI != sys.ImgSizeY)
				{
					Undo.RecordObject(sys, "Inspector");
					sys.ImgSizeY = newI;
				}

				newI = Math.Max(1, EditorGUILayout.IntField("Number of threads", sys.NThreads));
				if (newI != sys.NThreads)
				{
					Undo.RecordObject(sys, "Inspector");
					sys.NThreads = newI;
				}

				newI = Math.Max(1, EditorGUILayout.IntField("Samples per pixel", sys.SamplesPerPixel));
				if (newI != sys.SamplesPerPixel)
				{
					Undo.RecordObject(sys, "Inspector");
					sys.SamplesPerPixel = newI;
				}

				newI = Math.Max(0, EditorGUILayout.IntField("Max ray bounces", sys.MaxBounces));
				if (newI != sys.MaxBounces)
				{
					Undo.RecordObject(sys, "Inspector");
					sys.MaxBounces = newI;
				}

				newF = Math.Max(0.0001f, EditorGUILayout.FloatField("FOV scale", sys.FovScale));
				if (newF != sys.FovScale)
				{
					Undo.RecordObject(sys, "Inspector");
					sys.FovScale = newF;
				}

				newF = EditorGUILayout.FloatField("Gamma", sys.Gamma);
				if (newF != sys.Gamma)
				{
					Undo.RecordObject(sys, "Inspector");
					sys.Gamma = newF;
				}


				//The "Render scene" button.
				MyGUI.BeginCompact();
				if (GUILayout.Button("Render scene"))
				{
					string filePath = EditorUtility.SaveFilePanel("Choose where to save the image",
																  Application.dataPath, "Img", "png");
					if (filePath != null && filePath != "")
					{
						//Write the JSON to a temporary file.

						string tempDir = Path.Combine(Application.dataPath, "..\\TempJSON");
						if (!Directory.Exists(tempDir))
							Directory.CreateDirectory(tempDir);

						int i = 0;
						string tempPath = Path.Combine(tempDir, i.ToString() + ".json");
						while (File.Exists(tempPath))
						{
							i += 1;
							tempPath = Path.Combine(tempDir, i.ToString() + ".json");
						}

						sys.ToFile(tempPath);

						//Generate the texture.
						Camera c = UnityEditor.SceneView.lastActiveSceneView.camera;
						Texture2D tex = sys.GenerateImage(c.transform, tempPath);
						if (tex != null)
						{
							try
							{
								File.WriteAllBytes(filePath, tex.EncodeToPNG());
							}
							catch (Exception e)
							{
								Debug.LogError("Unable to write image to " + filePath +
											       ": (" + e.GetType() + ") " + e.Message);
							}
						}

						//Clean up temp files.
						File.Delete(tempPath);
						if (Directory.GetFiles(filePath).Length == 0)
							Directory.Delete(filePath, true);
					}
				}
				MyGUI.EndCompact();

				MyGUI.EndTab();
			}
		}
	}
}