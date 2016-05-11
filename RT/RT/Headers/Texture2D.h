#pragma once

#include <vector>

#include "Main.hpp"
#include "DataSerialization.h"
#include "Vectors.h"


class RT_API Texture2D
{
public:

    enum SupportedFileTypes
    {
        BMP,
        PNG,
        UNKNOWN,
    };


    //Loads an image from the given file of the given type.
    //If the given file type is "UNKNOWN", it will be inferred based on the file's extension.
    //Outputs an error message if the given file wasn't loaded successfully.
    Texture2D(const std::string& filePath, std::string& outErrorMsg,
              SupportedFileTypes fileType = UNKNOWN);

    Texture2D(size_t _width, size_t _height, Vector3f col = Vector3f(1.0f, 0.0f, 1.0f))
        : width(_width), height(_height), widthF((float)_width), heightF((float)_height)
    {
        colors = new Vector3f[width * height];
        for (size_t i = 0; i < width * height; ++i)
            colors[i] = col;
    }
    ~Texture2D() { if (colors != nullptr) delete[] colors; }

    Texture2D(Texture2D&& moveFrom) { *this = std::move(moveFrom); }
    Texture2D& operator=(Texture2D&& moveFrom);


    size_t GetWidth() const { return width; }
    size_t GetHeight() const { return height; }

    Vector3f GetColor(size_t x, size_t y) const { return colors[x + (width * y)]; }
    Vector3f GetColor(Vector2f uv) const { return GetColor(uv.x, uv.y); }
    Vector3f GetColor(float u, float v) const;
    
    const Vector3f* GetRawRGB() const { return colors; }
    Vector3f* GetRawRGB() { return colors; }

    void SetColor(size_t x, size_t y, const Vector3f& newCol) { colors[x + (width * y)] = newCol; }

    //Loads an image from the given file of the given type.
    //If the given file type is "UNKNOWN", it will be inferred based on the file's extension.
    //Outputs an error message if the given file wasn't loaded successfully.
    std::string Reload(const std::string& filePath, SupportedFileTypes fileType = UNKNOWN);

    //Saves this to the given PNG file, overwriting if it already exists.
    //Returns an error message, or the empty string if everything went fine.
    std::string SavePNG(const std::string& filePath) const;
    //Saves this to the given BMP file, overwriting if it already exists.
    //Returns an error message, or the empty string if everything went fine.
    std::string SaveBMP(const std::string& filePath) const;

    void Resize(size_t newWidth, size_t newHeight, Vector3f col = Vector3f(1.0f, 0.0f, 1.0f));
    void Fill(const Vector3f& col);


private:

    size_t width, height;
    float widthF, heightF;

    Vector3f* colors;


    Texture2D(const Texture2D& cpy) = delete;
    Texture2D& operator=(const Texture2D& cpy) = delete;
};