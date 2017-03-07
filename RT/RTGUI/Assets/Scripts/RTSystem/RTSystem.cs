using System;
using UnityEngine;
using UnityEditor;


namespace RT
{
	//TODO: Use Unity's lighting/skybox system instead of a custom skybox mesh: https://en.wikibooks.org/wiki/Cg_Programming/Unity/Skyboxes#Shader_Code_for_Unity.27s_Skybox_System

	[ExecuteInEditMode]
	public class RTSystem : MonoBehaviour
	{
		public static RTSystem Instance
		{
			get { if (rtSys == null) rtSys = FindObjectOfType<RTSystem>(); return rtSys; }
		}
		private static RTSystem rtSys = null;


		public static readonly string Param_PureNoiseTex = "_PureNoise",
									  Param_ShapePos = "_ShapePos",
									  Param_ShapeScale = "_ShapeScale",
									  Param_ShapeRot = "_ShapeRot";
		public static readonly string Input_WorldPos = "IN.worldPos",
									  Input_ScreenPos = "(IN.rtUV_ScreenPos.zw)",
									  Input_UV = "(IN.rtUV_ScreenPos.xy)",
									  Input_WorldNormal = "IN.worldNormal",
									  Input_Tangent = "(IN.tangent.xyz)",
									  Input_Bitangent = "(cross(IN.worldNormal, IN.tangent.xyz) * IN.tangent.w)",
									  Input_CamPos = "_WorldSpaceCameraPos",
									  Input_RayDir = "normalize(IN.worldPos - _WorldSpaceCameraPos)";


		public Mesh Shape_Sphere, Shape_Plane,
					SkySphere;
		
		public Texture2D PureNoiseTex;


		[HideInInspector]
		public int ImgSizeX = 800,
				   ImgSizeY = 600,
				   NThreads = 4,
				   SamplesPerPixel = 100,
				   MaxBounces = 50;
		[HideInInspector]
		public float VertFOVDegrees = 60.0f,
					 Gamma = 2.2f,
					 Aperture = 0.0f,
					 FocusDist = 0.0f;


		public SceneViewCameraProxy EditorCameraProxy
		{
			get
			{
				if (editorCamProxy == null)
					editorCamProxy = FindObjectOfType<SceneViewCameraProxy>();
				return editorCamProxy;
			}
		}
		private SceneViewCameraProxy editorCamProxy = null;


		public void OnValidate()
		{
			NThreads = Math.Max(1, NThreads);
			ImgSizeY = Math.Max(ImgSizeY, NThreads);
			SamplesPerPixel = Math.Max(0, SamplesPerPixel);
			MaxBounces = Math.Max(0, MaxBounces);
			Gamma = Math.Max(0.0f, Gamma);
			Aperture = Math.Max(0.0f, Aperture);
			FocusDist = Math.Max(0.0f, FocusDist);
		}
		
		/// <summary>
		/// Saves the scene to the given JSON file.
		/// Returns an error message, or the empty string if everything went fine.
		/// </summary>
		public string ToFile(string filePath)
		{
			Scene scene = new Scene();

			using (Serialization.JSONWriter writer = new Serialization.JSONWriter(filePath))
			{
				string errMsg = "";
				try
				{
					writer.Structure(scene, "data");
				}
				catch (Exception e)
				{
					errMsg = "Error writing scene to " + filePath +
						         ": (" + e.GetType() + ") " + e.Message;
				}

				return errMsg;
			}
		}
		/// <summary>
		/// Loads a scene from the given JSON file.
		/// Doesn't change anything currently in the scene.
		/// Returns an error message, or the empty string if everything went fine.
		/// </summary>
		public string FromFile(string filePath)
		{
			string errMsg = "";
			try
			{
				//Remove all objects from the current scene except for reflection probes.
				foreach (GameObject go in FindObjectsOfType<GameObject>())
				{
					if (go.transform.parent == null && go != gameObject &&
						go.GetComponentInChildren<ReflectionProbe>() == null)
					{
						DestroyImmediate(go);
					}
				}

				//Read in the new scene.
				Serialization.JSONReader reader = new Serialization.JSONReader(filePath);
				reader.Structure(new RT.Scene(), "data");
			}
			catch (Exception e)
			{
				errMsg = "Error reading scene from " + filePath +
							 ": (" + e.GetType() + ") " + e.Message + "\n\n" + e.StackTrace;
			}

			return errMsg;
		}

		/// <summary>
		/// If something went wrong, returns null and prints an error.
		/// </summary>
		public Texture2D GenerateImage(Transform cam, string sceneJSON)
		{
			Texture2D tex = new Texture2D(ImgSizeX, ImgSizeY, TextureFormat.RGBA32, false);

			string err = C_API.GenerateImage(tex, (uint)SamplesPerPixel, (uint)MaxBounces,
											 (uint)NThreads, VertFOVDegrees, Aperture, FocusDist,
									 		 cam.position, cam.forward, cam.up,
											 sceneJSON);
			if (err.Length > 0)
			{
				Debug.LogError(err);
				return null;
			}

			//Gamma-correct.
			if (Gamma != 1.0f)
			{
				Color[] pixels = tex.GetPixels();
				float gammaPow = 1.0f / Gamma;
				for (int i = 0; i < pixels.Length; ++i)
				{
					pixels[i] = new Color(Mathf.Pow(pixels[i].r, gammaPow),
										  Mathf.Pow(pixels[i].g, gammaPow),
										  Mathf.Pow(pixels[i].b, gammaPow),
										  pixels[i].a);
				}
				tex.SetPixels(pixels);
				tex.Apply(true, false);
			}

			return tex;
		}
	}
}