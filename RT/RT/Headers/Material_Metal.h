#pragma once

#include "Material.h"
#include "MaterialValues.h"


class RT_API Material_Metal : public Material
{
public:

    MaterialValue::Ptr Albedo;
    MaterialValue::Ptr Roughness;


    Material_Metal(MaterialValue::Ptr albedo = new MaterialValue_Constant(Vector3f(1.0f, 1.0f, 1.0f)),
                   MaterialValue::Ptr roughness = new MaterialValue_Constant(0.0f))
        : Albedo(albedo.Release()), Roughness(roughness.Release()) { }


    virtual bool Scatter(const Ray& rIn, const Vertex& surf, const Shape& shpe,
                         FastRand& prng, Vector3f& attenuation, Ray& rOut) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


    ADD_MATERIAL_REFLECTION_DATA_H(Material_Metal, Metal);
};