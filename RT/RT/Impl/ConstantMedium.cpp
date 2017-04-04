#include "../Headers/ConstantMedium.h"

#include "../Headers/Material.h"

using namespace RT;


ADD_SHAPE_REFLECTION_DATA_CPP(ConstantMedium);


void ConstantMedium::PrecalcData()
{
    Surface->PrecalcData();
}

bool ConstantMedium::CastRay(const Ray& ray, Vertex& outHit, FastRand& prng,
                             float tMin, float tMax) const
{
    BoundingBox surfaceBox;
    Surface->GetBoundingBox(surfaceBox);
    if (!surfaceBox.RayIntersects(ray, tMin, tMax))
        return false;

    constexpr float inf = std::numeric_limits<float>::infinity();

    //Get where the ray enters and exits the surface.
    Vertex enterHit, exitHit;
    if (Surface->CastRay(ray, enterHit, prng, -inf, inf))
    {
        float entranceT = ray.GetT(enterHit.Pos);

        if (Surface->CastRay(ray, exitHit, prng, entranceT + Material::PushoffDist, inf))
        {
            float exitT = ray.GetT(exitHit.Pos);

            //Clamp the entrance/exit positions along the ray.
            entranceT = (entranceT < tMin ? tMin : entranceT);
            exitT = (exitT > tMax ? tMax : exitT);
            if (entranceT >= exitT)
                return false;
            entranceT = (entranceT < 0.0f ? 0.0f : entranceT);

            //Get the distance through this medium before the ray hits a particle.
            float distThroughMedium = (exitT - entranceT);
            float hitDist = -log(prng.NextFloat()) / Density;

            if (hitDist < distThroughMedium)
            {
                float outHitDist = entranceT + hitDist;
                outHit.Pos = ray.GetPos(outHitDist);

                //The medium is constant, so the normal/tangent/bitangent is random.
                outHit.Normal = prng.NextUnitVector3();
                outHit.Normal.GetOrthoBasis(outHit.Tangent, outHit.Bitangent);
                //Make the UV random as well.
                outHit.UV = Vector2f(prng.NextFloat(), prng.NextFloat());

                return true;
            }
        }
    }

    return false;
}

void ConstantMedium::WriteData(DataWriter& writer) const
{
    Shape::WriteData(writer);

    writer.WriteFloat(Density, "Density");
    Shape::WriteValue(*Surface, writer, "Surface");
}
void ConstantMedium::ReadData(DataReader& reader)
{
    Shape::ReadData(reader);

    reader.ReadFloat(Density, "Density");

    Shape* outShape;
    Shape::ReadValue(outShape, reader, "Surface");
    Surface = outShape;
}