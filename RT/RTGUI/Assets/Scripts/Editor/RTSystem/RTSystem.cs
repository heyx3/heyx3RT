using System;
using UnityEngine;


namespace RT
{
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
		
		public Material Mat_Lambert, Mat_Metal;
		public Material SkyMat_SolidColor, SkyMat_VerticalGradient;

		public Texture2D PureNoiseTex;


		/// <summary>
		/// Saves the scene to the given JSON file.
		/// Returns an error message, or the empty string if everything went fine.
		/// </summary>
		public string ToFile(string filePath, string rootObjectName)
		{
			RTScene scene = new RTScene();

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
			RTScene scene = new RTScene("temp scene");

			string errMsg = "";
			try
			{
				Serialization.JSONReader reader = new Serialization.JSONReader(filePath);
				reader.Structure(scene, rootObjectName);
				foreach (Transform childObj in scene.Container)
					childObj.SetParent(null, true);
			}
			catch (Exception e)
			{
				errMsg = "Error reading scene from " + filePath +
							 ": (" + e.GetType() + ") " + e.Message;
			}
			finally
			{
				Destroy(scene.Container);
			}

			return errMsg;
		}
	}
}