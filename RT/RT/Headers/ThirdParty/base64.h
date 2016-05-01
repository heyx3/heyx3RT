#pragma once

#include <string>
#include <vector>


namespace base64
{
    //Converts byte data to a string for nice storage into text files.
    std::string encode(const unsigned char* bytes, size_t nBytes);

    //Converts the string version of byte data back into byte data.
    void decode(const std::string& s, std::vector<unsigned char>& outBytes);
}