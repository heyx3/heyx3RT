#pragma once

#include "Main.hpp"
#include "Tracer.h"


//A way to calculate the color of the sky (i.e. a ray that didn't hit anything).
class RT_API SkyMaterial
{
public:

    virtual ~SkyMaterial() { }


    //Gets the color on the given surface using the given ray tracer.
    virtual Vector3f GetColor(const Ray& ray) const = 0;
};