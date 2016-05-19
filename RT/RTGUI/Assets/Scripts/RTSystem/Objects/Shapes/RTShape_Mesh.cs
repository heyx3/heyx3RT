using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ObjLoader.Loader.Loaders;



namespace RT
{
	//TODO: Finish this once a "MeshFile" shape exists in RT.
	public class RTShape_MeshFile : RTShape
	{
		public override string TypeName { get { return TypeName_MeshFile; } }
		public override Mesh UnityMesh
		{
			get
			{
				if (!changed)
					return procMesh;
				if (!File.Exists(MeshPath))
					return null;

				ObjLoaderFactory loaderFact = new ObjLoaderFactory();
				IObjLoader loader = loaderFact.Create();
				
				FileStream stream = new FileStream(MeshPath, FileMode.Open);
				LoadResult scene = loader.Load(stream);
				stream.Close();

				procMesh = new Mesh();
				procMesh.vertices = scene.Vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToArray();
				procMesh.uv = scene.Textures.Select(t => new Vector2(t.X, t.Y)).ToArray();
				procMesh.normals = scene.Normals.Select(n => new Vector3(n.X, n.Y, n.Z)).ToArray();
				procMesh.RecalculateBounds();
				//TODO: Figure out how to set up triangles from LoadResult.
				procMesh.UploadMeshData(true);

				changed = false;
				return procMesh;
			}
		}


		public string MeshPath;

		private bool changed = false;
		private Mesh procMesh = null;


		private RTGui.FileBrowser fileWindow = null;

		public override void DoGUI()
		{
			base.DoGUI();

			GUILayout.Label("File path: " + MeshPath, Gui.Style_Text);
			if (GUILayout.Button("Reload file", Gui.Style_Button))
			{
				changed = true;
				MeshFlt.mesh = UnityMesh;
			}
			if (fileWindow == null && GUILayout.Button("Change file", Gui.Style_Button))
			{
				fileWindow = new RTGui.FileBrowser(MeshPath,
												   new Rect(Screen.width * 0.5f, Screen.height * 0.5f,
															200.0f, 400.0f),
												   new GUIContent("Choose mesh file"),
												   (fle) =>
												   {
													   MeshPath = fle.FullName;
													   changed = true;
												   },
												   Gui.Style_FileBrowser_Files,
												   Gui.Style_FileBrowser_Buttons,
												   Gui.Style_FileBrowser_Text,
												   ".obj");
			}

			if (fileWindow != null)
			{
				fileWindow.DoGUI();
			}
		}

		public override void WriteData(RTSerializer.Writer writer)
		{
			base.WriteData(writer);

			writer.WriteString(MeshPath, "FilePath");
		}
		public override void ReadData(RTSerializer.Reader reader)
		{
			base.ReadData(reader);

			MeshPath = reader.ReadString("FilePath");
			changed = true;
		}
	}
}