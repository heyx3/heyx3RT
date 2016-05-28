#pragma once

#include <vector>
#include <assert.h>

#include "Triangle.h"
#include "Shape.h"


#pragma warning (disable: 4251)
EXPORT_STL_VECTOR(Triangle);
EXPORT_STL_VECTOR(Vertex);
#pragma warning (default: 4251)


//TODO: Also provide "IndexedMesh" and "MeshFile" class.
struct RT_API Mesh : public Shape
{
public:

    std::vector<Triangle> Tris;


    Mesh() { }
    Mesh(const std::vector<Triangle>& tris): Tris(tris) { }

    //Every group of three consecutive vertices makes one triangle.
    //If the last group of triangles has less than three vertices, it is ignored.
    Mesh(const std::vector<Vertex>& verts);


    virtual void PrecalcData() override;

    virtual void GetBoundingBox(BoundingBox& b) const override { b = bounds; }
    virtual bool CastRay(const Ray& ray, Vertex& outHit) const override;


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


private:

    BoundingBox bounds;


    ADD_SHAPE_REFLECTION_DATA_H(Mesh);
};