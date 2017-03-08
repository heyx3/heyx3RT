#pragma once

#include "Material.h"
#include "MaterialValues.h"


namespace RT
{
    class RT_API Material_Dielectric : public Material
    {
    public:

        MaterialValue::Ptr IndexOfRefraction;


        Material_Dielectric(MaterialValue::Ptr indexOfRefraction = MV_Constant::Create(1.0f))
            : IndexOfRefraction(indexOfRefraction) { }


        virtual bool Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                             FastRand& prng, Vector3f& attenuation, Vector3f& emission,
                             Ray& rOut) const override;


        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;


        ADD_MATERIAL_REFLECTION_DATA_H(Material_Dielectric, Dielectric);
    };
}