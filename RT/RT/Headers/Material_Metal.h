#pragma once

#include "Material.h"


class RT_API Material_Metal : public Material
{
public:

    Vector3f Albedo;
    float Roughness;


    Material_Metal(Vector3f albedo = Vector3f(1.0f, 1.0f, 1.0f), float roughness = 0.0f)
        : Albedo(albedo), Roughness(roughness) { }


    virtual bool Scatter(const Ray& rIn, const Vertex& surf, const Shape& shpe,
                         FastRand& prng, Vector3f& attenuation, Ray& rOut) const override;
};