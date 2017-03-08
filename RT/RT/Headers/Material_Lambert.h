#pragma once

#include "Material.h"
#include "MaterialValues.h"


namespace RT
{
    class RT_API Material_Lambert : public Material
    {
    public:

        MaterialValue::Ptr Albedo, Emissive;

    
        Material_Lambert(MaterialValue::Ptr albedo = new MV_Constant(Vector3f(1.0f, 1.0f, 1.0f)),
                         MaterialValue::Ptr emissive = new MV_Constant(Vector3f()))
            : Albedo(albedo), Emissive(emissive) { }


        virtual bool Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                             FastRand& prng, Vector3f& attenuation, Vector3f& emission,
                             Ray& rOut) const override;


        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;


        ADD_MATERIAL_REFLECTION_DATA_H(Material_Lambert, Lambert);
    };
}