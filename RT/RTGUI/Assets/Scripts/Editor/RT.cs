using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;


namespace RT
{
	public static class RT
	{
		[MenuItem("CONTEXT/RTMaterial/Change to Lambert")]
		public static void ChangeToLambert(MenuCommand cmd)
		{
			RTMaterial oldMat = (RTMaterial)cmd.context;

			GameObject go = oldMat.gameObject;
			GameObject.DestroyImmediate(oldMat);

			RTMaterial_Lambert lmbt = go.AddComponent<RTMaterial_Lambert>();
		}
		[MenuItem("CONTEXT/RTMaterial/Change to Lambert", true)]
		private static bool ChangeToLambertValidation(MenuCommand cmd)
		{
			return !(cmd.context is RTMaterial_Lambert);
		}

		[MenuItem("CONTEXT/RTMaterial/Change to Metal")]
		public static void ChangeToMetal(MenuCommand cmd)
		{
			RTMaterial oldMat = (RTMaterial)cmd.context;

			GameObject go = oldMat.gameObject;
			GameObject.DestroyImmediate(oldMat);

			RTMaterial_Metal mtl = go.AddComponent<RTMaterial_Metal>();
		}
		[MenuItem("CONTEXT/RTMaterial/Change to Metal", true)]
		private static bool ChangeToMetalValidation(MenuCommand cmd)
		{
			return !(cmd.context is RTMaterial_Metal);
		}


		[MenuItem("CONTEXT/RTSkyMaterial/Change to Simple Color")]
		public static void ChangeToSimpleColor(MenuCommand cmd)
		{
			RTSkyMaterial oldMat = (RTSkyMaterial)cmd.context;

			GameObject go = oldMat.gameObject;
			GameObject.DestroyImmediate(oldMat);

			RTSkyMaterial_SimpleColor sc = go.AddComponent<RTSkyMaterial_SimpleColor>();
		}
		[MenuItem("CONTEXT/RTSkyMaterial/Change to Simple Color", true)]
		public static bool ChangeToSimpleColorValidation(MenuCommand cmd)
		{
			return !(cmd.context is RTSkyMaterial_SimpleColor);
		}

		[MenuItem("CONTEXT/RTSkyMaterial/Change to Vertical Gradient")]
		public static void ChangeToVertGrad(MenuCommand cmd)
		{
			RTSkyMaterial oldMat = (RTSkyMaterial)cmd.context;

			GameObject go = oldMat.gameObject;
			GameObject.DestroyImmediate(oldMat);

			RTSkyMaterial_VerticalGradient vg = go.AddComponent<RTSkyMaterial_VerticalGradient>();
		}
		[MenuItem("CONTEXT/RTSkyMaterial/Change to Simple Color", true)]
		public static bool ChangeToVertGradValidation(MenuCommand cmd)
		{
			return !(cmd.context is RTSkyMaterial_VerticalGradient);
		}


		[DllImport("RT")]
		public static extern byte rt_ERRORCODE_SUCCESS();
		[DllImport("RT")]
		public static extern byte rt_ERRORCODE_BAD_SIZE();
		[DllImport("RT")]
		public static extern byte rt_ERRORCODE_BAD_VALUE();
		[DllImport("RT")]
		public static extern byte rt_ERRORCODE_BAD_JSON();

		[DllImport("RT")]
		public static extern byte rt_GetError(uint imgWidth, uint imgHeight, uint samplesPerPixel,
											  uint maxBounces, uint nThreads,
											  float fovScale, float gamma,
											  float camPosX, float camPosY, float camPosZ,
											  float camForwardX, float camForwardY, float camForwardZ,
											  float camUpX, float camUpY, float camUpZ,
											  string sceneJSONPath, string rootJSONObjName);

		public static void rt_GenerateImage(uint imgWidth, uint imgHeight, uint samplesPerPixel,
											uint maxBounces, uint nThreads,
											float fovScale, float gamma,
											Vector3 camPos, Vector3 camForward, Vector3 camUp,
											string sceneJSONPath, string rootJSONObjName,
											Texture2D outTex)
		{
			//Do the ray-tracing.
			IntPtr arrayPtr = rt_GenerateImage(imgWidth, imgHeight, samplesPerPixel,
											   maxBounces, nThreads, fovScale, gamma,
											   camPos.x, camPos.y, camPos.z,
											   camForward.x, camForward.y, camForward.z,
											   camUp.x, camUp.y, camUp.z,
											   sceneJSONPath, rootJSONObjName);
			float[] floatArr = new float[imgWidth * imgHeight * 3];
			Marshal.Copy(arrayPtr, floatArr, 0, floatArr.Length);
			rt_ReleaseImage(arrayPtr);

			//Convert the data to a color array.
			Color[] cols = new Color[imgWidth * imgHeight];
			for (uint y = 0; y < imgHeight; ++y)
				for (uint x = 0; x < imgWidth; ++x)
				{
					uint i = (x * 3) + (y * imgWidth * 3);
					cols[x + (y * imgWidth)] = new Color(floatArr[i], floatArr[i + 1], floatArr[i + 2], 1.0f);
				}

			//Output the color array into a texture.
			outTex.Resize((int)imgWidth, (int)imgHeight);
			outTex.SetPixels(cols);
		}
		[DllImport("RT")]
		private static extern IntPtr rt_GenerateImage(uint imgWidth, uint imgHeight, uint samplesPerPixel,
													  uint maxBounces, uint nThreads,
													  float fovScale, float gamma,
													  float camPosX, float camPosY, float camPosZ,
													  float camForwardX, float camForwardY, float camForwardZ,
													  float camUpX, float camUpY, float camUpZ,
													  string sceneJSONPath, string rootJSONObjName);
		[DllImport("RT")]
		private static extern void rt_ReleaseImage(IntPtr img);
	}
}