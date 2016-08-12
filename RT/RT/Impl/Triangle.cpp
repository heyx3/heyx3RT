#include "../Headers/Triangle.h"

using namespace RT;


namespace
{
    float GetArea(const Vector3f& p1, const Vector3f& p2, const Vector3f& p3,
                  float p1P2Dist)
    {
        float b = p1.Distance(p3);
        float c = p2.Distance(p3);
        float s = (p1P2Dist + b + c) * 0.5f;
        return sqrtf(s * (s - p1P2Dist) * (s - b) * (s - c));
    }
}


bool Triangle::RayIntersect(const Ray& ray, Vector3f& outPos, float& outDist) const
{
    //Miller-Trumbore intersection algorithm.

    Vector3f p = ray.GetDir().Cross(e2);
    float determinant = e1.Dot(p);

    const float EPSILON = 0.00001f;
    if (determinant > -EPSILON && determinant < EPSILON)
        return false;

    float invDet = 1.0f / determinant;

    Vector3f T = ray.GetPos() - Verts[0].Pos;
    float u = T.Dot(p) * invDet;
    if (u < 0.0f || u > 1.0f)
        return false;

    Vector3f q = T.Cross(e1);

    float v = ray.GetDir().Dot(q) * invDet;
    if (v < 0.0f || (u + v) > 1.0f)
        return false;

    outDist = e2.Dot(q) * invDet;
    if (outDist > EPSILON)
    {
        outPos = ray.GetPos() + (ray.GetDir() * outDist);
        return true;
    }

    return false;
}
void Triangle::PrecalcData()
{
    e1 = Verts[1].Pos - Verts[0].Pos;
    e2 = Verts[2].Pos - Verts[0].Pos;

    //Heron's formula for the area of a triangle.
    length01 = Verts[0].Pos.Distance(Verts[1].Pos);
    length02 = Verts[0].Pos.Distance(Verts[2].Pos);
    length12 = Verts[1].Pos.Distance(Verts[2].Pos);
    float s = (length01 + length02 + length12) * 0.5f;
    invTotalArea = 1.0f / sqrtf(s * (s - length01) * (s - length12) * (s - length02));
}

void Triangle::GetMoreData(Vertex& vert, const Matrix4f& worldM) const
{
    float area2 = invTotalArea * GetArea(Verts[0].Pos, Verts[1].Pos, vert.Pos, length01),
          area1 = invTotalArea * GetArea(Verts[0].Pos, Verts[2].Pos, vert.Pos, length02),
          area0 = invTotalArea * GetArea(Verts[1].Pos, Verts[2].Pos, vert.Pos, length12);

    vert.Normal = ((Verts[0].Normal * area0) + (Verts[1].Normal * area1) + (Verts[2].Normal * area2)).Normalize();
    vert.Tangent = ((Verts[0].Tangent * area0) + (Verts[1].Tangent * area1) + (Verts[2].Tangent * area2)).Normalize();

    vert.Normal = worldM.ApplyVector(vert.Normal).Normalize();
    vert.Tangent = worldM.ApplyVector(vert.Tangent).Normalize();
    vert.Bitangent = vert.Normal.Cross(vert.Tangent);

    vert.UV = (Verts[0].UV * area0) + (Verts[1].UV * area1) + (Verts[2].UV * area2);

    vert.Pos = worldM.ApplyPoint(vert.Pos);
}