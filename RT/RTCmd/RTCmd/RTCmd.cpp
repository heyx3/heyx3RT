/*

RT Cmd: Generates a ray-traced image via the command-line using RT.
The generated image is a BMP or PNG file.

Note that if any mandatory arguments are missing or malformed, the user will be queried for them through standard I/O.

Parameters (mandatory unless said otherwise):
-interactive             Makes all arguments mandatory, so that the user will be queried if they aren't specified.
-cPos 1.0 1.0 1.0        The camera's position.
-cForward 1.0 1.0 1.0    The camera's forward vector. Automatically normalized by the program.
-cUp 0.0 1.0 0.0         The camera's upward vector. Automatically normalized by the program.
-nSamples 100            The number of rays/samples per pixel.
-nBounces 50             The maximum number of times each ray can bounce/scatter.
-outputPath "MyImg.bmp"  The path of the output image. Must end in either .bmp or .png.
-outputSize 800 600      The width/height of the output image.
-scene "MyScene.json"    The scene JSON file containing the Tracer scene.
-sceneRoot "data"        The name of the root object in the JSON scene file.
-nThreads 4              OPTIONAL (default 4): The number of threads to split the work across.
-gamma 2.2               OPTIONAL (default 2.2): The gamma value, used to gamma-correct the output file.
-fovScale 1.0            OPTIONAL (default 1.0): Scales the FOV of the camera.

Bad or unrecognized arguments will just be ignored and the program will attempt to continue.

Exit codes:

1: output file type wasn't .bmp or .png.
2: couldn't parse scene JSON file.
3: couldn't save output image file.

*/

//TODO: Test that this works.

#include <RT.hpp>

#include "RTCmdIO.h"

using namespace RT;


int main(int argc, const char* argv[])
{
    //Aquire the arguments.
    std::string errorMsg;
    CmdArgs cmdArgs(argv, argc, errorMsg);
    if (!errorMsg.empty())
    {
        std::cout << "Error parsing command-line arguments:\n" << errorMsg <<
                     "\nIgnoring failed arguments and trying to continue...\n";
    }
    Camera cam(cmdArgs.CamPos, cmdArgs.CamForward, cmdArgs.CamUp,
               (float)cmdArgs.OutImgWidth / (float)cmdArgs.OutImgHeight);

    //Read the scene data from the file.
    Tracer tracer;
    String err;
    JsonSerialization::FromJSONFile(RT::String(cmdArgs.InputSceneFile.GetValue().c_str()),
                                    tracer,
                                    RT::String(cmdArgs.SceneFileRootName.GetValue().c_str()),
                                    err);
    if (err.GetSize() > 0)
    {
        std::cout << "Error reading " << cmdArgs.InputSceneFile.GetValue() << ": " << err.CStr() << "\n";
        char dummy;
        std::cin >> dummy;
        exit(2);
    }


    //Run the tracer.

    Texture2D tex(cmdArgs.OutImgWidth, cmdArgs.OutImgHeight);
    tracer.PrecalcData();

    std::cout << "Rendering...\n";

    tracer.TraceFullImage(cam, tex, cmdArgs.NThreads, cmdArgs.NBounces,
                          cmdArgs.FovScale, cmdArgs.Gamma, cmdArgs.NSamples);


    //Generate an image file.
    err = "";
    std::string extension = cmdArgs.OutputImgPath.GetValue().substr(cmdArgs.OutputImgPath.GetValue().size() - 3, 3);
    if (extension == "bmp")
    {
        err = tex.SaveBMP(cmdArgs.OutputImgPath.GetValue().c_str());
    }
    else if (extension == "png")
    {
        err = tex.SavePNG(cmdArgs.OutputImgPath.GetValue().c_str());
    }
    else
    {
        std::cout << "Unrecognized output image type " << extension << "\n";
        exit(1);
    }

    if (!err.IsEmpty())
    {
        std::cout << "Error saving file: " << err.CStr() << "\n";
        exit(3);
    }

    std::cout << "Done!\n\n";
    exit(0);
}

/*
int haha(int argc, const char* argv[])
{
    Tracer trc;
    trc.SkyMat = new SkyMaterial_SimpleColor(MV_Constant::Create(1.0f));
    trc.Objects.PushBack(ShapeAndMat(new Plane(Vector3f(0.0f, 0.0f, 0.0f), 900.0f),
                                     new Material_Lambert(MV_Constant::Create(0.9f, 0.65f, 0.65f))));
    trc.Objects.PushBack(ShapeAndMat(new Sphere(Vector3f(5.0f, 10.0f, 5.0f), 2.5f),
                                     new Material_Lambert(MV_Constant::Create(0.5f, 0.5f, 0.5f))));
    trc.Objects.PushBack(ShapeAndMat(new Sphere(Vector3f(4.5f, 3.0f, 4.5f), 2.75f),
                                     new Material_Metal(MV_Constant::Create(0.75f, 0.75f, 0.85f),
                                                        MV_Constant::Create(0.1f))));

    String errMsg;
    JsonSerialization::ToJSONFile("SampleScene.json", trc, false, errMsg);
    std::cout << errMsg.CStr();

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
    //haha(nArgs, args);
    return 0;
}
*/