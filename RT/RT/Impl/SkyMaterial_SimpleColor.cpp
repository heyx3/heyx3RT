#include "../Headers/SkyMaterial_SimpleColor.h"


ADD_SKYMAT_REFLECTION_DATA_CPP(SkyMaterial_SimpleColor);


Vector3f SkyMaterial_SimpleColor::GetColor(const Ray& ray, FastRand& prng) const
{
    return Color->GetValue(ray, prng);
}
void SkyMaterial_SimpleColor::WriteData(DataWriter& writer) const
{
    SkyMaterial::WriteData(writer);

    MaterialValue::WriteValue(Color, writer, "Color");
}
void SkyMaterial_SimpleColor::ReadData(DataReader& reader)
{
    SkyMaterial::ReadData(reader);

    MaterialValue::ReadValue(Color, reader, "Color");
}