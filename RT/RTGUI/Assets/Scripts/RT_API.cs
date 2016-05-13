using System;
using System.Threading;
using System.Runtime.InteropServices;
using UnityEngine;


public class RT_API
{
	public static string ErrorMsg;


	//Generates a ray-traced image with the given arguments.
	//The resulting color will be organized left to right, then top to bottom.
	//If there was an error, "null" will be returned and "ErrorMsg" will be set.
	public static Color[] GenerateImage(uint imgWidth, uint imgHeight, uint samplesPerPixel,
										uint maxBounces, uint nThreads, float gamma,
										Vector3 camPos, Vector3 camForward, Vector3 camUp,
										string sceneJSONPath)
	{
		//Make sure there aren't any errors.
		int errCode = GetError(imgWidth, imgHeight, samplesPerPixel, maxBounces, nThreads, gamma,
							   camPos.x, camPos.y, camPos.z,
							   camForward.x, camForward.y, camForward.z,
							   camUp.x, camUp.y, camUp.z,
							   sceneJSONPath);
		if (errCode == ERRORCODE_BAD_SIZE())
		{
			ErrorMsg = "Image is too small to render";
			return null;
		}
		else if (errCode == ERRORCODE_BAD_ZERO())
		{
			ErrorMsg = "samplesPerPixel and nThreads cannot be 0!";
			return null;
		}
		else if (errCode == ERRORCODE_BAD_JSON())
		{
			ErrorMsg = "Error parsing JSON scene file";
			return null;
		}
		else if (errCode != ERRORCODE_SUCCESS())
		{
			ErrorMsg = "Unknown error code: " + errCode;
			return null;
		}

		//Trace the scene.
		IntPtr imgPtr = GenerateImage(imgWidth, imgHeight, samplesPerPixel, maxBounces, nThreads, gamma,
									  camPos.x, camPos.y, camPos.z,
									  camForward.x, camForward.y, camForward.z,
									  camUp.x, camUp.y, camUp.z,
									  sceneJSONPath);

		//Copy the data into a C# array and then free the original array.
		float[] imgArray = new float[imgWidth * imgHeight * 3];
		Marshal.Copy(imgPtr, imgArray, 0, imgArray.Length);
		ReleaseImage(imgPtr);
		
		//Convert to a color array.
		Color[] outCol = new Color[imgWidth * imgHeight];
		for (uint y = 0; y < imgHeight; ++y)
		{
			for (uint x = 0; x < imgWidth; ++x)
			{
				uint outIndex = x + (y * imgWidth),
					 inIndex = outIndex * 3;
				outCol[outIndex] = new Color(imgArray[inIndex],
											 imgArray[inIndex + 1],
											 imgArray[inIndex + 2]);
			}
		}
		return outCol;
	}


	[DllImport("RT.dll")]
	private static extern IntPtr GenerateImage(uint imgWidth, uint imgHeight, uint samplesPerPixel,
										       uint maxBounces, uint nThreads, float gamma,
										       float camPosX, float camPosY, float camPosZ,
										       float camForwardX, float camForwardY, float camForwardZ,
										       float camUpX, float camUpY, float camUpZ,
										       string sceneJSONPath);
	[DllImport("RT.dll")]
	private static extern void ReleaseImage(IntPtr img);
	

	[DllImport("RT.dll")]
	private static extern int ERRORCODE_SUCCESS();
	[DllImport("RT.dll")]
	private static extern int ERRORCODE_BAD_SIZE();
	[DllImport("RT.dll")]
	private static extern int ERRORCODE_BAD_ZERO();
	[DllImport("RT.dll")]
	private static extern int ERRORCODE_BAD_JSON();

	[DllImport("RT.dll")]
	private static extern int GetError(uint imgWidth, uint imgHeight, uint samplesPerPixel,
									   uint maxBounces, uint nThreads, float gamma,
									   float camPosX, float camPosY, float camPosZ,
									   float camForwardX, float camForwardY, float camForwardZ,
									   float camUpX, float camUpY, float camUpZ,
									   string sceneJSONPath);
}