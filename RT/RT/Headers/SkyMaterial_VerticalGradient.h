#pragma once

#include "SkyMaterial.h"


class RT_API SkyMaterial_VerticalGradient : public SkyMaterial
{
public:

    Vector3f BottomCol, TopCol;

    
    SkyMaterial_VerticalGradient(Vector3f bottomCol = Vector3f(),
                                 Vector3f topCol = Vector3f(1.0f, 1.0f, 1.0f))
        : BottomCol(bottomCol), TopCol(topCol) { }


    virtual Vector3f GetColor(const Ray& ray) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;
};