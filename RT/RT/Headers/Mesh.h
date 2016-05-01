#pragma once

#include <vector>
#include <assert.h>

#include "Triangle.h"
#include "Shape.h"


//TODO: Also provide "IndexedMesh" class.
struct RT_API Mesh : public Shape
{
public:

    Triangle* Tris;
    int NTris;


    Mesh();
    Mesh(Triangle* tris, int nTris);

    ~Mesh();


    virtual void PrecalcData() override;

    virtual void GetBoundingBox(BoundingBox& b) const override { b = bounds; }
    virtual bool CastRay(const Ray& ray, Vertex& outHit) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


private:

    BoundingBox bounds;
};