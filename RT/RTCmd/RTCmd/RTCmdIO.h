#pragma once

#include "IOHelper.h"

#include <Camera.h>


namespace RTCmdIO
{
    bool FailParse(const char* str, const char* type)
    {
        std::string stdS = "ERROR: couldn't parse \"";
        stdS.append(str);
        stdS.append("\" as ");
        stdS.append(type);
        IOHelper::PrintLn(stdS);
        IOHelper::Pause();
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

    //Returns whether the parsing was successful.
    //If it wasn't successful, an error message will have been printed to cout.
    bool ParseArgs(size_t nArgs, const char* args[],
                   Camera& cam, float& gamma, size_t& nSamples, size_t& nBounces,
                   std::string& outputFilePath, std::string& sceneFilePath,
                   size_t& outputFileWidth, size_t& outputFileHeight, size_t& nThreads)
    {
        //Skip the first argument, which is the program path.
        for (size_t i = 1; i < nArgs; ++i)
        {
            IOHelper::PrintLn("Parsing option ", args[i]);
#define IS_STR(s) std::string(s).compare(args[i]) == 0
            if (IS_STR("-cPos"))
            {
                TryParse(&args[i + 1], cam.Pos);
                i += 3;
                IOHelper::PrintLn(cam.Pos);
            }
            else if (IS_STR("-cForward"))
            {
                Vector3f forw;
                if (TryParse(&args[i + 1], forw))
                    cam.SetRotation(forw.Normalize(), cam.GetUpward());
                i += 3;
                IOHelper::PrintLn(cam.GetForward());
            }
            else if (IS_STR("-cUp"))
            {
                Vector3f up;
                if (TryParse(&args[i + 1], up))
                    cam.SetRotation(cam.GetForward(), up.Normalize());
                i += 3;
                IOHelper::PrintLn(cam.GetUpward());
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
                IOHelper::PrintLn("ERROR: Unrecognized option ", args[i]);
                IOHelper::Pause();
                return false;
            }
#undef IS_STR
        }

        return true;
    }
}