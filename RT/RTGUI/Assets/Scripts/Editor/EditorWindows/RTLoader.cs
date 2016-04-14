using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEditor;


public class RTLoader : EditorWindow
{
	[MenuItem("RT/Load From File")]
	public static void ShowEditor()
	{
		GetWindow<RTLoader>().Show();
	}

	void OnGUI()
	{
		name = "Load RT file";

		GUILayout.Label("NOTE: Loading a scene file will wipe out all current shapes/materials!");

		if (GUILayout.Button("Load scene"))
		{
			string path = EditorUtility.OpenFilePanel("Choose scene file", Application.dataPath, "xml");
			if (path != null && path != "")
			{
				foreach (RT.RTShape shpe in GameObject.FindObjectsOfType<RT.RTShape>())
					Destroy(shpe.gameObject);


				List<RT.RTMaterial> mats = new List<RT.RTMaterial>();
				List<RT.RTShape> shapes = new List<RT.RTShape>();
				RT.RTSkyMaterial skyMat;
				RT.RTSceneFile.FromFile(path, shapes, mats, out skyMat);
			}
		}
	}
}