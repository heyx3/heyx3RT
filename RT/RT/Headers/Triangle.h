#pragma once

#include "Vertex.h"
#include "Ray.h"
#include "Transform.h"


namespace RT
{
    //A triangle in a mesh.
    struct RT_API Triangle
    {
    public:

        Vertex Verts[3];


        Triangle() { }
        Triangle(const Vertex& v1, const Vertex& v2, const Vertex& v3)
            { Verts[0] = v1; Verts[1] = v2; Verts[2] = v3; }


        //Call this after modifying this triangle's position.
        //Pre-calculates data that will speed up calculation of ray intersections.
        void PrecalcData();

        //Given a position on this triangle, fills in the normal, UV, etc. of that position
        //    by interpolating across the surface of this triangle.
        //Transforms all data (including input position) using the given matrix.
        void GetMoreData(Vertex& vert, const Transform& worldTransform) const;

        //Returns whether an intersection actually happened.
        bool RayIntersect(const Ray& ray, Vector3f& outPos, float& outDistance,
                          float tMin = 0.0f,
                          float tMax = std::numeric_limits<float>::infinity()) const;


    private:

        Vector3f e1, e2;
        float length01, length02, length12, invTotalArea;
    };
}