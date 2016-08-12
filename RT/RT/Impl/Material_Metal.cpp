#include "../Headers/Material_Metal.h"

using namespace RT;


ADD_MATERIAL_REFLECTION_DATA_CPP(Material_Metal);


bool Material_Metal::Scatter(const Ray& rIn, const Vertex& surf, const Shape& shpe,
                             FastRand& prng, Vector3f& atten, Ray& rOut) const
{
    atten = Albedo->GetValue(rIn, prng, &shpe, &surf);

    //TODO: See if this "normalize" is necessary.
    Vector3f reflected = rIn.GetDir().Reflect(surf.Normal).Normalize();
    //Add randomness based on roughness.
    reflected += (prng.GetRandUnitVector() * (float)Roughness->GetValue(rIn, prng, &shpe, &surf));

    //If the ray is pointing into the surface, count it as absorbed.
    if (reflected.Dot(surf.Normal) > 0.0f)
    {
        rOut = Ray(surf.Pos + (surf.Normal * 0.0001f),
                   reflected);
        return true;
    }
    else
    {
        return false;
    }
}
void Material_Metal::WriteData(DataWriter& writer) const
{
    Material::WriteData(writer);

    MaterialValue::WriteValue(Albedo, writer, "Albedo");
    MaterialValue::WriteValue(Roughness, writer, "Roughness");
}
void Material_Metal::ReadData(DataReader& reader)
{
    Material::ReadData(reader);

    MaterialValue::ReadValue(Albedo, reader, "Albedo");
    MaterialValue::ReadValue(Roughness, reader, "Roughness");
}