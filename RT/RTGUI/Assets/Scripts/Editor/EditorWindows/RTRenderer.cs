using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEditor;


public class RTRenderer : EditorWindow
{
	[MenuItem("RT/Render the Scene")]
	public static void ShowEditor()
	{
		GetWindow<RTRenderer>().Show();
	}
	
	private static string _execPath = null;


	public int NSamples = 100,
	           NBounces = 50,
	           NThreads = 8;
	public float Gamma = 2.2f;
	public int OutputWidth, OutputHeight;

	
	void OnGUI()
	{
		NSamples = EditorGUILayout.IntField("Samples per pixel", NSamples);
		NBounces = EditorGUILayout.IntField("Max bounces per ray", NBounces);
		NThreads = EditorGUILayout.IntField("Threads to use", NThreads);

		EditorGUILayout.Space();

		Gamma = EditorGUILayout.FloatField("Gamma", Gamma);
		
		EditorGUILayout.Space();
		
		OutputWidth = EditorGUILayout.IntField("Output image width", OutputWidth);
		OutputHeight = EditorGUILayout.IntField("Output image height", OutputHeight);

		EditorGUILayout.Space();


		if (GUILayout.Button("Render Scene"))
		{
			string imgPath = EditorUtility.SaveFilePanel("Choose output image path", Application.dataPath,
			                                          "MyImg.bmp", "bmp");
			if (imgPath != null && imgPath != "")
			{
				string execPath = EditorUtility.OpenFilePanel("Choose the RT executable",
				                                              (_execPath == null ?
				                                                   Application.dataPath :
				                                                   _execPath),
				                                              "exe");
				if (execPath != null && execPath != "")
				{
					_execPath = execPath;

					string sceneFile = Path.Combine(Application.dataPath, "__TempScene.xml");
					RTSaver.SaveSceneTo(sceneFile);

					Transform cam = SceneView.lastActiveSceneView.camera.transform;
					Vector3 camP = cam.position,
					        camF = cam.forward,
					        camU = cam.up;

					System.Diagnostics.Process.Start(execPath,
					                                 "-nThreads " + NThreads +
					                                  " -cPos " + camP.x + " " + camP.y + " " + camP.z +
					                                  " -cForward " + camF.x + " " + camF.y + " " + camF.z +
					                                  " -cUp " + camU.x + " " + camU.y + " " + camU.z +
					                                  " -gamma " + Gamma +
					                                  " -nSamples " + NSamples +
					                                  " -nBounces " + NBounces +
					                                  " -outputPath \"" + imgPath + "\"" +
					                                  " -outputSize " + OutputWidth + " " + OutputHeight +
					                                  " -scene \"" + sceneFile + "\"");
				}
			}
		}
	}
}