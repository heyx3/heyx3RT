#pragma once

#include "Shape.h"


//A finite plane.
class RT_API Plane : public Shape
{
public:

    static const Vector3f LocalNormal;

    bool IsOneSided;


    Plane(bool isOneSided = false) : IsOneSided(isOneSided) { }


    virtual void PrecalcData() override;

    virtual void GetBoundingBox(BoundingBox& outBox) const override;
    virtual bool CastRay(const Ray& ray, Vertex& outHit) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


private:

    Vector3f normal, tangent, bitangent;
    float planePos;

    BoundingBox bounds;


    ADD_SHAPE_REFLECTION_DATA_H(Plane);
};