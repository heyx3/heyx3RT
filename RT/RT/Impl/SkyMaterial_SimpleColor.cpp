#include "../Headers/SkyMaterial_SimpleColor.h"


ADD_SKYMAT_REFLECTION_DATA_CPP(SkyMaterial_SimpleColor);


Vector3f SkyMaterial_SimpleColor::GetColor(const Ray& ray) const
{
    return Color->GetValue(;
}
void SkyMaterial_SimpleColor::WriteData(DataWriter& writer) const
{
    SkyMaterial::WriteData(writer);

    writer.WriteVec3f(Color, "Color");
}
void SkyMaterial_SimpleColor::ReadData(DataReader& reader)
{
    SkyMaterial::ReadData(reader);

    reader.ReadVec3f(Color, "Color");
}