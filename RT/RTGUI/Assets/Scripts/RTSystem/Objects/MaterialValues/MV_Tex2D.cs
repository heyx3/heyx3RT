using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace RT
{
	public class MV_Tex2D : MaterialValue
	{
		private static Texture2D Load(string filePath)
		{
			if (!File.Exists(filePath))
				return null;

			Texture2D tex = null;

			string ext = Path.GetExtension(filePath);
			if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
			{
				tex = new Texture2D(1, 1);
				tex.LoadImage(File.ReadAllBytes(filePath), true);
			}

			return tex;
		}


		public override string TypeName { get { return TypeName_Tex2D; } }
		
		public string TexturePath = null;

		private Texture2D loadedTex = null;
		private RTGui.FileBrowser fileBrowser = null;


		public MV_Tex2D(string texPath = null) : this(new MV_SurfUV(), texPath) { }
		public MV_Tex2D(MaterialValue uv, string texPath = null)
		{
			AddChild(uv);
			TexturePath = texPath;

			loadedTex = Load(TexturePath);
		}


		protected override void DoGUI()
		{
			base.DoGUI();

			if (fileBrowser == null)
			{
				if (loadedTex != null)
				{
					GUILayout.Box(loadedTex, Gui.Style_MValTexture,
								  GUILayout.MaxWidth(Gui.MaxTexPreviewSize.x),
								  GUILayout.MaxHeight(Gui.MaxTexPreviewSize.y));

					if (GUILayout.Button("Reload", Gui.Style_Button))
						loadedTex = Load(TexturePath);
				}

				if (GUILayout.Button("Change"))
				{
					fileBrowser = new RTGui.FileBrowser((TexturePath == null ?
															Application.dataPath :
															TexturePath),
														new Rect(),
														new GUIContent("Choose new texture file"),
														(fle) =>
														{
															loadedTex = (fle == null ?
																			null :
																			Load(fle.FullName));
															fileBrowser.Release();
															fileBrowser = null;
														},
														Gui.Style_FileBrowser_Files,
														Gui.Style_FileBrowser_Buttons,
														Gui.Style_FileBrowser_Text,
														".png", ".jpg", ".jpeg");
				}
			}
			else
			{
				GUILayout.Label("Waiting for file browser...", Gui.Style_Text);
				fileBrowser.DoGUI();
			}
		}
		protected override string GetInputName(int index) { return "UV"; }

		public override void SetMaterialParams(Material mat,
											   string texParamName = null,
											   string colorParamName = null)
		{
			if (colorParamName != null)
				mat.SetColor(colorParamName, Color.white);
			if (texParamName != null && loadedTex != null)
				mat.SetTexture(texParamName, loadedTex);
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);

			writer.WriteString(TexturePath, "FilePath");
			writer.WriteString("Automatic", "FileType");

			Write(GetChild(0), writer, "UVs");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);

			TexturePath = reader.ReadString("FilePath");
			loadedTex = Load(TexturePath);

			ClearChildren();
			AddChild(Read(reader, "UVs"));
		}
	}
}