#include "../Headers/Texture2D.h"

#include <assert.h>
#include "../Headers/ThirdParty/bmp_io.hpp"
#include "../Headers/ThirdParty/lodepng.h"

using namespace RT;


namespace
{
    template<typename T>
    T Clamp(T min, T max, T val) { return (val < min ? min : (val > max ? max : val)); }
}


Texture2D::Texture2D(const String& filePath, String& errMsg, SupportedFileTypes fileType)
    : colors(nullptr)
{
    errMsg = Reload(filePath, fileType);
}

String Texture2D::Reload(const String& filePath, SupportedFileTypes fileType)
{
    if (fileType == UNKNOWN)
    {
        if (filePath.GetSize() < 4)
            return "File-name was too short to infer a type";

        String extension = filePath.SubStr(filePath.GetSize() - 4, 4);

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
            if (bmp_read(filePath.CStr(), &width, &height, &reds, &greens, &blues))
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
            unsigned width, height;
            unsigned errCode = lodepng::decode(bytes, width, height, filePath.CStr());
            if (errCode != 0)
            {
                return String("Error reading the PNG file: ") + lodepng_error_text(errCode);
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

                    SetColor(x, height - y - 1,
                             Vector3f((float)bytes[indexOffset2] * invMaxVal,
                                      (float)bytes[indexOffset2 + 1] * invMaxVal,
                                      (float)bytes[indexOffset2 + 2] * invMaxVal));
                }
            }
            } break;

        default:
            return String("Unexpected file type enum value: ") + String(fileType);
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

String Texture2D::SavePNG(const String& path) const
{
    std::vector<unsigned char> data;
    data.reserve(height * width * 4);

    for (size_t y = 0; y < height; ++y)
    {
        for (size_t x = 0; x < width; ++x)
        {
            Vector3f col = GetColor(x, height - y - 1);

            data.push_back((unsigned char)Clamp(0, 255, (int)(col.x * 255.0f)));
            data.push_back((unsigned char)Clamp(0, 255, (int)(col.y * 255.0f)));
            data.push_back((unsigned char)Clamp(0, 255, (int)(col.z * 255.0f)));
            data.push_back(255);
        }
    }

    unsigned errCode = lodepng::encode(path.CStr(), data, (unsigned)width, (unsigned)height);
    if (errCode != 0)
    {
        return String("LodePNG error: ") + lodepng_error_text(errCode);
    }
    
    return "";
}
String Texture2D::SaveBMP(const String& path) const
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

    if (bmp_24_write(path.CStr(), (unsigned long)width, (long)height,
                     reds.data(), greens.data(), blues.data()))
    {
        return "error writing BMP file";
    }

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