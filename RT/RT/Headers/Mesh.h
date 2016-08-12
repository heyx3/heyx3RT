#pragma once

#include <assert.h>

#include "Triangle.h"
#include "Shape.h"
#include "List.h"


namespace RT
{
    EXPORT_RT_LIST(Triangle);
    EXPORT_RT_LIST(Vertex);


    //TODO: Also provide "IndexedMesh" and "MeshFile" class.
    struct RT_API Mesh : public Shape
    {
    public:

        List<Triangle> Tris;


        Mesh() { }
        Mesh(const List<Triangle>& tris): Tris(tris) { }

        //Every group of three consecutive vertices makes one triangle.
        //If the last group of triangles has less than three vertices, it is ignored.
        Mesh(const List<Vertex>& verts);


        virtual void PrecalcData() override;

        virtual void GetBoundingBox(BoundingBox& b) const override { b = bounds; }
        virtual bool CastRay(const Ray& ray, Vertex& outHit) const override;


        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;


    private:

        BoundingBox bounds;


        ADD_SHAPE_REFLECTION_DATA_H(Mesh);
    };
}