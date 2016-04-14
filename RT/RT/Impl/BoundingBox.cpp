#include "../Headers/BoundingBox.h"


bool BoundingBox::RayIntersects(const Ray& ray) const
{
    Vector3f dirFrac = ray.GetDir().Reciprocal();

    Vector3f minD = (Min - ray.GetPos()) * dirFrac,
             maxD = (Max - ray.GetPos()) * dirFrac;

    float tMin = max(max(min(minD.x, maxD.x), min(minD.y, maxD.y)), min(minD.z, maxD.z)),
          tMax = min(min(max(minD.x, maxD.x), max(minD.y, maxD.y)), max(minD.z, maxD.z));
    return (tMax < 0.0f || tMin > tMax);
}