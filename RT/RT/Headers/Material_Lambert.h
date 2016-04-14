#pragma once

#include "Material.h"


//A solid color material.
class RT_API Material_Lambert : public Material
{
public:

    Vector3f Color;

    
    Material_Lambert(Vector3f col = Vector3f(1.0f, 1.0f, 1.0f)) : Color(col) { }


    virtual bool Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                         FastRand& prng, Vector3f& attenuation, Ray& rOut) const override;
};