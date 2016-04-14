#include "../Headers/Sphere.h"


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
bool Sphere::CastRay(const Ray& ray, Vertex& outHit) const
{
    //http://stackoverflow.com/questions/6533856/ray-sphere-intersection

    //Transform the ray to local space.
    //Note that this sphere has a radius of 1.0 in that space.

    Ray newRay(Tr.GetMatToLocal().ApplyPoint(ray.GetPos()),
               Tr.GetMatToLocal().ApplyVector(ray.GetDir()).Normalize());

    float a = newRay.GetDir().Dot(newRay.GetDir()),
          b = 2.0f * newRay.GetDir().Dot(newRay.GetPos()),
          c = newRay.GetPos().Dot(newRay.GetPos()) - 1.0f;

    float discriminant = (b * b) - (4.0f * a * c);

    if (discriminant < 0.0f)
    {
        return false;
    }

    //Find the intersection distance.
    float outDist;
    if (discriminant == 0.0f)
    {
        outDist = -b / (2.0f * a);
    }
    else
    {
        float inv2a = 0.5f / a,
              temp = sqrtf(discriminant);
        float t1 = (-b - temp) * inv2a,
              t2 = (-b + temp) * inv2a;

        if (t2 < 0)
        {
            if (t1 < 0)
            {
                return false;
            }
            else
            {
                outDist = t1;
            }
        }
        else if (t1 < 0)
        {
            outDist = t2;
        }
        else
        {
            outDist = min(t1, t2);
        }

    }
    
    //Calculate other data.
    outHit.Pos = newRay.GetPos(outDist);
    FillInData(outHit, newRay.GetPos(outDist));

    return true;
}
void Sphere::FillInData(Vertex& v, const Vector3f& localPos) const
{
    v.Pos = Tr.GetMatToWorld().ApplyPoint(localPos);

    Vector3f localNormal = localPos.Normalize(); //TODO: Is normalization necessary?

    v.Normal = Tr.GetMatToWorld().ApplyVector(localNormal).Normalize();
    v.Tangent = v.Normal.Cross(fabs(v.Normal.x) == 1.0f ? Vector3f::Y() : Vector3f::X()).Normalize();
    v.Bitangent = v.Normal.Cross(v.Tangent);
    
    const float invPi = 1.0f / (float)M_PI,
                inv2Pi = 0.5f * invPi;
    v.UV[0] = 0.5f + (inv2Pi * atan2(localNormal.z, localNormal.x));
    v.UV[1] = 0.5f - (invPi * asin(localNormal.y));
}