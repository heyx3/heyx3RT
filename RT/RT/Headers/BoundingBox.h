#pragma once

#include "Ray.h"

#include <limits>


namespace RT
{
    struct RT_API BoundingBox
    {
    public:

        Vector3f Min, Max;

        BoundingBox() : Min(), Max() { }
        BoundingBox(const Vector3f& min, const Vector3f& max) : Min(min), Max(max) { }

        bool RayIntersects(const Ray& ray,
                           float tMin = 0.0f,
                           float tMax = std::numeric_limits<float>::infinity()) const;
    };
}