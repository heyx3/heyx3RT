using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;


namespace RT.CustomInspectors
{
	[Serializable]
	[CustomEditor(typeof(RTSystem))]
	public class Editor_RTSystem : Editor
	{
		public Texture2D TraceResult = null;
		public float TraceResultScale = 1.0f;
		private GUIStyle titleStyle = null,
						 traceResultStyle = null;


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			RTSystem sys = (RTSystem)target;
			if (sys != RTSystem.Instance)
			{
				GUILayout.Label("There is another 'RTSystem' in this scene!");
				return;
			}

			UpdateCamEffects(sys);

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

			//"Clean up scene" button.
			MyGUI.BeginCompact();
			if (GUILayout.Button("Regenerate scene materials"))
			{
				foreach (RTBaseMaterial mat in FindObjectsOfType<RTBaseMaterial>())
					mat.NullifyMaterial();

				string generatedDir = Path.Combine(Application.dataPath,
												   RTBaseMaterial.GeneratedFolderPath);
				if (Directory.Exists(generatedDir))
					Directory.Delete(generatedDir, true);
				AssetDatabase.Refresh();

				foreach (RTBaseMaterial mat in FindObjectsOfType<RTBaseMaterial>())
					mat.RegenerateMaterial();
			}
			MyGUI.EndCompact();

			GUILayout.Space(25.0f);


			//"Render scene" section.
			//Edit the various rendering options.

			if (titleStyle == null)
			{
				titleStyle = new GUIStyle(GUI.skin.label);
				titleStyle.fontSize = 22;
				titleStyle.fontStyle = FontStyle.Bold;
			}
			GUILayout.Label("Render", titleStyle);

			GUILayout.Space(15.0f);
			
			EditorGUI.BeginChangeCheck();

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

			newF = Math.Max(0.0001f, EditorGUILayout.FloatField("Vertical FOV (degrees)",
																sys.VertFOVDegrees));
			if (newF != sys.VertFOVDegrees)
			{
				Undo.RecordObject(sys, "Inspector");
				sys.VertFOVDegrees = newF;
			}

			newF = EditorGUILayout.FloatField("Aperture Size", sys.Aperture);
			if (newF != sys.Aperture)
			{
				Undo.RecordObject(sys, "Inspector");
				sys.Aperture = newF;
			}

			newF = EditorGUILayout.FloatField("Focus Distance", sys.FocusDist);
			if (newF != sys.FocusDist)
			{
				Undo.RecordObject(sys, "Inspector");
				sys.FocusDist = newF;
			}

			newF = EditorGUILayout.FloatField("Gamma", sys.Gamma);
			if (newF != sys.Gamma)
			{
				Undo.RecordObject(sys, "Inspector");
				sys.Gamma = newF;
			}

			if (EditorGUI.EndChangeCheck())
				sys.OnValidate();


			//"Render" button.
			MyGUI.BeginCompact();
			if (GUILayout.Button("Render"))
			{
				try
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

					string errMsg = sys.ToFile(tempPath);
					if (errMsg.Length > 0)
						throw new Exception(errMsg);

					//Generate the texture.
					Camera c = UnityEditor.SceneView.lastActiveSceneView.camera;
					TraceResult = sys.GenerateImage(c.transform, tempPath);
					if (TraceResult != null)
					{
						TraceResult.filterMode = FilterMode.Point;
						TraceResult.wrapMode = TextureWrapMode.Clamp;
						TraceResult.anisoLevel = 0;
					}

					//Clean up temp files.
					File.Delete(tempPath);
				}
				catch (Exception e)
				{
					Debug.LogError("Unable to render image: (" + e.GetType().Name +
									   ") " + e.Message + "\n" + e.StackTrace);
				}
			}
			MyGUI.EndCompact();

			//Results of the render.
			if (TraceResult != null)
			{
				TraceResultScale = EditorGUILayout.Slider(TraceResultScale, 0.1f, 50.0f);

				//Draw the render.
				if (traceResultStyle == null)
				{
					traceResultStyle = new GUIStyle(GUI.skin.box);
					traceResultStyle.stretchWidth = true;
					traceResultStyle.stretchHeight = true;
				}
				MyGUI.BeginCompact();
				traceResultStyle.normal.background = TraceResult;
				var texSpace = EditorGUILayout.GetControlRect(false,
															  GUILayout.Width(TraceResult.width * TraceResultScale),
															  GUILayout.Height(TraceResult.height * TraceResultScale));
				EditorGUI.DrawPreviewTexture(texSpace, TraceResult, null, ScaleMode.StretchToFill);
				MyGUI.EndCompact();
				
				MyGUI.BeginCompact();
				if (GUILayout.Button("Save render"))
				{
					string filePath = EditorUtility.SaveFilePanel("Choose where to save the image",
																  Application.dataPath, "Img", "png");
					if (filePath != null && filePath != "")
					{
						try
						{
							File.WriteAllBytes(filePath, TraceResult.EncodeToPNG());
						}
						catch (Exception e)
						{
							Debug.LogError("Unable to write image to " + filePath +
											   ": (" + e.GetType() + ") " + e.Message);

							if (Directory.GetFiles(filePath).Length == 0)
								Directory.Delete(filePath, true);
						}
					}
				}
				MyGUI.EndCompact();
			}
		}
		private void UpdateCamEffects(RTSystem sys)
		{
			//Depth-of-Field.
			var dof = sys.EditorCameraProxy.GetComponent<UnityStandardAssets.ImageEffects.DepthOfField>();
			dof.aperture = Mathf.Clamp01(sys.Aperture);
			dof.focalLength = sys.FocusDist;
			dof.focalSize = 0.0f;

			var sceneCam = sys.EditorCameraProxy.SceneCamera;
			if (sceneCam != null)
			{
				sceneCam.fieldOfView = sys.VertFOVDegrees;
				sceneCam.hdr = true;
			}

			sys.EditorCameraProxy.Update();
		}
	}
}