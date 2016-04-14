#include "../Headers/SkyMaterial_VerticalGradient.h"

#include "../Headers/Mathf.h"


Vector3f SkyMaterial_VerticalGradient::GetColor(const Ray& ray) const
{
    return Vector3f::Lerp(BottomCol, TopCol, 0.5f + (0.5f * ray.GetDir().y));
}