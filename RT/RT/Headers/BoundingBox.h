#pragma once

#include "Ray.h"


struct RT_API BoundingBox
{
public:

    Vector3f Min, Max;

    BoundingBox() : Min(), Max() { }
    BoundingBox(const Vector3f& min, const Vector3f& max) : Min(min), Max(max) { }

    bool RayIntersects(const Ray& ray) const;
};