#pragma once

#include <string>

#include "Main.hpp"


namespace RT
{
    class RT_API String
    {
    public:

        //Represents a bad position or size.
        static const size_t NPos;


        String() { }
        String(const char* cStr) : str(cStr) { }

        explicit String(float f) : str(std::to_string(f)) { }
        explicit String(double d) : str(std::to_string(d)) { }
        explicit String(int i) : str(std::to_string(i)) { }
        explicit String(size_t u) : str(std::to_string(u)) { }
        explicit String(bool b) : str(b ? "true" : "false") { }

        
        char& operator[](size_t i) { return str[i]; }
        char operator[](size_t i) const { return str[i]; }

        String operator+(const char* _str) const { return String(str + _str); }
        String operator+(const String& _str) const { return String(str + _str.str); }

        String& operator+=(const char* _str) { str += _str; return *this; }
        String& operator+=(const String& _str) { str += _str.str; return *this; }

        bool operator==(const String& _str) const { return str == _str.str; }
        bool operator==(const char* _str) const { return str == _str; }


        size_t GetSize() const { return str.size(); }
        const char* CStr() const { return str.c_str(); }

        void Insert(size_t i, char c) { str.insert(str.begin() + i, c); }
        void Erase(size_t i) { str.erase(str.begin() + i); }

        //Returns NPos if the given character wasn't found.
        size_t IndexOf(char c) const { return str.find(c); }
        //Returns NPos if the given string wasn't found.
        size_t IndexOf(const String& s) const { return str.find(std::string(s.CStr())); }

        String SubStr(size_t start, size_t length) const { return str.substr(start, length); }

        bool IsEmpty() const { return str.empty(); }


    private:

        std::string str;

        String(const std::string& _str) : str(_str) { }
    };
}