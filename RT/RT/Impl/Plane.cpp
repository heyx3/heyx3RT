#include "../Headers/Plane.h"

using namespace RT;


ADD_SHAPE_REFLECTION_DATA_CPP(Plane);


namespace
{
    float max(float f1, float f2) { return (f1 > f2) ? f1 : f2; }
    float min(float f1, float f2) { return (f1 > f2) ? f2 : f1; }
}


const Vector3f Plane::LocalNormal = Vector3f::Y();


void Plane::PrecalcData()
{
    normal = Tr.Normal_LocalToWorld(LocalNormal).Normalize();
    normal.GetOrthoBasis(tangent, bitangent);

    planePos = -Tr.GetPos().Dot(normal);

    Vector3f p1 = Tr.Point_LocalToWorld(Vector3f(-1.0f, 0.0, -1.0f)),
             p2 = Tr.Point_LocalToWorld(Vector3f(1.0f, 0.0f, -1.0f)),
             p3 = Tr.Point_LocalToWorld(Vector3f(-1.0f, 0.0f, 1.0f)),
             p4 = Tr.Point_LocalToWorld(Vector3f(1.0f, 0.0f, 1.0f));

    bounds.Min = Vector3f(min(p1.x, min(p2.x, min(p3.x, p4.x))),
                          min(p1.y, min(p2.y, min(p3.y, p4.y))),
                          min(p1.z, min(p2.z, min(p3.z, p4.z))));
    bounds.Max = Vector3f(max(p1.x, max(p2.x, max(p3.x, p4.x))),
                          max(p1.y, max(p2.y, max(p3.y, p4.y))),
                          max(p1.z, max(p2.z, max(p3.z, p4.z))));
}
void Plane::GetBoundingBox(BoundingBox& outB) const
{
    outB = bounds;
}
bool Plane::CastRay(const Ray& ray, Vertex& outHit) const
{
    //If ray is not pointing towards this plane's surface, exit.
    float dotted = normal.Dot(ray.GetDir());
    if (dotted == 0.0f || (IsOneSided && dotted > 0.0f))
        return false;

    //If ray intersection is behind the ray's start, exit.
    float dotted2 = -(normal.Dot(ray.GetPos()) + planePos);
    float t = dotted2 / dotted;
    if (t < 0.0f)
        return false;

    outHit.Pos = ray.GetPos(t);

    //If ray intersection is outside the plane's bounds, exit.
    Vector3f localHitPos = Tr.Point_WorldToLocal(outHit.Pos);
    if (localHitPos.x < -1.0f || localHitPos.x > 1.0f ||
        localHitPos.z < -1.0f || localHitPos.z > 1.0f)
    {
        return false;
    }

    //Compute the normal, tangent, and bitangent.
    outHit.Normal = normal;
    outHit.Tangent = tangent;
    outHit.Bitangent = bitangent;
    if (dotted > 0.0f)
    {
        outHit.Normal = -outHit.Normal;
        outHit.Tangent = -outHit.Tangent;
        outHit.Bitangent = -outHit.Bitangent;
    }

    //Compute UV.
    outHit.UV = (Vector2f(localHitPos.x, localHitPos.z) * -0.5f) + 0.5f;

    return true;
}
void Plane::WriteData(DataWriter& writer) const
{
    Shape::WriteData(writer);
    writer.WriteBool(IsOneSided, "IsOneSided");
}
void Plane::ReadData(DataReader& reader)
{
    Shape::ReadData(reader);
    reader.ReadBool(IsOneSided, "IsOneSided");
}