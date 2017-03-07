#pragma once

#include "RT.hpp"


//The below code provides a C interface for the most common use-case of RT.

#define C_RT_API extern "C" __declspec(dllexport)


//Generates a ray-traced image and returns the image data. Returns null if there was an error.
//The data is organized into RGB floats from left to right, then bottom to top.
//NOTE: The image memory is allocated on the heap and must be freed by calling "ReleaseImage()"!
//NOTE: The size of the output image array is imgWidth * imgHeight * 3. It is ordered in rows.
//The arguments are as follows:
//"imgWidth" and "imgHeight" are the size of the output image.
//"samplesPerPixel" is the number of rays used per pixel. Higher values yields better quality.
//"maxBounces" is the maximum number of bounces a ray is allowed to make. Higher values reduce error.
//"nThreads" is the number of threads to run this computation on.
//"vertFOVDegrees" is the vertical Field of View of the camera, in degrees.
//"aperture" is the diameter of the simulated camera lens.
//"focusDist" is the distance of the lens focus.
//"camPosX/Y/Z" are the position of the camera along each axis.
//"camForwardX/Y/Z" are the forward axis of the camera. This does not have to be normalized.
//"camUpX/Y/Z" are the upward axis of the camera. This does not have to be normalized.
//"sceneJSONPath" is the path to the Tracer object serialized as a JSON file.
//"rootJSONObjName" is the name of the root object in the JSON file.
C_RT_API float* rt_GenerateImage(unsigned int imgWidth, unsigned int imgHeight, unsigned int samplesPerPixel,
                                 unsigned int maxBounces, unsigned int nThreads,
                                 float vertFOVDegrees, float aperture, float focusDist,
                                 float camPosX, float camPosY, float camPosZ,
                                 float camForwardX, float camForwardY, float camForwardZ,
                                 float camUpX, float camUpY, float camUpZ,
                                 const char* sceneJSONPath);
//Frees up the data returned by "GenerateImage()".
//Failing to call this when finished with the data results in a memory leak.
C_RT_API void rt_ReleaseImage(float* img);


//The code that represents "everything was successful!"
C_RT_API unsigned char rt_ERRORCODE_SUCCESS();
//The code that represents "The texture is not large enough to be traced successfully".
//This can happen if the width is 0 or the height is less than the number of threads to use.
C_RT_API unsigned char rt_ERRORCODE_BAD_SIZE();
//'nThreads' or 'samplesPerPixel' is 0, or 'vertFOVDegrees' is non-positive.
C_RT_API unsigned char rt_ERRORCODE_BAD_VALUE();
//The code that represents "Couldn't parse the JSON file correctly".
C_RT_API unsigned char rt_ERRORCODE_BAD_JSON();

//Has the same signature as "GenerateImage()".
//Checks over the inputs for any possible errors and returns an error code.
C_RT_API unsigned char rt_GetError(unsigned int imgWidth, unsigned int imgHeight, unsigned int samplesPerPixel,
                                   unsigned int maxBounces, unsigned int nThreads,
                                   float vertFOVDegrees, float aperture, float focusDist,
                                   float camPosX, float camPosY, float camPosZ,
                                   float camForwardX, float camForwardY, float camForwardZ,
                                   float camUpX, float camUpY, float camUpZ,
                                   const char* sceneJSONPath);