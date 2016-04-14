using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEditor;


public class RTSaver : EditorWindow
{
	[MenuItem("RT/Save to File")]
	public static void ShowEditor()
	{
		GetWindow<RTSaver>().Show();
	}

	public static void SaveSceneTo(string path)
	{
		List<RT.RTMaterial> mats = FindObjectsOfType<RT.RTMaterial>().ToList();
		List<RT.RTShape> shapes = FindObjectsOfType<RT.RTShape>().ToList();
		RT.RTSkyMaterial skyMat = FindObjectOfType<RT.RTSkyMaterial>();
		RT.RTSceneFile.ToFile(path, shapes, mats, skyMat);
	}
	
	
	void OnGUI()
	{
		name = "Save RT file";

		if (GUILayout.Button("Save scene"))
		{
			string path = EditorUtility.SaveFilePanel("Choose file location", Application.dataPath,
			                                          "MyScene.xml", "xml");
			if (path != null && path != "")
			{
				SaveSceneTo(path);
			}
		}
	}
}