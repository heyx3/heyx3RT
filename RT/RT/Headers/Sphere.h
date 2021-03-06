#pragma once

#include "Shape.h"


namespace RT
{
    //A sphere with unscaled radius 1.
    class RT_API Sphere : public Shape
    {
    public:

        //The axis that the UV's are wrapped around.
        //0 = X, 1 = Y, 2 = Z.
        unsigned char WrapAxis = 1;


        Sphere() { }
        Sphere(Vector3f pos, float radius) { Tr.ScaleBy(radius); Tr.SetPos(pos); }


        virtual void PrecalcData() override;

        virtual void GetBoundingBox(BoundingBox& outB) const override;
        virtual bool CastRay(const Ray& ray, Vertex& outHit, FastRand& prng,
                             float tMin = 0.0f,
                             float tMax = std::numeric_limits<float>::infinity()) const override;


        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;


    private:

        BoundingBox bounds;

        void FillInData(Vertex& hitPos, Vector3f localIntersectPos, Vector3f worldIntersectPos) const;


        ADD_SHAPE_REFLECTION_DATA_H(Sphere);
    };
}