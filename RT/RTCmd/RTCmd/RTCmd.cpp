/*

RT Cmd: Generates a ray-traced image via the command-line using RT.
The generated image is a BMP or PNG file.

Options:
-nThreads 4              The number of threads to split the work across. Defaults to 4.
-cPos 1.0 1.0 1.0        The camera's position.
-cForward 1.0 1.0 1.0    The camera's forward vector. Automatically normalized by the program.
-cUp 0.0 1.0 0.0         The camera's upward vector. Automatically normalized by the program.
-gamma 2.2               The gamma value, used to gamma-correct the output file.
-nSamples 100            The number of rays/samples per pixel.
-nBounces 50             The maximum number of times each ray can bounce/scatter.
-outputPath "MyImg.bmp"  The path of the output image. Must end in either .bmp or .png.
-outputSize 800 600      The width/height of the output image.
-scene "MyScene.json"     The scene JSON file containing the Tracer scene.

Exit codes:

1: unable to parse integer or float.
2: unrecognized option.
3: output file type wasn't .bmp or .png.
4: couldn't parse scene XML file.

*/


#include "targetver.h"

#include <iostream>
#include <fstream>
#include <vector>

#include <RT.hpp>

#include "../../RT/Headers/JsonSerialization.h"


namespace
{
    void PrintLn(const char* str)
    {
        std::cout << str << "\n";
    }
    void PrintLn(const std::string& str) { PrintLn(str.c_str()); }
    void PrintLn(const char* str1, const char* str2)
    {
        std::string str = str1;
        str += str2;
        PrintLn(str);
    }
    void PrintLn(const char* str1, const char* str2, const char* str3)
    {
        std::string str = str1;
        str += str2;
        str += str3;
        PrintLn(str);
    }
    void PrintLn(const char* str1, const char* str2, const char* str3, const char* str4)
    {
        std::string str = str1;
        str += str2;
        str += str3;
        str += str4;
        PrintLn(str);
    }
    void PrintLn(const char* str1, const char* str2, const char* str3, const char* str4, const char* str5)
    {
        std::string str = str1;
        str += str2;
        str += str3;
        str += str4;
        str += str5;
        PrintLn(str);
    }
    void PrintLn(const Vector3f& v)
    {
        char str[64];
        sprintf(str, "{%f, %f, %f}", v.x, v.y, v.z);
        PrintLn(str);
    }

    void Pause()
    {
        std::cin.get();
    }


    bool FailParse(const char* str, const char* type)
    {
        std::string stdS = "ERROR: couldn't parse \"";
        stdS.append(str);
        stdS.append("\" as ");
        stdS.append(type);
        PrintLn(stdS);
        Pause();
        exit(1);
        return false;
    }
    bool TryParse(const char* str, int& outI)
    {
        int i = 0;
        while (str[i] != '\0')
        {
            bool isDigit = (str[i] >= '0' && str[i] <= '9');
            if (!isDigit && (i > 0 || str[i] != '-'))
                return FailParse(str, "int");
            i += 1;
        }

        outI = atoi(str);
        return true;
    }
    bool TryParse(const char* str, size_t& outU)
    {
        size_t i = 0;
        while (str[i] != '\0')
        {
            if (str[i] < '0' || str[i] > '9')
                return FailParse(str, "int");
            i += 1;
        }

        outU = (size_t)atoi(str);
        return true;
    }
    bool TryParse(const char* str, float& outF)
    {
        int i = 0;
        bool foundDot = false;
        while (str[i] != '\0')
        {
            if (str[i] == '.')
            {
                if (foundDot)
                    return FailParse(str, "float");
                foundDot = true;
            }
            else
            {
                bool isDigit = (str[i] >= '0' && str[i] <= '9');
                if (!isDigit && (i > 0 || str[i] != '-'))
                    return FailParse(str, "float");
            }
            i += 1;
        }

        outF = (float)atof(str);
        return true;
    }
    bool TryParse(const char** args, Vector3f& outV)
    {
        return TryParse(args[0], outV.x) &&
               TryParse(args[1], outV.y) &&
               TryParse(args[2], outV.z);
    }

    void ParseArgs(size_t nArgs, const char* args[],
                   Camera& cam, float& gamma, size_t& nSamples, size_t& nBounces,
                   std::string& outputFilePath, std::string& sceneFilePath,
                   size_t& outputFileWidth, size_t& outputFileHeight, size_t& nThreads)
    {
        //Skip the first argument, which is the program path.
        for (size_t i = 1; i < nArgs; ++i)
        {
            PrintLn("Parsing option ", args[i]);
#define IS_STR(s) std::string(s).compare(args[i]) == 0
            if (IS_STR("-cPos"))
            {
                TryParse(&args[i + 1], cam.Pos);
                i += 3;
                PrintLn(cam.Pos);
            }
            else if (IS_STR("-cForward"))
            {
                Vector3f forw;
                if (TryParse(&args[i + 1], forw))
                    cam.SetRotation(forw.Normalize(), cam.GetUpward());
                i += 3;
                PrintLn(cam.GetForward());
            }
            else if (IS_STR("-cUp"))
            {
                Vector3f up;
                if (TryParse(&args[i + 1], up))
                    cam.SetRotation(cam.GetForward(), up.Normalize());
                i += 3;
                PrintLn(cam.GetUpward());
            }
            else if (IS_STR("-gamma"))
            {
                TryParse(args[i + 1], gamma);
                i += 1;
            }
            else if (IS_STR("-nSamples"))
            {
                TryParse(args[i + 1], nSamples);
                i += 1;
            }
            else if (IS_STR("-nBounces"))
            {
                TryParse(args[i + 1], nBounces);
                i += 1;
            }
            else if (IS_STR("-outputPath"))
            {
                outputFilePath = args[i + 1];
                i += 1;
            }
            else if (IS_STR("-outputSize"))
            {
                TryParse(args[i + 1], outputFileWidth);
                i += 1;
                TryParse(args[i + 1], outputFileHeight);
                i += 1;
            }
            else if (IS_STR("-scene"))
            {
                sceneFilePath = args[i + 1];
                i += 1;
            }
            else if (IS_STR("-nThreads"))
            {
                TryParse(args[i + 1], nThreads);
                i += 1;
            }
            else
            {
                PrintLn("ERROR: Unrecognized option ", args[i]);
                Pause();
                exit(2);
            }
#undef IS_STR
        }
    }
}


int main(int argc, const char* argv[])
{
    //Read in arguments from the command line.
    Camera cam(Vector3f(-50.0f, 5.0f, -50.0f), Vector3f(50.0f, -5.0f, 50.0f).Normalize(),
               Vector3f(0.0f, 1.0f, 0.0f), 2.0f);
    float gamma = 2.2f;
    size_t nSamples = 100,
           nBounces = 25,
           nThreads = 4;
    std::string outFilePath = "MyImg.png",
                sceneFilePath = "SampleScene.json";
    size_t outFileW = 256,
           outFileH = 128;
    ParseArgs(argc, argv, cam, gamma, nSamples, nBounces, outFilePath, sceneFilePath, outFileW, outFileH, nThreads);

    //Finalize the camera.
    cam.SetRotation(cam.GetForward(), cam.GetSideways().Cross(cam.GetForward()).Normalize());
    cam.WidthOverHeight = (float)outFileW / (float)outFileH;

    //Read the scene data from the file.
    Tracer tracer;
    std::string err;
    JsonSerialization::FromJSONFile(sceneFilePath, tracer, err);
    if (err.size() > 0)
    {
        PrintLn("ERROR: ", err.c_str());
        Pause();
        exit(4);
    }


    //Run the tracer.

    Texture2D tex(outFileW, outFileH);
    tracer.PrecalcData();

    PrintLn("Rendering...");

    tracer.TraceFullImage(cam, tex, nThreads, nBounces, gamma, nSamples);


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
        PrintLn("Unrecognized output image type ", outFilePath.substr(outFilePath.size() - 3, 3).c_str());
        Pause();
        exit(3);
    }

    if (!err.empty())
    {
        PrintLn("Error saving file: ", err.c_str());
    }

    PrintLn("Done!");
    Pause();
    return 0;
}