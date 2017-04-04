#include "../Headers/Sphere.h"

using namespace RT;


ADD_SHAPE_REFLECTION_DATA_CPP(Sphere);


namespace
{
    float max(float f1, float f2) { return (f1 > f2) ? f1 : f2; }
    float min(float f1, float f2) { return (f1 > f2) ? f2 : f1; }
}


void Sphere::PrecalcData()
{
    //http://stackoverflow.com/questions/4368961/calculating-an-aabb-for-a-transformed-sphere

    float sVals[4][4] = { { 1.0f, 0.0f, 0.0f, 0.0f },
                          { 0.0f, 1.0f, 0.0f, 0.0f },
                          { 0.0f, 0.0f, 1.0f, 0.0f },
                          { 0.0f, 0.0f, 0.0f, -1.0f } };
    Matrix4f s(sVals);
    Matrix4f invS;
    s.GetInverse(invS);

    Matrix4f mTransp;
    Tr.GetMatToWorld().GetTranspose(mTransp);

    Matrix4f r(mTransp, Matrix4f(invS, Tr.GetMatToWorld()));

    float denom = 1.0f / r.Get(3, 3);
    float tempZ = sqrt((r.Get(2, 3) * r.Get(2, 3)) - (r.Get(3, 3) * r.Get(2, 2))),
          tempY = sqrt((r.Get(1, 3) * r.Get(1, 3)) - (r.Get(3, 3) * r.Get(1, 1))),
          tempX = sqrt((r.Get(0, 3) * r.Get(0, 3)) - (r.Get(3, 3) * r.Get(0, 0)));
    float z1 = (r.Get(2, 3) + tempZ) * denom,
          z2 = (r.Get(2, 3) - tempZ) * denom,
          y1 = (r.Get(1, 3) + tempY) * denom,
          y2 = (r.Get(1, 3) - tempY) * denom,
          x1 = (r.Get(0, 3) + tempX) * denom,
          x2 = (r.Get(0, 3) - tempX) * denom;

    bounds.Min = Vector3f(min(x1, x2), min(y1, y2), min(z1, z2));
    bounds.Max = Vector3f(max(x1, x2), max(y1, y2), max(z1, z2));
}

void Sphere::GetBoundingBox(BoundingBox& outB) const
{
    outB = bounds;
}
bool Sphere::CastRay(const Ray& ray, Vertex& outHit, FastRand& prng,
                     float tMin, float tMax) const
{
    //http://stackoverflow.com/questions/6533856/ray-sphere-intersection

    //Transform the ray to local space.
    //Note that this sphere has a radius of 1.0 in that space.

    Ray newRay(Tr.Point_WorldToLocal(ray.GetPos()),
               Tr.Dir_WorldToLocal(ray.GetDir()).Normalize());

    float a = newRay.GetDir().Dot(newRay.GetDir()),
          b = 2.0f * newRay.GetDir().Dot(newRay.GetPos()),
          c = newRay.GetPos().Dot(newRay.GetPos()) - 1.0f;

    float discriminant = (b * b) - (4.0f * a * c);

    if (discriminant < 0.0f)
    {
        return false;
    }

    //Find the intersection distance,
    //    either along the local-space ray or the world-space ray.
    float outDist;
    bool outDistIsLocal;
    if (discriminant == 0.0f)
    {
        outDist = -b / (2.0f * a);
        outDistIsLocal = true;
    }
    else
    {
        float inv2a = 0.5f / a,
              temp = sqrtf(discriminant);
        float t1 = (-b - temp) * inv2a,
              t2 = (-b + temp) * inv2a;

        //Get the world-space t values for comparison against tMin and tMax.
        //Common edge-case: if tMin is 0 and tMax is +INF, no need to bother.
        if (tMin == 0.0f && tMax == std::numeric_limits<float>::infinity())
        {
            outDistIsLocal = true;
        }
        else
        {
            outDistIsLocal = false;

            Vector3f localHit1 = newRay.GetPos(t1),
                     localHit2 = newRay.GetPos(t2);
            Vector3f worldHit1 = Tr.Point_LocalToWorld(localHit1),
                     worldHit2 = Tr.Point_LocalToWorld(localHit2);

            t1 = ray.GetT(worldHit1);
            t2 = ray.GetT(worldHit2);
        }

        bool t1Invalid = (t1 < tMin || t1 > tMax),
             t2Invalid = (t2 < tMin || t2 > tMax);

        if (t2Invalid)
            if (t1Invalid)
                return false;
            else
                outDist = t1;
        else if (t1Invalid)
            outDist = t2;
        else
            outDist = min(t1, t2);
    }
    
    //Calculate world-space intersection data.
    Vector3f localHit, worldHit;
    if (outDistIsLocal)
    {
        localHit = newRay.GetPos(outDist);
        worldHit = Tr.Point_LocalToWorld(localHit);
    }
    else
    {
        worldHit = ray.GetPos(outDist);
        localHit = Tr.Point_WorldToLocal(worldHit);
    }
    FillInData(outHit, localHit, worldHit);

    return true;
}
void Sphere::FillInData(Vertex& v, Vector3f localIntersectPos, Vector3f worldIntersectPos) const
{
    v.Pos = worldIntersectPos;

    Vector3f localNormal = localIntersectPos.Normalize(); //TODO: Is normalization necessary?

    v.Normal = Tr.Normal_LocalToWorld(localNormal).Normalize();
    v.Tangent = v.Normal.Cross(fabs(v.Normal.x) == 1.0f ? Vector3f::Y() : Vector3f::X()).Normalize();
    v.Bitangent = v.Normal.Cross(v.Tangent);
    
    size_t otherAxis1 = (WrapAxis == 0 ? 2 : (WrapAxis - 1)),
           otherAxis2 = (WrapAxis == 2 ? 0 : (WrapAxis + 1));
    const float invPi = 1.0f / (float)M_PI,
                inv2Pi = 0.5f * invPi;
    v.UV.x = 0.5f + (inv2Pi * atan2(localNormal[otherAxis2], localNormal[otherAxis1]));
    v.UV.y = 0.5f - (invPi * asin(localNormal[WrapAxis]));
}

void Sphere::WriteData(DataWriter& writer) const
{
    Shape::WriteData(writer);
    writer.WriteByte(WrapAxis, "WrapAxis");
}
void Sphere::ReadData(DataReader& reader)
{
    Shape::ReadData(reader);
    reader.ReadByte(WrapAxis, "WrapAxis");
    if (WrapAxis > 2)
    {
        reader.ErrorMessage = "Sphere wrap axis should be [0, 2] but it was ";
        reader.ErrorMessage += String(WrapAxis);
        throw DataReader::EXCEPTION_FAILURE;
    }
}