#include "../Headers/BoundingBox.h"

using namespace RT;


namespace
{
    float max(float f1, float f2) { return (f1 > f2) ? f1 : f2; }
    float min(float f1, float f2) { return (f1 > f2) ? f2 : f1; }
}

bool BoundingBox::RayIntersects(const Ray& ray, float tMin, float tMax) const
{
    Vector3f dirFrac = ray.GetDir().Reciprocal();

    Vector3f minD = (Min - ray.GetPos()) * dirFrac,
             maxD = (Max - ray.GetPos()) * dirFrac;

    float _tMin = max(max(min(minD.x, maxD.x), min(minD.y, maxD.y)), min(minD.z, maxD.z)),
          _tMax = min(min(max(minD.x, maxD.x), max(minD.y, maxD.y)), max(minD.z, maxD.z));
    return (_tMin >= tMin && _tMin <= tMax) ||
           (_tMax >= tMin && _tMax <= tMax);
}