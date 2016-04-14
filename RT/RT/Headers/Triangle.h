#pragma once

#include "Vertex.h"
#include "Ray.h"
#include "Matrix4f.h"


//A triangle in a mesh.
struct RT_API Triangle
{
public:

    Vertex Verts[3];



    //Call this after modifying this triangle's position.
    //Pre-calculates data that will speed up calculation of ray intersections.
    void PrecalcData();

    //Given a position on this triangle, fills in the normal, UV, etc. of that position
    //    by interpolating across the surface of this triangle.
    //Transforms all data (including input position) using the given matrix.
    void GetMoreData(Vertex& vert, const Matrix4f& worldTransform) const;

    //Returns whether an intersection actually happened.
    bool RayIntersect(const Ray& ray, Vector3f& outPos, float& outDistance) const;


private:

    Vector3f e1, e2;
    float length01, length02, length12, invTotalArea;
};