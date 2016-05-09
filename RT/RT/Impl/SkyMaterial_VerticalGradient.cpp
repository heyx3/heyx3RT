#include "../Headers/SkyMaterial_VerticalGradient.h"

#include "../Headers/Mathf.h"


ADD_SKYMAT_REFLECTION_DATA_CPP(SkyMaterial_VerticalGradient);


Vector3f SkyMaterial_VerticalGradient::GetColor(const Ray& ray, FastRand& prng) const
{
    return Vector3f::Lerp(BottomCol->GetValue(ray, prng),
                          TopCol->GetValue(ray, prng),
                          0.5f + (0.5f * ray.GetDir().y));
}
void SkyMaterial_VerticalGradient::WriteData(DataWriter& writer) const
{
    SkyMaterial::WriteData(writer);

    MaterialValue::WriteValue(BottomCol, writer, "BottomCol");
    MaterialValue::WriteValue(TopCol, writer, "TopCol");
}
void SkyMaterial_VerticalGradient::ReadData(DataReader& reader)
{
    SkyMaterial::ReadData(reader);

    MaterialValue::ReadValue(BottomCol, reader, "BottomCol");
    MaterialValue::ReadValue(TopCol, reader, "TopCol");
}