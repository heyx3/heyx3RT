#pragma once

#include "SkyMaterial.h"
#include "MaterialValues.h"


class RT_API SkyMaterial_VerticalGradient : public SkyMaterial
{
public:

    MaterialValue::Ptr BottomCol, TopCol;

    
    SkyMaterial_VerticalGradient(MaterialValue::Ptr bottomCol = new MaterialValue_Constant(Vector3f()),
                                 MaterialValue::Ptr topCol = new MaterialValue_Constant(Vector3f(1.0f, 1.0f, 1.0f)))
        : BottomCol(bottomCol.Release()), TopCol(topCol.Release()) { }


    virtual Vector3f GetColor(const Ray& ray, FastRand& prng) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


    ADD_SKYMAT_REFLECTION_DATA_H(SkyMaterial_VerticalGradient, VerticalGradient);
};