#include "../Headers/Texture2D.h"


 Vector3f Texture2D::GetColor(float u, float v) const
 {
     int x = (int)(u * widthF),
         y = (int)(v * heightF);

     x %= width;
     y %= height;

     if (x < 0)
         x = width + x;
     if (y < 0)
         y = height + y;

     return GetColor(x, y);
 }
 
void Texture2D::Resize(int newWidth, int newHeight, Vector3f col)
{
    bool different = ((width * height) != (newWidth * newHeight));

    width = newWidth;
    height = newHeight;
    widthF = (float)width;
    heightF = (float)height;

    if (different)
    {
        delete[] colors;
        colors = new Vector3f[width * height];
        for (int i = 0; i < width * height; ++i)
            colors[i] = col;
    }
}
void Texture2D::Fill(const Vector3f& col)
{
    for (int i = 0; i < width * height; ++i)
        colors[i] = col;
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