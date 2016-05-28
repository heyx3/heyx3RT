/*

RT Cmd: Generates a ray-traced image via the command-line using RT.
The generated image is a BMP or PNG file.

Options:
-nThreads 4              The number of threads to split the work across. Defaults to 4.
-cPos 1.0 1.0 1.0        The camera's position.
-cForward 1.0 1.0 1.0    The camera's forward vector. Automatically normalized by the program.
-cUp 0.0 1.0 0.0         The camera's upward vector. Automatically normalized by the program.
-gamma 2.2               The gamma value, used to gamma-correct the output file.
-fovScale 1.0            Scales the FOV of the camera.
-nSamples 100            The number of rays/samples per pixel.
-nBounces 50             The maximum number of times each ray can bounce/scatter.
-outputPath "MyImg.bmp"  The path of the output image. Must end in either .bmp or .png.
-outputSize 800 600      The width/height of the output image.
-scene "MyScene.json"    The scene JSON file containing the Tracer scene.
-sceneRoot "data"        The name of the root object in the JSON scene file.

Exit codes:

1: unable to parse integer or float.
2: unrecognized option.
3: output file type wasn't .bmp or .png.
4: couldn't parse scene JSON file.

*/


#include <RT.hpp>

#include "RTCmdIO.h"


int main(int argc, const char* argv[])
{
    //Set up default values for arguments.
    Camera cam(Vector3f(-50.0f, 5.0f, -50.0f), Vector3f(50.0f, -5.0f, 50.0f).Normalize(),
               Vector3f(0.0f, 1.0f, 0.0f), 2.0f);
    float gamma = 2.2f,
          fovScale = 1.0f;
    size_t nSamples = 100,
           nBounces = 25,
           nThreads = 4;
    std::string outFilePath = "MyImg.png",
                sceneFilePath = "SampleScene.json",
                jsonRootName = "data";
    size_t outFileW = 256,
           outFileH = 128;

    //Read in arguments from the command line.
    if (!RTCmdIO::ParseArgs(argc, argv,
                            cam, gamma, fovScale, nSamples, nBounces,
                            outFilePath, sceneFilePath, jsonRootName,
                            outFileW, outFileH, nThreads))
    {
        exit(2);
    }

    //Finalize the camera.
    cam.SetRotation(cam.GetForward(), cam.GetSideways().Cross(cam.GetForward()).Normalize());
    cam.WidthOverHeight = (float)outFileW / (float)outFileH;

    //Read the scene data from the file.
    Tracer tracer;
    std::string err;
    JsonSerialization::FromJSONFile(sceneFilePath, tracer, jsonRootName, err);
    if (err.size() > 0)
    {
        IOHelper::PrintLn("ERROR: ", err.c_str());
        IOHelper::Pause();
        exit(4);
    }


    //Run the tracer.

    Texture2D tex(outFileW, outFileH);
    tracer.PrecalcData();

    IOHelper::PrintLn("Rendering...");

    tracer.TraceFullImage(cam, tex, nThreads, nBounces, fovScale, gamma, nSamples);


    //Generate an image file.
    err = "";
    if (outFilePath.substr(outFilePath.size() - 3, 3).compare("bmp") == 0)
    {
        err = tex.SaveBMP(outFilePath);
    }
    else if (outFilePath.substr(outFilePath.size() - 3, 3).compare("png") == 0)
    {
        err = tex.SavePNG(outFilePath);
    }
    else
    {
        IOHelper::PrintLn("Unrecognized output image type ", outFilePath.substr(outFilePath.size() - 3, 3).c_str());
        IOHelper::Pause();
        exit(3);
    }

    if (!err.empty())
    {
        IOHelper::PrintLn("Error saving file: ", err.c_str());
    }

    IOHelper::PrintLn("Done!");
    IOHelper::Pause();
    return 0;
}
int haha(int argc, const char* argv[])
{
    Tracer trc;
    trc.SkyMat = new SkyMaterial_SimpleColor(MV_Constant::Create(1.0f));
    trc.Objects.push_back(ShapeAndMat(new Plane(Vector3f(0.0f, 0.0f, 0.0f), 900.0f),
                                      new Material_Lambert(MV_Constant::Create(0.9f, 0.65f, 0.65f))));
    trc.Objects.push_back(ShapeAndMat(new Sphere(Vector3f(5.0f, 10.0f, 5.0f), 2.5f),
                                      new Material_Lambert(MV_Constant::Create(0.5f, 0.5f, 0.5f))));
    trc.Objects.push_back(ShapeAndMat(new Sphere(Vector3f(4.5f, 3.0f, 4.5f), 2.75f),
                                      new Material_Metal(MV_Constant::Create(0.75f, 0.75f, 0.85f),
                                                         MV_Constant::Create(0.1f))));

    std::string errMsg;
    JsonSerialization::ToJSONFile("SampleScene.json", trc, false, errMsg);
    std::cout << errMsg;

    const int nArgs = 32;
    const char* args[nArgs] = {
        "",
        "-nThreads", "4",
        "-cPos", "0.0", "1.0", "0.0",
        "-cForward", "1.0", "0.75", "1.0",
        "-cUp", "0.0", "1.0", "0.0",
        "-gamma", "2.2",
        "-fovScale", "2.3",
        "-nSamples", "100",
        "-nBounces", "50",
        "-outputPath", "MyImg.bmp",
        "-outputSize", "400", "200",
        "-scene", "SampleScene.json",
        "-sceneRoot", "data",
    };
    haha(nArgs, args);
    return 0;
}