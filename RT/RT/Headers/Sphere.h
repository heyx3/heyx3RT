#pragma once

#include "Shape.h"


//A sphere with unscaled radius 1.
class RT_API Sphere : public Shape
{
public:

    Sphere() { }
    Sphere(Vector3f pos, float radius)
    {
        Tr.ScaleBy(radius);
        Tr.SetPos(pos);
    }


    virtual void PrecalcData() override;

    virtual void GetBoundingBox(BoundingBox& outB) const override;
    virtual bool CastRay(const Ray& ray, Vertex& outHit) const override;


private:

    BoundingBox bounds;

    void FillInData(Vertex& hitPos, const Vector3f& localPos) const;
};