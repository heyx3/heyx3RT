#pragma once

#include "SkyMaterial.h"
#include "MaterialValues.h"


class RT_API SkyMaterial_SimpleColor : public SkyMaterial
{
public:

    MaterialValue::Ptr Color;


    SkyMaterial_SimpleColor(MaterialValue::Ptr col = new MV_Constant(Vector3f(0.5f, 0.5f, 1.0f)))
        : Color(col.Release()) { }


    virtual Vector3f GetColor(const Ray& ray, FastRand& prng) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


    ADD_SKYMAT_REFLECTION_DATA_H(SkyMaterial_SimpleColor, SimpleColor);
};