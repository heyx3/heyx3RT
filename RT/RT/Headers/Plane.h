#pragma once

#include "Shape.h"


namespace RT
{
    //A finite plane.
    //If no transforms are applied, it stretches from { -1, 0, -1 } to { 1, 0, 1 }.
    class RT_API Plane : public Shape
    {
    public:

        static const Vector3f LocalNormal;

        bool IsOneSided;


        Plane() { }
        Plane(Vector3f pos, float size, bool isOneSided = false)
            : Shape(pos, size), IsOneSided(isOneSided) { }


        virtual void PrecalcData() override;

        virtual void GetBoundingBox(BoundingBox& outBox) const override;
        virtual bool CastRay(const Ray& ray, Vertex& outHit, FastRand& prng,
                             float tMin = 0.0f,
                             float tMax = std::numeric_limits<float>::infinity()) const override;


        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;


    private:

        Vector3f normal, tangent, bitangent;
        float planePos;

        BoundingBox bounds;


        ADD_SHAPE_REFLECTION_DATA_H(Plane);
    };
}