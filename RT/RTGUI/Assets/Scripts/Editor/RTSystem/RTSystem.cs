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
	}
}