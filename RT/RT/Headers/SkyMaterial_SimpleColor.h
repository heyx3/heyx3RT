#pragma once

#include "SkyMaterial.h"


class RT_API SkyMaterial_SimpleColor : public SkyMaterial
{
public:

    Vector3f Color;


    SkyMaterial_SimpleColor(Vector3f col = Vector3f(0.5f, 0.5f, 1.0f)) : Color(col) { }


    virtual Vector3f GetColor(const Ray& ray) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;
};