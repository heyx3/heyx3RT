#pragma once

#include "RT.hpp"


//The below code provides a C interface for the most common use-case of RT.

#define C_RT_API extern "C" __declspec(dllexport)


//Generates a ray-traced image and returns the image data. Returns null if there was an error.
//The data is organized into RGB floats from left to right, then bottom to top.
//NOTE: The image memory is allocated on the heap and must be freed by calling "ReleaseImage()"!
//The arguments are as follows:
//"imgWidth" and "imgHeight" are the size of the output image.
//"samplesPerPixel" is the number of rays used per pixel. Higher values yields better quality.
//"maxBounces" is the maximum number of bounces a ray is allowed to make. Higher values reduce error.
//"nThreads" is the number of threads to run this computation on.
//"gamma" is the gamma-correction value (a common value is 2.2).
//"camPosX/Y/Z" are the position of the camera along each axis.
//"camForwardX/Y/Z" are the forward axis of the camera. This does not have to be normalized.
//"camUpX/Y/Z" are the upward axis of the camera. This does not have to be normalized.
//sceneJSONPath" is the path to the Tracer object serialized as a JSON file.
C_RT_API float* GenerateImage(size_t imgWidth, size_t imgHeight, size_t samplesPerPixel,
                              size_t maxBounces, size_t nThreads, float gamma,
                              float camPosX, float camPosY, float camPosZ,
                              float camForwardX, float camForwardY, float camForwardZ,
                              float camUpX, float camUpY, float camUpZ,
                              const char* sceneJSONPath)
{
    //Set up the camera.
    Vector3f camForward = Vector3f(camForwardX, camForwardY, camForwardZ).Normalize(),
                camUp = Vector3f(camUpX, camUpY, camUpZ).Normalize();
    Camera cam(Vector3f(camPosX, camPosY, camPosZ),
                camForward, camForward.Cross(camUp).Cross(camForward).Normalize(),
                (float)imgWidth / (float)imgHeight, false);

    //Load the scene.
    Tracer tracer;
    std::string err;
    JsonSerialization::FromJSONFile(sceneJSONPath, tracer, err);
    if (err.size() > 0)
    {
        return nullptr;
    }
    tracer.PrecalcData();

    //Run the trace.
    Texture2D tex(imgWidth, imgHeight);
    tracer.TraceFullImage(cam, tex, nThreads, maxBounces, gamma, samplesPerPixel);

    //Copy out the color data.
    size_t elementsWide = imgWidth * 3;
    float* colors = new float[elementsWide * imgHeight];
    for (size_t y = 0; y < imgHeight; ++y)
    {
        size_t indexOffset = y * elementsWide;

        for (size_t x = 0; x < elementsWide; x += 3)
        {
            Vector3f col = tex.GetColor(x, y);
            size_t index = x + indexOffset;

            colors[index] = col.x;
            colors[index + 1] = col.y;
            colors[index + 2] = col.z;
        }
    }
    return colors;
}
//Frees up the data returned by "GenerateImage()".
//Failing to call this when finished with the data results in a memory leak.
C_RT_API void ReleaseImage(float* img)
{
    delete[] img;
}


//The code that represents "everything was successful!"
C_RT_API int ERRORCODE_SUCCESS() { return 0; }
//The code that represents "The texture is not large enough to be traced successfully".
//This can happen if the width is 0 or the height is less than the number of threads to use.
C_RT_API int ERRORCODE_BAD_SIZE() { return 1; }
//The code that represents "'nThreads' or 'samplesPerPixel' is 0".
C_RT_API int ERRORCODE_BAD_ZERO() { return 2; }
//The code that represents "Couldn't parse the JSON file correctly".
C_RT_API int ERRORCODE_BAD_JSON() { return 3; }

//Has the same signature as "GenerateImage()".
//Checks over the inputs for any possible errors and returns an error code.
C_RT_API int GetError(size_t imgWidth, size_t imgHeight, size_t samplesPerPixel,
                      size_t maxBounces, size_t nThreads, float gamma,
                      float camPosX, float camPosY, float camPosZ,
                      float camForwardX, float camForwardY, float camForwardZ,
                      float camUpX, float camUpY, float camUpZ,
                      const char* sceneJSONPath)
{
    if (nThreads == 0 || samplesPerPixel == 0)
        return ERRORCODE_BAD_ZERO();

    if (imgWidth == 0 || imgHeight < nThreads)
        return ERRORCODE_BAD_SIZE();

    Tracer tr;
    std::string err;
    JsonSerialization::FromJSONFile(sceneJSONPath, tr, err);
    if (err.size() > 0)
    {
        std::cout << "\nERROR reading JSON: " << err << "\n\n";
        return ERRORCODE_BAD_JSON();
    }

    return ERRORCODE_SUCCESS();
}