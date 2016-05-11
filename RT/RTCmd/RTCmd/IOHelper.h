#pragma once

#include <Vectors.h>
#include <iostream>


namespace IOHelper
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
}