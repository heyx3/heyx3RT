#pragma once

#include "FastRand.h"
#include "Tracer.h"


//A way to calculate the color of a surface.
class RT_API Material
{
public:

    //Converts a tangent-space normal to world space.
    //Used for normal-mapping.
    static Vector3f TangentSpaceToWorldSpace(const Vector3f& tangentSpaceNormal,
                                             const Vector3f& worldNormal,
                                             const Vector3f& worldTangent,
                                             const Vector3f& worldBitangent);


    virtual ~Material() { }


    //Scatters the given incoming ray after it hits the given surface point of this material.
    //Also potentially attenuates the ray.
    //Returns "true" if the ray scattered, or "false" if the ray was absorbed.
    virtual bool Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                         FastRand& prng, Vector3f& attenuation, Ray& rOut) const = 0;
};