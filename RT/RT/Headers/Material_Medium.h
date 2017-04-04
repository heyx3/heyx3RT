#pragma once

#include "Material.h"
#include "MaterialValues.h"


namespace RT
{
    //A material with a straightforward surface color and which always scatters randomly.
    class RT_API Material_Medium : public Material
    {
    public:

        MaterialValue::Ptr Albedo;

        Material_Medium(MaterialValue::Ptr albedo = new MV_Constant(Vector3f(1.0f, 1.0f, 1.0f)))
            : Albedo(albedo) { }

        virtual bool Scatter(const Ray& rIn, const Vertex& surface,
                             const Shape& shpe, FastRand& prng,
                             Vector3f& outAttenuation, Vector3f& outEmission, Ray& outRay) const override;

        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;

        ADD_MATERIAL_REFLECTION_DATA_H(Material_Medium, Medium);
    };
}