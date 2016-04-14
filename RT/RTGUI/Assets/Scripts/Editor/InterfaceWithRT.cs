using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public static class InterfaceWithRT
{
	public static void InitShape(GameObject shpe)
	{
		RT.RTShape rtShpe = shpe.GetComponent<RT.RTShape>();
		RT.RTMaterial rtMat = shpe.GetComponent<RT.RTMaterial>();

		MeshFilter mf = shpe.GetComponent<MeshFilter>();
		if (mf == null)
			mf = shpe.AddComponent<MeshFilter>();
		mf.mesh = rtShpe.GetUnityMesh();
		
		MeshRenderer mr = shpe.GetComponent<MeshRenderer>();
		if (mr == null)
			mr = shpe.AddComponent<MeshRenderer>();

		mr.sharedMaterial = rtMat.GetUnityMat();
		rtMat.SetUnityMatParams(mr.sharedMaterial);
	}

	private static bool CheckForRTSystem()
	{
		if (RT.RTSystem.Instance == null)
		{
			Debug.LogError("No RTSystem component exists!");
			return false;
		}
		if (RT.RTSystem.Instance.Shape_Sphere == null)
		{
			Debug.LogError("RTSystem.Shape_Sphere is null");
			return false;
		}
		if (RT.RTSystem.Instance.Mat_Lambert == null)
		{
			Debug.LogError("RTSystem.Mat_Lambert is null");
			return false;
		}
		if (RT.RTSystem.Instance.Mat_Metal == null)
		{
			Debug.LogError("RTSystem.Mat_Metal is null");
			return false;
		}
		if (RT.RTSystem.Instance.SkyMat_SolidColor == null)
		{
			Debug.LogError("RTSystem.SkyMat_SolidColor is null");
			return false;
		}
		if (RT.RTSystem.Instance.SkyMat_VerticalGradient == null)
		{
			Debug.LogError("RTSystem.SkyMat_VerticalGradient is null");
			return false;
		}
		return true;
	}

	
	[MenuItem("RT/Create Sphere")]
	public static void CreateSphere()
	{
		if (!CheckForRTSystem())
			return;

		GameObject go = new GameObject("Sphere");
		go.AddComponent<RT.RTMaterial_Lambert>();
		go.AddComponent<RT.RTShape_Sphere>();
		InitShape(go);
		
		Selection.activeObject = go;
	}
	[MenuItem("RT/Create Plane")]
	public static void CreatePlane()
	{
		if (!CheckForRTSystem())
			return;

		GameObject go = new GameObject("Plane");
		go.AddComponent<RT.RTMaterial_Lambert>();
		go.AddComponent<RT.RTShape_Plane>();
		InitShape(go);
		
		Selection.activeObject = go;
	}
	[MenuItem("RT/Create Mesh")]
	public static void CreateMesh()
	{
		if (!CheckForRTSystem())
			return;

		GameObject go = new GameObject("Mesh");
		go.AddComponent<RT.RTMaterial_Lambert>();
		go.AddComponent<RT.RTShape_Mesh>();
		InitShape(go);
		
		Selection.activeObject = go;
	}
	
	[MenuItem("RT/Create Sky/Simple Color")]
	public static void CreateSky_SimpleCol()
	{
		if (!CheckForRTSystem())
			return;

		if (GameObject.FindObjectOfType<RT.RTSkyMaterial>() != null)
		{
			Debug.LogError("Already have a sky material in the scene!");
			return;
		}

		GameObject go = new GameObject("Sky");
		go.transform.localScale = Vector3.one * 50.0f;
		RT.RTSkyMaterial mat = go.AddComponent<RT.RTSkyMaterial_SimpleColor>();
		go.AddComponent<MeshFilter>().sharedMesh = RT.RTSystem.Instance.SkySphere;

		go.AddComponent<MeshRenderer>().sharedMaterial = mat.GetUnityMat();
		mat.SetUnityMatParams(go.GetComponent<MeshRenderer>().sharedMaterial);
	}
	[MenuItem("RT/Create Sky/Vertical Gradient")]
	public static void CreateSky_VerticalGradient()
	{
		if (!CheckForRTSystem())
			return;

		if (GameObject.FindObjectOfType<RT.RTSkyMaterial>() != null)
		{
			Debug.LogError("Already have a sky material in the scene!");
			return;
		}
		
		GameObject go = new GameObject("Sky");
		go.transform.localScale = Vector3.one * 50.0f;
		RT.RTSkyMaterial mat = go.AddComponent<RT.RTSkyMaterial_VerticalGradient>();
		go.AddComponent<MeshFilter>().sharedMesh = RT.RTSystem.Instance.SkySphere;

		go.AddComponent<MeshRenderer>().sharedMaterial = mat.GetUnityMat();
		mat.SetUnityMatParams(go.GetComponent<MeshRenderer>().sharedMaterial);
	}


	[MenuItem("CONTEXT/RTMaterial/Change to Lambert")]
	public static void ChangeToLambert(MenuCommand cmd)
	{
		RT.RTMaterial oldMat = (RT.RTMaterial)cmd.context;

		GameObject go = oldMat.gameObject;
		GameObject.DestroyImmediate(oldMat);

		RT.RTMaterial_Lambert lmbt = go.AddComponent<RT.RTMaterial_Lambert>();
	}
	[MenuItem("CONTEXT/RTMaterial/Change to Lambert", true)]
	private static bool ChangeToLambertValidation(MenuCommand cmd)
	{
		return !(cmd.context is RT.RTMaterial_Lambert);
	}
	
	[MenuItem("CONTEXT/RTMaterial/Change to Metal")]
	public static void ChangeToMetal(MenuCommand cmd)
	{
		RT.RTMaterial oldMat = (RT.RTMaterial)cmd.context;
		
		GameObject go = oldMat.gameObject;
		GameObject.DestroyImmediate(oldMat);
		
		RT.RTMaterial_Metal mtl = go.AddComponent<RT.RTMaterial_Metal>();
	}
	[MenuItem("CONTEXT/RTMaterial/Change to Metal", true)]
	private static bool ChangeToMetalValidation(MenuCommand cmd)
	{
		return !(cmd.context is RT.RTMaterial_Metal);
	}
	
	
	[MenuItem("CONTEXT/RTSkyMaterial/Change to Simple Color")]
	public static void ChangeToSimpleColor(MenuCommand cmd)
	{
		RT.RTSkyMaterial oldMat = (RT.RTSkyMaterial)cmd.context;
		
		GameObject go = oldMat.gameObject;
		GameObject.DestroyImmediate(oldMat);
		
		RT.RTSkyMaterial_SimpleColor sc = go.AddComponent<RT.RTSkyMaterial_SimpleColor>();
	}
	[MenuItem("CONTEXT/RTSkyMaterial/Change to Simple Color", true)]
	public static bool ChangeToSimpleColorValidation(MenuCommand cmd)
	{
		return !(cmd.context is RT.RTSkyMaterial_SimpleColor);
	}
	
	[MenuItem("CONTEXT/RTSkyMaterial/Change to Vertical Gradient")]
	public static void ChangeToVertGrad(MenuCommand cmd)
	{
		RT.RTSkyMaterial oldMat = (RT.RTSkyMaterial)cmd.context;
		
		GameObject go = oldMat.gameObject;
		GameObject.DestroyImmediate(oldMat);
		
		RT.RTSkyMaterial_VerticalGradient vg = go.AddComponent<RT.RTSkyMaterial_VerticalGradient>();
	}
	[MenuItem("CONTEXT/RTSkyMaterial/Change to Simple Color", true)]
	public static bool ChangeToVertGradValidation(MenuCommand cmd)
	{
		return !(cmd.context is RT.RTSkyMaterial_VerticalGradient);
	}
}