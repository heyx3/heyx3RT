using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;


namespace RT
{
	/// <summary>
	/// The C API for RT.
	/// Allows Unity C# code to interface with RT.dll.
	/// </summary>
	public static class C_API
	{
		/// <summary>
		/// Returns an error message, or an empty string if everything went fine.
		/// </summary>
		public static string GenerateImage(Texture2D outTex, uint samplesPerPixel,
							  			   uint maxBounces, uint nThreads,
										   float fovScale, float gamma,
										   Vector3 camPos, Vector3 camForward, Vector3 camUp,
										   string sceneJSONPath)
		{
			uint imgWidth = (uint)outTex.width,
				 imgHeight = (uint)outTex.height;

			//Error-checking.
			byte err = rt_GetError(imgWidth, imgHeight, samplesPerPixel, maxBounces, nThreads,
								   fovScale, gamma, camPos.x, camPos.y, camPos.z,
								   camForward.x, camForward.y, camForward.z,
								   camUp.x, camUp.y, camUp.z, sceneJSONPath);
			if (err == rt_ERRORCODE_BAD_JSON())
				return "Badly-forced JSON in " + sceneJSONPath;
			else if (err == rt_ERRORCODE_BAD_SIZE())
				return "Image size is too small to render";
			else if (err == rt_ERRORCODE_BAD_VALUE())
				return "Make sure samplesPerPixel and nThreads are greater than 0, and fovScale is positive";
			else if (err != rt_ERRORCODE_SUCCESS())
				return "Unknown error " + err;

			//Do the ray-tracing and copy the resulting texture data into a managed .NET array.
			IntPtr arrayPtr = rt_GenerateImage(imgWidth, imgHeight, samplesPerPixel,
											   maxBounces, nThreads, fovScale, gamma,
											   camPos.x, camPos.y, camPos.z,
											   camForward.x, camForward.y, camForward.z,
											   camUp.x, camUp.y, camUp.z,
											   sceneJSONPath);
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

			//Output the color array into the texture.
			outTex.SetPixels(cols);

			return "";
		}

		
		[DllImport("RT")]
		private static extern byte rt_GetError(uint imgWidth, uint imgHeight, uint samplesPerPixel,
											   uint maxBounces, uint nThreads,
											   float fovScale, float gamma,
											   float camPosX, float camPosY, float camPosZ,
											   float camForwardX, float camForwardY, float camForwardZ,
											   float camUpX, float camUpY, float camUpZ,
											   string sceneJSONPath);
		
		[DllImport("RT")]
		private static extern byte rt_ERRORCODE_SUCCESS();
		[DllImport("RT")]
		private static extern byte rt_ERRORCODE_BAD_SIZE();
		[DllImport("RT")]
		private static extern byte rt_ERRORCODE_BAD_VALUE();
		[DllImport("RT")]
		private static extern byte rt_ERRORCODE_BAD_JSON();

		[DllImport("RT")]
		private static extern IntPtr rt_GenerateImage(uint imgWidth, uint imgHeight, uint samplesPerPixel,
													  uint maxBounces, uint nThreads,
													  float fovScale, float gamma,
													  float camPosX, float camPosY, float camPosZ,
													  float camForwardX, float camForwardY, float camForwardZ,
													  float camUpX, float camUpY, float camUpZ,
													  string sceneJSONPath);
		[DllImport("RT")]
		private static extern void rt_ReleaseImage(IntPtr img);
	}
}