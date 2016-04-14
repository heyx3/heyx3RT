#include "../Headers/Material_Lambert.h"


bool Material_Lambert::Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                               FastRand& prng, Vector3f& attenuation, Ray& rOut) const
{
    Vector3f newPos = surface.Pos + (surface.Normal * 0.001f);
    Vector3f targetPos = surface.Pos + surface.Normal + prng.GetRandUnitVector();
    rOut = Ray(newPos, (targetPos - newPos).Normalize());

    attenuation = Color;

    return true;
}