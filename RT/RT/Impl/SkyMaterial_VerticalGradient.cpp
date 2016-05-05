#include "../Headers/SkyMaterial_VerticalGradient.h"

#include "../Headers/Mathf.h"


ADD_SKYMAT_REFLECTION_DATA_CPP(SkyMaterial_VerticalGradient);


Vector3f SkyMaterial_VerticalGradient::GetColor(const Ray& ray) const
{
    return Vector3f::Lerp(BottomCol, TopCol, 0.5f + (0.5f * ray.GetDir().y));
}
void SkyMaterial_VerticalGradient::WriteData(DataWriter& writer) const
{
    SkyMaterial::WriteData(writer);

    writer.WriteVec3f(BottomCol, "BottomCol");
    writer.WriteVec3f(TopCol, "TopCol");
}
void SkyMaterial_VerticalGradient::ReadData(DataReader& reader)
{
    SkyMaterial::ReadData(reader);

    reader.ReadVec3f(BottomCol, "BottomCol");
    reader.ReadVec3f(TopCol, "TopCol");
}