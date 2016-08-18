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
									  Input_ScreenPos = "IN.screenPos",
									  Input_UV = "IN.uv",
									  Input_WorldNormal = "IN.worldNorm",
									  Input_Tangent = "IN.tangent",
									  Input_Bitangent = "IN.bitangent",
									  Input_CamPos = "_WorldSpaceCameraPos",
									  Input_RayDir = "normalize(IN.worldPos - _WorldSpaceCameraPos)";


		public Mesh Shape_Sphere, Shape_Plane,
					SkySphere;
		
		public Material Mat_Lambert, Mat_Metal;
		public Material SkyMat_SolidColor, SkyMat_VerticalGradient;

		public Texture2D PureNoiseTex;
	}
}