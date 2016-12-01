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
									  Input_ScreenPos = "(IN.screenPos.xy)",
									  Input_UV = "(IN.screenPos.zw)",
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
		public float FovScale = 1.0f,
					 Gamma = 2.2f;


		public void OnValidate()
		{
			//Reinitialize all shapes/materials so that they definitely are using the correct meshes.

			if (RTSkyMaterial.Instance != null)
				RTSkyMaterial.Instance.Awake();

			foreach (RTShape shpe in RTShape.Shapes)
			{
				shpe.Awake();
				shpe.GetComponent<RTMaterial>().Awake();
			}
		}

		/// <summary>
		/// Saves the scene to the given JSON file.
		/// Returns an error message, or the empty string if everything went fine.
		/// </summary>
		public string ToFile(string filePath, string rootObjectName)
		{
			Scene scene = new Scene();

			using (Serialization.JSONWriter writer = new Serialization.JSONWriter(filePath))
			{
				string errMsg = "";
				try
				{
					writer.Structure(scene, rootObjectName);
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
		public string FromFile(string filePath, string rootObjectName)
		{
			string errMsg = "";
			try
			{
				//Read all objects into a new scene.
				var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
							       UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
				UnityEditor.SceneManagement.EditorSceneManager.SetActiveScene(newScene);

				//Do the actual reading.
				Serialization.JSONReader reader = new Serialization.JSONReader(filePath);
				reader.Structure(new RT.Scene(), rootObjectName);
			}
			catch (Exception e)
			{
				errMsg = "Error reading scene from " + filePath +
							 ": (" + e.GetType() + ") " + e.Message;
			}

			return errMsg;
		}

		/// <summary>
		/// If something went wrong, returns null and prints an error.
		/// </summary>
		public Texture2D GenerateImage(Transform cam, string sceneJSON, string sceneJSONRootName)
		{
			Texture2D tex = new Texture2D(ImgSizeX, ImgSizeY, TextureFormat.RGBA32, false);

			string err = C_API.rt_GenerateImage(tex, (uint)SamplesPerPixel, (uint)MaxBounces,
												(uint)NThreads, FovScale, Gamma,
									 		    cam.position, cam.forward, cam.up,
												sceneJSON, sceneJSONRootName);
			if (err.Length > 0)
			{
				Debug.LogError(err);
				return null;
			}

			return tex;
		}
	}
}