#include "../Headers/Material_Lambert.h"

using namespace RT;


ADD_MATERIAL_REFLECTION_DATA_CPP(Material_Lambert);


bool Material_Lambert::Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                               FastRand& prng, Vector3f& attenuation, Ray& rOut) const
{
    Vector3f newPos = surface.Pos + (surface.Normal * 0.001f);
    Vector3f targetPos = surface.Pos + surface.Normal + prng.GetRandUnitVector();
    rOut = Ray(newPos, (targetPos - newPos).Normalize());

    attenuation = Color->GetValue(rIn, prng, &shpe, &surface);

    return true;
}

void Material_Lambert::WriteData(DataWriter& writer) const
{
    Material::WriteData(writer);
    MaterialValue::WriteValue(Color, writer, "Color");
}
void Material_Lambert::ReadData(DataReader& reader)
{
    Material::ReadData(reader);
    MaterialValue::ReadValue(Color, reader, "Color");
}