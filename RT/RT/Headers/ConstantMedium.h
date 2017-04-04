#pragma once

#include "Shape.h"
#include "SmartPtrs.h"


namespace RT
{
    //A foggy medium with a constant uniform density.
    //It is highly recommended to pair this shape with Material_Medium.
    class RT_API ConstantMedium : public Shape
    {
    public:

        //The density of the medium.
        float Density;
        //The surface of this medium.
        SharedPtr<Shape> Surface;


        ConstantMedium(SharedPtr<Shape> surface = nullptr,
                       float density = 1.0f)
            : Surface(surface), Density(density) { }


        virtual void PrecalcData() override;

        virtual void GetBoundingBox(BoundingBox& outB) const override { Surface->GetBoundingBox(outB); }
        virtual bool CastRay(const Ray& ray, Vertex& outHit, FastRand& prng,
                             float tMin = 0.0f,
                             float tMax = std::numeric_limits<float>::infinity()) const override;

        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;


    private:
        
        ADD_SHAPE_REFLECTION_DATA_H(ConstantMedium);
    };
}