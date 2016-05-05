#include "../Headers/Material_Metal.h"


ADD_MATERIAL_REFLECTION_DATA_CPP(Material_Metal);


bool Material_Metal::Scatter(const Ray& rIn, const Vertex& surf, const Shape& shpe,
                             FastRand& prng, Vector3f& atten, Ray& rOut) const
{
    atten = Albedo;

    //TODO: See if this "normalize" is necessary.
    Vector3f reflected = rIn.GetDir().Reflect(surf.Normal).Normalize();
    //Add randomness based on roughness.
    reflected += (prng.GetRandUnitVector() * Roughness);

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

    writer.WriteVec3f(Albedo, "Albedo");
    writer.WriteFloat(Roughness, "Roughness");
}
void Material_Metal::ReadData(DataReader& reader)
{
    Material::ReadData(reader);

    reader.ReadVec3f(Albedo, "Albedo");
    reader.ReadFloat(Roughness, "Roughness");
}