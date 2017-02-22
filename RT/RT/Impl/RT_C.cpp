#include "../Headers/RT_C.h"

using namespace RT;

#define C_RT_API_IMPL

C_RT_API_IMPL float* rt_GenerateImage(unsigned int imgWidth, unsigned int imgHeight,
                                      unsigned int samplesPerPixel,
                                      unsigned int maxBounces, unsigned int nThreads,
                                      float fovScale, float gamma,
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
    String err;
    JsonSerialization::FromJSONFile(sceneJSONPath, tracer, err);

    tracer.PrecalcData();


    //Run the trace.
    Texture2D tex(imgWidth, imgHeight);
    tracer.TraceFullImage(cam, tex, nThreads, maxBounces, fovScale, gamma, samplesPerPixel);

    //Copy out the color data.
    size_t elementsWide = imgWidth * 3;
    size_t nElements = elementsWide * imgHeight;
    float* colors = new float[nElements];
    for (size_t y = 0; y < imgHeight; ++y)
    {
        size_t indexOffset = y * elementsWide;

        for (size_t x = 0; x < imgWidth; ++x)
        {
            Vector3f col = tex.GetColor(x, y);
            size_t index = (x * 3) + indexOffset;

            colors[index] = col.x;
            colors[index + 1] = col.y;
            colors[index + 2] = col.z;
        }
    }
    return colors;
}

C_RT_API_IMPL void rt_ReleaseImage(float* img)
{
    delete[] img;
}

C_RT_API_IMPL unsigned char rt_ERRORCODE_SUCCESS() { return 0; }
C_RT_API_IMPL unsigned char rt_ERRORCODE_BAD_SIZE() { return 1; }
C_RT_API_IMPL unsigned char rt_ERRORCODE_BAD_VALUE() { return 2; }
C_RT_API_IMPL unsigned char rt_ERRORCODE_BAD_JSON() { return 3; }

unsigned char rt_GetError(unsigned int imgWidth, unsigned int imgHeight, unsigned int samplesPerPixel,
                          unsigned int maxBounces, unsigned int nThreads,
                          float fovScale, float gamma,
                          float camPosX, float camPosY, float camPosZ,
                          float camForwardX, float camForwardY, float camForwardZ,
                          float camUpX, float camUpY, float camUpZ,
                          const char* sceneJSONPath)
{
    if (nThreads == 0 || samplesPerPixel == 0 || fovScale <= 0.0f)
        return rt_ERRORCODE_BAD_VALUE();

    if (imgWidth == 0 || imgHeight < nThreads)
        return rt_ERRORCODE_BAD_SIZE();

    Tracer tr;
    String err;
    JsonSerialization::FromJSONFile(sceneJSONPath, tr, err);
    if (err.GetSize() > 0)
    {
        std::cout << "\nERROR reading JSON: " << err.CStr() << "\n\n";
        return rt_ERRORCODE_BAD_JSON();
    }

    return rt_ERRORCODE_SUCCESS();
}