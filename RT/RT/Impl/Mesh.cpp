#include "../Headers/Mesh.h"


ADD_SHAPE_REFLECTION_DATA_CPP(Mesh);


Mesh::Mesh(const std::vector<Vertex>& verts)
{
    Tris.reserve(verts.size() / 3);
    for (int i = 0; (i + 2) < verts.size(); i += 3)
    {
        Tris.push_back(Triangle(verts[i], verts[i + 1], verts[i + 2]));
    }
}

void Mesh::PrecalcData()
{
    if (Tris.size() == 0)
    {
        bounds.Min = Vector3f();
        bounds.Max = Vector3f();
    }

    bounds.Min = Tris[0].Verts[0].Pos;
    bounds.Max = Tris[0].Verts[0].Pos;

    for (int tri = 0; tri < Tris.size(); ++tri)
    {
        Tris[tri].PrecalcData();
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
    for (int i = 0; i < Tris.size(); ++i)
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

void Mesh::WriteData(DataWriter& writer) const
{
    Shape::WriteData(writer);

    //Convert data to vertices for a more compact/simplified format.
    std::vector<Vertex> verts;
    verts.reserve(Tris.size() * 3);
    for (int i = 0; i < Tris.size(); ++i)
    {
        verts.push_back(Tris[i].Verts[0]);
        verts.push_back(Tris[i].Verts[1]);
        verts.push_back(Tris[i].Verts[2]);
    }

    writer.WriteList<Vertex>(verts.data(), verts.size(),
                             [](DataWriter& wr, const Vertex& v, const std::string& name)
                                 { wr.WriteDataStructure(Vertex_Writable(v), name); },
                             "Vertices");
}
void Mesh::ReadData(DataReader& reader)
{
    Shape::ReadData(reader);

    //Data is stored as vertices in the serializer.
    std::vector<Vertex> verts;
    reader.ReadList<Vertex>(&verts,
                            [](void* pList, size_t nElements)
                                { ((std::vector<Vertex>*)pList)->resize(nElements); },
                            [](DataReader& rd, void* pList, size_t i, const std::string& name)
                                { rd.ReadDataStructure(Vertex_Readable((*(std::vector<Vertex>*)pList)[i]),
                                                       name); },
                            "Vertices");

    Tris.clear();
    Tris.reserve(verts.size() / 3);
    for (int i = 0; (i + 2) < verts.size(); i += 3)
    {
        Tris.push_back(Triangle(verts[i], verts[i + 1], verts[i + 2]));
    }
}