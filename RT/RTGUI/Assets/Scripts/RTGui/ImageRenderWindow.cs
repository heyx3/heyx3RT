using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;


namespace RTGui
{
	public class ImageRenderWindow : ManagedWindow
	{
		private static void GUIWindowCallback(int id)
		{
			ImageRenderWindow data = Get<ImageRenderWindow>(id);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Image width", Gui.Instance.Style_Text);
			GUILayout.FlexibleSpace();
			data.Width = GUIUtil.TextEditor(data.Width, ref data.widthStr, Gui.Instance.Style_TextBox);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Image height", Gui.Instance.Style_Text);
			GUILayout.FlexibleSpace();
			data.Height = GUIUtil.TextEditor(data.Height, ref data.heightStr, Gui.Instance.Style_TextBox);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Samples per pixel", Gui.Instance.Style_Text);
			GUILayout.FlexibleSpace();
			data.SamplesPerPixel = GUIUtil.TextEditor(data.SamplesPerPixel, ref data.samplesStr,
													  Gui.Instance.Style_TextBox);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Max ray bounces", Gui.Instance.Style_Text);
			GUILayout.FlexibleSpace();
			data.MaxBounces = GUIUtil.TextEditor(data.MaxBounces, ref data.bouncesStr,
												 Gui.Instance.Style_TextBox);
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Threads to use", Gui.Instance.Style_Text);
			GUILayout.FlexibleSpace();
			data.Threads = GUIUtil.TextEditor(data.Threads, ref data.threadsStr,
											  Gui.Instance.Style_TextBox);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Field-of-View Scale", Gui.Instance.Style_Text);
			GUILayout.FlexibleSpace();
			data.FovScale = GUIUtil.TextEditor(data.FovScale, ref data.fovScaleStr,
											   Gui.Instance.Style_TextBox);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Gamma", Gui.Instance.Style_Text);
			GUILayout.FlexibleSpace();
			data.Gamma = GUIUtil.TextEditor(data.Gamma, ref data.gammaStr,
											Gui.Instance.Style_TextBox);
			GUILayout.EndHorizontal();
			

			GUILayout.Space(10.0f);


			if (data.saveImgBrowser == null)
			{
				if (GUILayout.Button("Generate Image", Gui.Instance.Style_Button))
				{
					data.saveImgBrowser = new SaveFileBrowser(Path.Combine(Application.dataPath,
																		   "MyImg.png"),
															  new Rect(),
															  new GUIContent("Choose where to save the image"),
															  (fi) =>
															  {
																  if (fi != null)
																	data.GeneratePNG(fi.FullName);
																  data.saveImgBrowser.Release();
																  data.saveImgBrowser = null;
															  },
															  ".png");
				}

				GUILayout.Space(10.0f);

				if (GUILayout.Button("Cancel", Gui.Instance.Style_Button))
				{
					data.OnCanceled();
				}
			}

			GUI.DragWindow();
		}


		public Action OnCanceled;

		public uint Width = 800, Height = 600;
		public uint SamplesPerPixel = 500,
				    MaxBounces = 50,
					Threads = 4;
		public float Gamma = 2.2f,
					 FovScale = 1.0f;

		private string widthStr, heightStr, samplesStr, bouncesStr, threadsStr, fovScaleStr, gammaStr;

		private SaveFileBrowser saveImgBrowser = null;


		public ImageRenderWindow(Rect startPos, GUIContent windowTitle, Action onCanceled)
			: base(windowTitle, startPos, GUIWindowCallback, true)
		{
			widthStr = Width.ToString();
			heightStr = Height.ToString();
			samplesStr = SamplesPerPixel.ToString();
			bouncesStr = MaxBounces.ToString();
			threadsStr = Threads.ToString();
			fovScaleStr = FovScale.ToString();
			gammaStr = Gamma.ToString();

			OnCanceled = onCanceled;
		}


		public void GeneratePNG(string path)
		{
			string jsonPath = Path.Combine(Application.dataPath, "TempSceneJSON.json");
			
			RTSerializer.JsonWriter writer = new RTSerializer.JsonWriter();
			writer.WriteDataStructure(RT.Tracer.Instance, "data");
			string errMsg = writer.SaveData(jsonPath, true);
			if (errMsg != null)
			{
				Debug.LogError("Error saving JSON file: " + errMsg);
				return;
			}


			if (Camera.main == null)
			{
				Debug.LogError("Can't find camera tagged with 'MainCamera'");
				return;
			}

			Transform cam = Camera.main.transform;

			Color[] cols = RT_API.GenerateImage(Width, Height,
												SamplesPerPixel, MaxBounces, Threads, FovScale, Gamma,
												cam.position, cam.forward, cam.up,
												jsonPath, "data");

			if (cols == null)
			{
				Debug.LogError("Unable to generate: " + RT_API.ErrorMsg);
				return;
			}

			Texture2D tex = new Texture2D((int)Width, (int)Height);
			Debug.Log(tex.width.ToString() + "x" + tex.height + " = " + cols.Length);
			tex.SetPixels(cols);
			tex.Apply(false, false);

			try
			{
				File.Delete(jsonPath);
			}
			catch (Exception e)
			{
				Debug.LogError("Error deleting temp file " + jsonPath + ": " + e.Message);
			}

			try
			{
				File.WriteAllBytes(path, tex.EncodeToPNG());
			}
			catch (Exception e)
			{
				Debug.LogError("Error saving texture file: " + e.Message);
				return;
			}


			//Start up a file explorer pointed at the texture.
			switch (Application.platform)
			{
				case RuntimePlatform.WindowsEditor:
				case RuntimePlatform.WindowsPlayer:
				case RuntimePlatform.WindowsWebPlayer:
					System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
					break;

				case RuntimePlatform.OSXEditor:
				case RuntimePlatform.OSXDashboardPlayer:
				case RuntimePlatform.OSXPlayer:
				case RuntimePlatform.OSXWebPlayer:
					try
					{
						System.Diagnostics.Process proc = new System.Diagnostics.Process();
						proc.StartInfo.FileName = "open";
						proc.StartInfo.Arguments = "-n -R \"" + path + "\"";
						proc.StartInfo.UseShellExecute = false;
						proc.StartInfo.RedirectStandardError = false;
						proc.StartInfo.RedirectStandardOutput = false;
						proc.ErrorDataReceived += (s, a) => Debug.LogError(a.Data);
						if (proc.Start())
						{
							proc.BeginErrorReadLine();
							proc.BeginOutputReadLine();
						}
						else
						{
							Debug.LogError("Error opening Finder to show texture file");
						}
					}
					catch (Exception e)
					{
						Debug.LogError("Error opening Finder to show texture file: " + e.Message);
					}
					break;

				default: break;
			}
		}
	}
}