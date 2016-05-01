#include "../Headers/Mesh.h"


Mesh::Mesh() : NTris(0), Tris(nullptr) { }
Mesh::Mesh(Triangle* tris, int nTris)
    : NTris(nTris), Tris(new Triangle[nTris])
{
    memcpy(Tris, tris, sizeof(Triangle) * nTris);
}

Mesh::~Mesh()
{
    if (Tris != nullptr)
    {
        delete[] Tris;
        Tris = nullptr;
        NTris = 0;
    }
}

void Mesh::PrecalcData()
{
    if (NTris == 0)
    {
        bounds.Min = Vector3f();
        bounds.Max = Vector3f();
    }

    bounds.Min = Tris[0].Verts[0].Pos;
    bounds.Max = Tris[0].Verts[0].Pos;

    for (int tri = 0; tri < NTris; ++tri)
    {
        for (int i = 0; i < 3; ++i)
        {
            bounds.Min.x = std::fminf(bounds.Min.x, Tris[tri].Verts[i].Pos.x);
            bounds.Min.y = std::fminf(bounds.Min.y, Tris[tri].Verts[i].Pos.y);
            bounds.Min.z = std::fminf(bounds.Min.z, Tris[tri].Verts[i].Pos.z);
            bounds.Max.x = std::fmaxf(bounds.Max.x, Tris[tri].Verts[i].Pos.x);
            bounds.Max.y = std::fmaxf(bounds.Max.y, Tris[tri].Verts[i].Pos.y);
            bounds.Max.z = std::fmaxf(bounds.Max.z, Tris[tri].Verts[i].Pos.z);
        }
    }
}
bool Mesh::CastRay(const Ray& ray, Vertex& outHit) const
{
    if (!bounds.RayIntersects(ray))
        return false;

    //TODO: Try not bothering to normalize the local ray's direction.
    Ray newRay(Tr.GetMatToLocal().ApplyPoint(ray.GetPos()),
               Tr.GetMatToLocal().ApplyVector(ray.GetDir()).Normalize());

    const Triangle* closest = nullptr;
    float hitDist = std::numeric_limits<float>().infinity();
    for (int i = 0; i < NTris; ++i)
    {
        float tempT;
        Vector3f tempPos;

        if (Tris[i].RayIntersect(newRay, tempPos, tempT))
        {
            if (tempT < hitDist)
            {
                hitDist = tempT;
                outHit.Pos = tempPos;
                closest = &Tris[i];
            }
        }
    }

    if (closest != nullptr)
    {
        closest->GetMoreData(outHit, Tr.GetMatToWorld());
        return true;
    }

    return false;
}