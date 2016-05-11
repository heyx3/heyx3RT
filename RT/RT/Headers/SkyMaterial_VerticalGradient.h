#pragma once

#include "SkyMaterial.h"
#include "MaterialValues.h"


class RT_API SkyMaterial_VerticalGradient : public SkyMaterial
{
public:

    MaterialValue::Ptr BottomCol, TopCol;
    MaterialValue::Ptr SkyDir;

    
    SkyMaterial_VerticalGradient(MaterialValue::Ptr bottomCol = new MV_Constant(Vector3f()),
                                 MaterialValue::Ptr topCol = new MV_Constant(Vector3f(1.0f, 1.0f, 1.0f)),
                                 MaterialValue::Ptr skyDir = new MV_Constant(Vector3f(0.0f, 1.0f, 0.0f)))
        : BottomCol(bottomCol.Release()), TopCol(topCol.Release()), SkyDir(skyDir.Release()) { }


    virtual Vector3f GetColor(const Ray& ray, FastRand& prng) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


    ADD_SKYMAT_REFLECTION_DATA_H(SkyMaterial_VerticalGradient, VerticalGradient);
};