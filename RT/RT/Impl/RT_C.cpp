#include "../Headers/RT_C.h"



#include <string>
#include <fstream>
namespace DEBUGGER
{
    const std::string path = "C:\\Users\\Billy\\Desktop\\Log.txt";

    void ClearFile()
    {
        std::ofstream(path, std::ios_base::trunc).clear();
    }
    void LogFile(const std::string& str)
    {
        std::ofstream fileOut(path, std::ios_base::app);
        if (fileOut.is_open())
            fileOut << str << "\n";
        fileOut.close();
    }
}


#define C_RT_API_IMPL

C_RT_API_IMPL float* GenerateImage(unsigned int imgWidth, unsigned int imgHeight, unsigned int samplesPerPixel,
                                   unsigned int maxBounces, unsigned int nThreads,
                                   float fovScale, float gamma,
                                   float camPosX, float camPosY, float camPosZ,
                                   float camForwardX, float camForwardY, float camForwardZ,
                                   float camUpX, float camUpY, float camUpZ,
                                   const char* sceneJSONPath, const char* rootJSONObjName)
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
    JsonSerialization::FromJSONFile(sceneJSONPath, tracer, rootJSONObjName, err);


    DEBUGGER::ClearFile();
    DEBUGGER::LogFile(std::string() + "N Objs: " + std::to_string(tracer.Objects.size()));
    for (size_t i = 0; i < tracer.Objects.size(); ++i)
    {
        DEBUGGER::LogFile(std::string() + "\t" + tracer.Objects[i].Shpe->GetTypeName() + "\t" + tracer.Objects[i].Mat->GetTypeName());
    }

    return new float[imgWidth * 3 * imgHeight];

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
        if (y >= tex.GetHeight())
        {
            DEBUGGER::LogFile(std::string() + "y " + std::to_string(y));
            break;
        }

        size_t indexOffset = y * elementsWide;

        for (size_t x = 0; x < imgWidth; ++x)
        {
            if (x >= tex.GetWidth())
            {
                DEBUGGER::LogFile(std::string() + "x " + std::to_string(x));
                break;
            }

            Vector3f col = tex.GetColor(x, y);
            size_t index = (x * 3) + indexOffset;

            if (index + 2 >= nElements)
            {
                DEBUGGER::LogFile(std::string() + "i " + std::to_string(index));
                break;
            }
            else
            {
                colors[index] = 0.0f;//col.x;
                colors[index + 1] = 0.0f;//col.y;
                colors[index + 2] = 0.0f;//col.z;
            }
        }
    }
    return colors;
}

C_RT_API_IMPL void ReleaseImage(float* img)
{
    delete[] img;
}

C_RT_API_IMPL unsigned char ERRORCODE_SUCCESS() { return 0; }
C_RT_API_IMPL unsigned char ERRORCODE_BAD_SIZE() { return 1; }
C_RT_API_IMPL unsigned char ERRORCODE_BAD_VALUE() { return 2; }
C_RT_API_IMPL unsigned char ERRORCODE_BAD_JSON() { return 3; }

unsigned char GetError(unsigned int imgWidth, unsigned int imgHeight, unsigned int samplesPerPixel,
                       unsigned int maxBounces, unsigned int nThreads,
                       float fovScale, float gamma,
                       float camPosX, float camPosY, float camPosZ,
                       float camForwardX, float camForwardY, float camForwardZ,
                       float camUpX, float camUpY, float camUpZ,
                       const char* sceneJSONPath, const char* rootJSONObjName)
{
    if (nThreads == 0 || samplesPerPixel == 0 || fovScale <= 0.0f)
        return ERRORCODE_BAD_VALUE();

    if (imgWidth == 0 || imgHeight < nThreads)
        return ERRORCODE_BAD_SIZE();

    Tracer tr;
    std::string err;
    JsonSerialization::FromJSONFile(sceneJSONPath, tr, rootJSONObjName, err);
    if (err.size() > 0)
    {
        std::cout << "\nERROR reading JSON: " << err << "\n\n";
        return ERRORCODE_BAD_JSON();
    }

    return ERRORCODE_SUCCESS();
}