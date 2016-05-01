#pragma once

#include <vector>

#include "Main.hpp"
#include "Vector3f.h"
#include "DataSerialization.h"


//TODO: Inherit from SurfaceColor.
class RT_API Texture2D
{
public:

    Texture2D(int _width, int _height, Vector3f col = Vector3f(1.0f, 0.0f, 1.0f))
        : width(_width), height(_height), widthF((float)_width), heightF((float)_height)
    {
        colors = new Vector3f[width * height];
        for (int i = 0; i < width * height; ++i)
            colors[i] = col;
    }
    ~Texture2D() { delete[] colors; }


    int GetWidth() const { return width; }
    int GetHeight() const { return height; }

    Vector3f GetColor(int x, int y) const { return colors[x + (width * y)]; }
    Vector3f GetColor(float u, float v) const;
    
    const Vector3f* GetRawRGB() const { return colors; }
    Vector3f* GetRawRGB() { return colors; }

    void SetColor(int x, int y, const Vector3f& newCol) { colors[x + (width * y)] = newCol; }

    void Resize(int newWidth, int newHeight, Vector3f col = Vector3f(1.0f, 0.0f, 1.0f));
    void Fill(const Vector3f& col);


private:

    int width, height;
    float widthF, heightF;

    Vector3f* colors;
};