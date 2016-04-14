#pragma once

#include "BoundingBox.h"
#include "Vertex.h"
#include "Transform.h"



//An abstract class that represents some geometry.
class RT_API Shape
{
public:

    Transform Tr;


    Shape() { }
    virtual ~Shape() { }


    virtual void PrecalcData() { }

    virtual void GetBoundingBox(BoundingBox& outBox) const = 0;
    virtual bool CastRay(const Ray& ray, Vertex& outHit) const = 0;
};