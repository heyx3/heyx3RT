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
				//TODO: Figure out how to set up triangles.
				procMesh.UploadMeshData(true);

				changed = false;
				return procMesh;
			}
		}


		public string MeshPath;

		private bool changed = false;
		private Mesh procMesh = null;


		//TODO: Use GUIUtil's file browser stuff instead.
		private class FileWindowData
		{
			public string CurrentPath;
			public DirectoryInfo CurrentDir;
			public Rect CurrentWindowPos;
			public int ID;
		}

		private FileWindowData fileWindow = null;


		protected override void DoGUI()
		{
			base.DoGUI();

			GUILayout.Label("File path: " + MeshPath);
			if (GUILayout.Button("Reload file"))
			{
				changed = true;
				MeshFlt.mesh = UnityMesh;
			}
			if (fileWindow == null && GUILayout.Button("Change file"))
			{
				fileWindow = new FileWindowData();
				fileWindow.CurrentPath = MeshPath;
				fileWindow.CurrentDir = new DirectoryInfo(fileWindow.CurrentPath);
				fileWindow.CurrentWindowPos = new Rect(Screen.width * 0.5f, Screen.height * 0.5f,
													   200.0f, 400.0f);
				fileWindow.ID = GetInstanceID();
			}

			if (fileWindow != null)
			{
				fileWindow.CurrentWindowPos = GUILayout.Window(fileWindow.ID, fileWindow.CurrentWindowPos,
															   GUIFileWindow, "Mesh");
			}
		}
		private void GUIFileWindow(int id)
		{
			//TODO: File window GUI.
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