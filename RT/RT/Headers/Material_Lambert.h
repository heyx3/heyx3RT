#pragma once

#include "Material.h"
#include "MaterialValues.h"


class RT_API Material_Lambert : public Material
{
public:

    MaterialValue::Ptr Color;

    
    Material_Lambert(MaterialValue::Ptr& col = new MaterialValue_Constant(Vector3f(1.0f, 1.0f, 1.0f)))
        : Color(col.Release()) { }


    virtual bool Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                         FastRand& prng, Vector3f& attenuation, Ray& rOut) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


    ADD_MATERIAL_REFLECTION_DATA_H(Material_Lambert);
};