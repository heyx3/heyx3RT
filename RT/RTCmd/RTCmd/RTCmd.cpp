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
-nThreads 4              OPTIONAL (default 4): The number of threads to split the work across.
-fov 60.0                OPTIONAL (default 60.0): The vertical Field of View, in degrees.
-aperture 0.0            OPTIONAL (default 0.0): The aperture of the camera lens.
-focusDist 0.0           OPTIONAL (default 0.0): The focus distance of the camera.

Bad or unrecognized arguments will just be ignored and the program will attempt to continue.

Exit codes:

1: output file type wasn't .bmp or .png.
2: couldn't parse scene JSON file.
3: couldn't save output image file.

*/

#include <RT.hpp>

#include "RTCmdIO.h"

using namespace RT;

#include <iostream>



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
    //TODO: This can't parse json unless it's line-broken properly?
    JsonSerialization::FromJSONFile(RT::String(cmdArgs.InputSceneFile.GetValue().c_str()),
                                    tracer,
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
                          cmdArgs.VertFOVDegrees, cmdArgs.Aperture, cmdArgs.FocusDist,
                          cmdArgs.NSamples);


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
void hehe()
{
    Tracer scene;

    SharedPtr<Shape> sphere = new Sphere(Vector3f(5.0f, 0.0f, 0.0f), 2.5f);
    SharedPtr<Material> mat = new Material_Lambert;
    SharedPtr<SkyMaterial> sky = new SkyMaterial_VerticalGradient;

    scene.Objects.PushBack(ShapeAndMat(sphere, mat));
    scene.SkyMat = sky;

    //Write the scene.
    const String path = "D:/Other Games/temp/scene.json";
    String err;
    JsonSerialization::ToJSONFile(path, scene, false, err);
    if (!err.IsEmpty())
    {
        std::cout << "Error writing file: " << err.CStr() << "\n";
        char dummy;
        std::cin >> dummy;
        return;
    }

    //Read the scene back.
    JsonSerialization::FromJSONFile(path, scene, err);
    if (!err.IsEmpty())
    {
        std::cout << "Error reading file: " << err.CStr() << "\n";
        char dummy;
        std::cin >> dummy;
        return;
    }

    //Trace the scene.
    Texture2D tex(128, 128);
    scene.TraceFullImage(Camera(Vector3f(), Vector3f(1.0f, 0.0f, 0.0f), Vector3f(0.0f, 1.0f, 0.0f), 1.0f),
                         tex, 8, 50, 1.0f, 2.0f, 1000);

    //Write the image to a file.
    err = tex.SavePNG("D:/Other Games/temp/img.png");
    if (!err.IsEmpty())
    {
        std::cout << "Error saving PNG: " << err.CStr() << "\n";
        char dummy;
        std::cin >> dummy;
        return;
    }
}

int main(int argc, const char* argv[])
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
        "-fovScale", "2.3",
        "-nSamples", "100",
        "-nBounces", "50",
        "-outputPath", "MyImg.bmp",
        "-outputSize", "400", "200",
        "-scene", "SampleScene.json",
    };
    haha(nArgs, args);
    return 0;
}
*/