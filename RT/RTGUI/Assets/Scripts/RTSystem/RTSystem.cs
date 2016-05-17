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


		public Mesh Shape_Sphere, Shape_Plane,
					SkySphere;
		

		public Material Mat_Lambert, Mat_Metal;
		public Material SkyMat_SolidColor, SkyMat_VerticalGradient;

		public Texture2D DefaultTex, WhiteTex;
	}
}