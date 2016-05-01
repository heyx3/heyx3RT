#pragma once

#include "MaterialValue.h"
#include "Texture2D.h"


class MaterialValue_Constant : public MaterialValue
{
public:

    Vector3f Value;
    MaterialValue_Constant(const Vector3f& value) : Value(value) { }
    virtual Vector3f GetValue(const Vertex& surface, const Ray& ray) const override { return Value; }


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;
};

class MaterialValue_Tex2D : public MaterialValue
{
public:

    const Texture2D* Tex;
    MaterialValue_Tex2D(const Texture2D* tex) : Tex(tex) { }
    virtual Vector3f GetValue(const Vertex& surface, const Ray& r) const override { return Tex->GetColor(surface.UV[0], surface.UV[1]); }


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;
};