#pragma once

#include <iostream>
#include <fstream>

#include <Camera.h>

using namespace RT;


template<typename T>
struct OptionalValue
{
public:

    bool HasValue() const { return hasValue; }

    const T& GetValue() const { assert(hasValue); return value; }
    T& GetValue() { assert(hasValue); return value; }

    //Marks this instance as having a value and returns a mutable reference to that value.
    //Note that if this instance already had a value, that value is unchanged.
    void MakeValue(const T& _value) { hasValue = true; value = _value; }
    //Marks this instance as not having a value.
    void RemoveValue() { hasValue = false; }

    OptionalValue() : hasValue(false) { }
    OptionalValue(const T& value) : hasValue(true), value(value) { }

    operator T() const { return GetValue(); }

private:

    bool hasValue;
    T value;
};


struct CmdArgs
{
public:
    OptionalValue<size_t> NThreads, NBounces, NSamples,
                            OutImgWidth, OutImgHeight;
    OptionalValue<Vector3f> CamPos, CamForward, CamUp;
    OptionalValue<float> Gamma, FovScale;
    OptionalValue<std::string> InputSceneFile, SceneFileRootName, OutputImgPath;


    CmdArgs() { }

    //Parses data from the given command-line arguments.
    //Any missing data is queried from the user through standard I/O.
    //Any errors are written to "outErrorMsg".
    CmdArgs(const char* args[], size_t nArgs, std::string& outErrorMsg)
    {
        bool isInteractive = false;

        #pragma region Read arguments

        //Skip the first argument, which is the program path.
        for (size_t i = 1; i < nArgs; ++i)
        {
            std::cout << "Parsing option \"" << args[i] << "\"\n\t";
            if (args[i] == "-interactive")
            {
                isInteractive = true;
            }
            else if (args[i] == "-cPos")
            {
                if (i > nArgs - 4)
                {
                    outErrorMsg += "\nNot enough arguments after -cPos";
                    i = nArgs;
                }
                else
                {
                    TryParse(&args[i + 1], CamPos, outErrorMsg);
                    i += 3;
                    std::cout << CamPos.GetValue().x << ", " <<
                                    CamPos.GetValue().y << ", " <<
                                    CamPos.GetValue().z << "\n";
                }
            }
            else if (args[i] == "-cForward")
            {
                if (i > nArgs - 4)
                {
                    outErrorMsg += "\nNot enough arguments after -cForward";
                    i = nArgs;
                }
                else
                {
                    TryParse(&args[i + 1], CamForward, outErrorMsg);
                    if (CamForward.HasValue())
                        CamForward.GetValue() = CamForward.GetValue().Normalize();
                    i += 3;
                    std::cout << CamForward.GetValue().x << ", " <<
                                    CamForward.GetValue().y << ", " <<
                                    CamForward.GetValue().z << "\n";
                }
            }
            else if (args[i] == "-cUp")
            {
                if (i > nArgs - 4)
                {
                    outErrorMsg += "\nNot enough arguments after -cUp";
                    i = nArgs;
                }
                else
                {
                    TryParse(&args[i = 1], CamUp, outErrorMsg);
                    if (CamUp.HasValue())
                        CamUp.GetValue() = CamUp.GetValue().Normalize();
                    i += 3;
                    std::cout << CamUp.GetValue().x << ", " <<
                                    CamUp.GetValue().y << ", " <<
                                    CamUp.GetValue().z << "\n";
                }
            }
            else if (args[i] == "-gamma")
            {
                if (i > nArgs - 2)
                {
                    outErrorMsg += "\nNot enough arguments after -gamma";
                    i = nArgs;
                }
                else
                {
                    TryParse(args[i + 1], Gamma, outErrorMsg);
                    i += 1;
                }
            }
            else if (args[i] == "-fovScale")
            {
                if (i > nArgs - 2)
                {
                    outErrorMsg += "\nNot enough arguments after -fovScale";
                    i = nArgs;
                }
                else
                {
                    TryParse(args[i + 1], FovScale, outErrorMsg);
                    i += 1;
                }
            }
            else if (args[i] == "-nSamples")
            {
                if (i > nArgs - 2)
                {
                    outErrorMsg += "\nNot enough arguments after -nSamples";
                    i = nArgs;
                }
                else
                {
                    TryParse(args[i + 1], NSamples, outErrorMsg);
                    i += 1;
                }
            }
            else if (args[i] == "-nBounces")
            {
                if (i > nArgs - 2)
                {
                    outErrorMsg += "\nNot enough arguments after -nBounces";
                    i = nArgs;
                }
                else
                {
                    TryParse(args[i + 1], NBounces, outErrorMsg);
                    i += 1;
                }
            }
            else if (args[i] == "-outputPath")
            {
                if (i > nArgs - 2)
                {
                    outErrorMsg += "\nNot enough arguments after -outputPath";
                    i = nArgs;
                }
                else
                {
                    OutputImgPath = std::string(args[i + 1]);
                    i += 1;
                }
            }
            else if (args[i] == "-outputSize")
            {
                if (i > nArgs - 3)
                {
                    outErrorMsg += "\nNot enough arguments after -outputSize";
                    i = nArgs;
                }
                else
                {
                    TryParse(args[i + 1], OutImgWidth, outErrorMsg);
                    TryParse(args[i + 2], OutImgHeight, outErrorMsg);
                    i += 2;
                }
            }
            else if (args[i] == "-scene")
            {
                if (i > nArgs - 2)
                {
                    outErrorMsg += "\nNot enough arguments after -scene";
                    i = nArgs;
                }
                else
                {
                    InputSceneFile = std::string(args[i + 1]);
                    i += 1;
                }
            }
            else if (args[i] == "-sceneRoot")
            {
                if (i > nArgs - 2)
                {
                    outErrorMsg += "\nNot enough arguments after -sceneRoot";
                    i = nArgs;
                }
                else
                {
                    SceneFileRootName = std::string(args[i + 1]);
                    i += 1;
                }
            }
            else if (args[i] == "-nThreads")
            {
                if (i > nArgs - 2)
                {
                    outErrorMsg += "\nNot enough arguments after -nThreads";
                    i = nArgs;
                }
                else
                {
                    TryParse(args[i + 1], NThreads, outErrorMsg);
                    i += 1;
                }
            }
            else
            {
                outErrorMsg += std::string() + "\nUnrecognized option \"" + args[i] + "\"";
            }
        }

        #pragma endregion

        #pragma region Ask the user for any missing options

        auto isValidUInt = [](const std::string& s) { return IsValidNumber(s.c_str(), false, false); };
        auto isValidInt = [](const std::string& s) { return IsValidNumber(s.c_str(), true, false); };
        auto isValidFloat = [](const std::string& s) { return IsValidNumber(s.c_str(), true, true); };
        auto isValidFile = [](const std::string& s) { return std::ifstream(s).is_open(); };
        auto isValidName = [](const std::string& s) { return !s.empty() && s.find('.') != std::string::npos; };
        auto alwaysValid = [](const std::string& s) { return true; };
        if (!NThreads.HasValue())
            if (isInteractive)
                TryParse(KeepTryingForValue("\nEnter the number of threads to use (Default 4): >", isValidUInt).c_str(),
                            NThreads, outErrorMsg);
            else
                NThreads = 4;
        if (!NBounces.HasValue())
            TryParse(KeepTryingForValue("\nEnter the number of bounces for each ray: >", isValidUInt).c_str(),
                        NBounces, outErrorMsg);
        if (!NSamples.HasValue())
            TryParse(KeepTryingForValue("\nEnter the number of samples per pixel: >", isValidUInt).c_str(),
                        NSamples, outErrorMsg);
        if (!OutImgWidth.HasValue())
            TryParse(KeepTryingForValue("\nEnter the width of the output image: >", isValidUInt).c_str(),
                        OutImgWidth, outErrorMsg);
        if (!OutImgHeight.HasValue())
            TryParse(KeepTryingForValue("\nEnter the height of the output image: >", isValidUInt).c_str(),
                        OutImgHeight, outErrorMsg);
        if (!CamPos.HasValue())
        {
            OptionalValue<float> x, y, z;
            TryParse(KeepTryingForValue("\nEnter the camera's X pos: >", isValidFloat).c_str(), x, outErrorMsg);
            TryParse(KeepTryingForValue("\nEnter the camera's Y pos: >", isValidFloat).c_str(), y, outErrorMsg);
            TryParse(KeepTryingForValue("\nEnter the camera's Z pos: >", isValidFloat).c_str(), z, outErrorMsg);
            CamPos.MakeValue(Vector3f(x.GetValue(), y.GetValue(), z.GetValue()));
        }
        if (!CamForward.HasValue())
        {
            OptionalValue<float> x, y, z;
            TryParse(KeepTryingForValue("\nEnter the camera's forward vector X: >", isValidFloat).c_str(), x, outErrorMsg);
            TryParse(KeepTryingForValue("\nEnter the camera's forward vector Y: >", isValidFloat).c_str(), y, outErrorMsg);
            TryParse(KeepTryingForValue("\nEnter the camera's forward vector Z: >", isValidFloat).c_str(), z, outErrorMsg);
            CamForward.MakeValue(Vector3f(x.GetValue(), y.GetValue(), z.GetValue()).Normalize());
        }
        if (!CamUp.HasValue())
        {
            OptionalValue<float> x, y, z;
            TryParse(KeepTryingForValue("\nEnter the camera's upwards vector X: >", isValidFloat).c_str(), x, outErrorMsg);
            TryParse(KeepTryingForValue("\nEnter the camera's upwards vector Y: >", isValidFloat).c_str(), y, outErrorMsg);
            TryParse(KeepTryingForValue("\nEnter the camera's upwards vector Z: >", isValidFloat).c_str(), z, outErrorMsg);
            CamUp.MakeValue(Vector3f(x.GetValue(), y.GetValue(), z.GetValue()).Normalize());
        }
        if (!Gamma.HasValue())
            if (isInteractive)
                TryParse(KeepTryingForValue("\nEnter the gamma value: >", isValidFloat).c_str(), Gamma, outErrorMsg);
            else
                Gamma = 2.2f;
        if (!FovScale.HasValue())
            if (isInteractive)
                TryParse(KeepTryingForValue("\nEnter the fov scale: >", isValidFloat).c_str(), FovScale, outErrorMsg);
            else
                FovScale = 1.0f;
        if (!InputSceneFile.HasValue())
            InputSceneFile = KeepTryingForValue("\nEnter the input scene file path: >", isValidFile);
        if (!SceneFileRootName.HasValue())
            SceneFileRootName = KeepTryingForValue("\nEnter the input scene file's root name: >", alwaysValid);
        if (!OutputImgPath.HasValue())
            OutputImgPath = KeepTryingForValue("\nEnter the output image file's path: >", isValidName);

        #pragma endregion
    }


private:

    static bool IsValidNumber(const char* str, bool allowNegative, bool allowDecimal)
    {
        int i = 0;
        while (str[i] != '\0')
        {
            if (str[i] == '.')
            {
                if (allowDecimal)
                    allowDecimal = false;
                else
                    return false;
            }
            else if (str[i] == '-')
            {
                if (!allowNegative)
                    return false;
            }
            else if (str[i] < '0' || str[i] > '9')
                return false;

            allowNegative = false;
            i += 1;
        }
        return true;
    }

    static void FailParse(const char* str, const char* type, std::string& outErrorMsg)
    {
        outErrorMsg += std::string("\nCouldn't parse \"") + str + "\" as " + type;
    }

    static void TryParse(const char* str, OptionalValue<int>& outI, std::string& outErrorMsg)
    {
        if (IsValidNumber(str, true, false))
            outI = atoi(str);
        else
        {
            FailParse(str, "int", outErrorMsg);
            return;
        }
    }
    static void TryParse(const char* str, OptionalValue<size_t>& outU, std::string& outErrorMsg)
    {
        if (IsValidNumber(str, false, false))
            outU = (size_t)atoi(str);
        else
        {
            FailParse(str, "uint", outErrorMsg);
            return;
        }
    }
    static void TryParse(const char* str, OptionalValue<float>& outF, std::string& outErrorMsg)
    {
        if (IsValidNumber(str, true, true))
            outF = (float)atof(str);
        else
        {
            FailParse(str, "float", outErrorMsg);
            return;
        }
    }
    static void TryParse(const char** xyzStrings, OptionalValue<Vector3f>& outV, std::string& outErrorMsg)
    {
        OptionalValue<float> x, y, z;
        TryParse(xyzStrings[0], x, outErrorMsg);
        TryParse(xyzStrings[1], y, outErrorMsg);
        TryParse(xyzStrings[2], z, outErrorMsg);

        if (x.HasValue() && y.HasValue() && z.HasValue())
            outV = Vector3f(x.GetValue(), y.GetValue(), z.GetValue());
        else
            outV.RemoveValue();
    }


    //Keeps printing the same message over and over until the user provides valid input.
    //Returns that input.
    static std::string KeepTryingForValue(const char* message,
                                            bool(*predicate)(const std::string& tryValue))
        {
            std::string tryVal;

            std::cout << message;
            std::cin >> tryVal;
            while (!predicate(tryVal))
            {
                std::cout << message;
                std::cin >> tryVal;
            }

            return tryVal;
        }
};