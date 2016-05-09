#include "../Headers/Texture2D.h"

#include <assert.h>
#include "../Headers/ThirdParty/bmp_io.hpp"
#include "../Headers/ThirdParty/lodepng.h"


namespace
{
    template<typename T>
    T Clamp(T min, T max, T val) { return (val < min ? min : (val > max ? max : val)); }
}


Texture2D::Texture2D(const std::string& filePath, std::string& errMsg, SupportedFileTypes fileType)
    : colors(nullptr)
{
    errMsg = Reload(filePath, fileType);
}

std::string Texture2D::Reload(const std::string& filePath, SupportedFileTypes fileType)
{
    if (fileType == UNKNOWN)
    {
        if (filePath.size() < 4)
            return "File-name was too short to infer a type";

        std::string extension = filePath.substr(filePath.size() - 4, 4);

        if (extension == ".png")
            fileType = PNG;
        else if (extension == ".bmp")
            fileType = BMP;
        else
            return "Couldn't infer type from file name";
    }

    switch (fileType)
    {
        case BMP: {

            unsigned long width;
            long height;
            unsigned char *reds,
                          *greens,
                          *blues;
            if (bmp_read(filePath.c_str(), &width, &height, &reds, &greens, &blues))
            {
                return "Error reading the BMP file";
            }

            Resize(width, height);

            //Convert to texture data.
            float invMaxVal = 1.0f / (float)std::numeric_limits<unsigned char>::max();
            for (long y = 0; y < height; ++y)
            {
                for (unsigned long x = 0; x < width; ++x)
                {
                    long i = x + (y * width);
                    Vector3f col((float)reds[i] * invMaxVal,
                                 (float)greens[i] * invMaxVal,
                                 (float)blues[i] * invMaxVal);
                    SetColor(x, y, col);
                }
            }
            } break;

        case PNG: {

            std::vector<unsigned char> bytes;
            unsigned int width, height;
            unsigned int errCode = lodepng::decode(bytes, width, height, filePath);
            if (errCode != 0)
            {
                return std::string("Error reading the PNG file: ") + lodepng_error_text(errCode);
            }

            Resize(width, height);

            //Convert to texture data.
            float invMaxVal = 1.0f / (float)std::numeric_limits<unsigned char>::max();
            for (size_t y = 0; y < height; ++y)
            {
                size_t indexOffset = y * width * 4;

                for (size_t x = 0; x < width; ++x)
                {
                    size_t indexOffset2 = indexOffset + (x * 4);

                    SetColor(x, y, Vector3f((float)bytes[indexOffset2] * invMaxVal,
                                            (float)bytes[indexOffset2 + 1] * invMaxVal,
                                            (float)bytes[indexOffset2 + 2] * invMaxVal));
                }
            }
            } break;

        default:
            return std::string("Unexpected file type enum value: ") + std::to_string(fileType);
    }

    return "";
}

 Vector3f Texture2D::GetColor(float u, float v) const
 {
     int x = (int)(u * widthF),
         y = (int)(v * heightF);

     x %= width;
     y %= height;

     if (x < 0)
         x = width + x;
     assert(x >= 0);

     if (y < 0)
         y = height + y;
     assert(y >= 0);

     return GetColor((size_t)x, (size_t)y);
 }
 
void Texture2D::Resize(size_t newWidth, size_t newHeight, Vector3f col)
{
    bool different = ((width * height) != (newWidth * newHeight));

    width = newWidth;
    height = newHeight;
    widthF = (float)width;
    heightF = (float)height;

    if (different)
    {
        if (colors != nullptr)
        {
            delete[] colors;
        }

        colors = new Vector3f[width * height];
        for (size_t i = 0; i < width * height; ++i)
            colors[i] = col;
    }
}
void Texture2D::Fill(const Vector3f& col)
{
    for (size_t i = 0; i < width * height; ++i)
        colors[i] = col;
}

std::string Texture2D::SavePNG(const std::string& path) const
{
    std::vector<unsigned char> data;
    data.reserve(height * width * 4);

    for (size_t y = 0; y < height; ++y)
    {
        size_t indexOffset = y * width * 4;

        for (size_t x = 0; x < width; ++x)
        {
            Vector3f col = GetColor(x, y);

            size_t indexOffset2 = indexOffset + (x * 4);
            data[indexOffset2] = (unsigned char)Clamp(0, 255, (int)(col.x * 255.0f));
            data[indexOffset2 + 1] = (unsigned char)Clamp(0, 255, (int)(col.y * 255.0f));
            data[indexOffset2 + 2] = (unsigned char)Clamp(0, 255, (int)(col.z * 255.0f));
            data[indexOffset2 + 3] = 255;
        }
    }

    unsigned int errCode = lodepng::encode(path, data, width, height);
    if (errCode != 0)
    {
        return std::string("LodePNG error: ") + lodepng_error_text(errCode);
    }
    
    return "";
}
std::string Texture2D::SaveBMP(const std::string& path) const
{
    std::vector<unsigned char> reds, greens, blues;
    reds.resize(width * height);
    greens.resize(width * height);
    blues.resize(width * height);

    for (size_t y = 0; y < height; ++y)
    {
        for (size_t x = 0; x < width; ++x)
        {
            Vector3f col = GetColor(x, y);
            size_t index = x + (y * width);
            reds[index] = (unsigned char)Clamp(0, 255, (int)(col.x * 255.0f));
            greens[index] = (unsigned char)Clamp(0, 255, (int)(col.y * 255.0f));
            blues[index] = (unsigned char)Clamp(0, 255, (int)(col.z * 255.0f));
        }
    }

    if (bmp_24_write(path.c_str(), width, height, reds.data(), greens.data(), blues.data()))
        return "error writing BMP file";

    return "";
}

Texture2D& Texture2D::operator=(Texture2D&& moveFrom)
{
    width = moveFrom.width;
    height = moveFrom.height;
    widthF = (float)width;
    heightF = (float)height;
    colors = moveFrom.colors;

    moveFrom.colors = nullptr;

    return *this;
}