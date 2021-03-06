#pragma once

#include "Material.h"
#include "MaterialValues.h"


namespace RT
{
    class RT_API Material_Metal : public Material
    {
    public:

        MaterialValue::Ptr Albedo, Roughness, Emissive;


        Material_Metal(MaterialValue::Ptr albedo = new MV_Constant(Vector3f(1.0f, 1.0f, 1.0f)),
                       MaterialValue::Ptr roughness = new MV_Constant(0.0f),
                       MaterialValue::Ptr emissive = new MV_Constant(Vector3f(0.0f, 0.0f, 0.0f)))
            : Albedo(albedo), Roughness(roughness), Emissive(emissive) { }


        virtual bool Scatter(const Ray& rIn, const Vertex& surface,
                             const Shape& shpe, FastRand& prng,
                             Vector3f& outAttenuation, Vector3f& outEmission, Ray& outRay) const override;


        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;


        ADD_MATERIAL_REFLECTION_DATA_H(Material_Metal, Metal);
    };
}