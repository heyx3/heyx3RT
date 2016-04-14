#include "../Headers/SkyMaterial_SimpleColor.h"


Vector3f SkyMaterial_SimpleColor::GetColor(const Ray& ray) const
{
    return Color;
}