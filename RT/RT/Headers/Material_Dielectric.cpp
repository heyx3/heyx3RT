#include "../Headers/Material_Dielectric.h"

#include "../Headers/MaterialValueGraph.h"
using namespace RT;


ADD_MATERIAL_REFLECTION_DATA_CPP(Material_Dielectric);


bool Material_Dielectric::Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                                  FastRand& prng, Vector3f& attenuation, Vector3f& emission,
                                  Ray& rOut) const
{
    float indexOfRefraction = (float)IndexOfRefraction->GetValue(rIn, prng, &shpe, &surface);

    float ratioOfIndices;
    Vector3f outwardNormal;
    float cosAngle;
    if (surface.Normal.Dot(rIn.GetDir()) > 0.0f)
    {
        ratioOfIndices = indexOfRefraction;
        outwardNormal = -surface.Normal;
        cosAngle = indexOfRefraction * rIn.GetDir().Dot(surface.Normal) / rIn.GetDir().Length();
    }
    else
    {
        ratioOfIndices = 1.0f / indexOfRefraction;
        outwardNormal = surface.Normal;
        cosAngle = -rIn.GetDir().Dot(surface.Normal) / rIn.GetDir().Length();
    }

    //Try refracting. Possibly reflect instead.
    float reflectionChance;
    Vector3f refracted;
    if (rIn.GetDir().Refract(outwardNormal, ratioOfIndices, refracted))
    {
        //Approximation of chance of refraction by Christophe Schlick:
        float r0 = (1.0f - indexOfRefraction) / (1.0f + indexOfRefraction);
        r0 *= r0;
        reflectionChance = r0 + ((1.0f - r0) * std::powf((1.0f - cosAngle), 5.0f));
    }
    else
    {
        reflectionChance = 1.01f;
    }

    if (prng.NextFloat() < reflectionChance)
        rOut = Ray(surface.Pos, rIn.GetDir().Reflect(surface.Normal));
    else
        rOut = Ray(surface.Pos, refracted);

    //Move the ray forward a tiny bit to get off the surface.
    rOut.SetPos(rOut.GetPos() + (rOut.GetDir() * PushoffDist));

    attenuation = Vector3f(1.0f, 1.0f, 1.0f);
    return true;
}

void Material_Dielectric::WriteData(DataWriter& writer) const
{
    Material::WriteData(writer);

    MaterialValueGraph graph(List<const MaterialValue*>(IndexOfRefraction.Get()));
    writer.WriteDataStructure(graph, "IndexOfRefraction");
}
void Material_Dielectric::ReadData(DataReader& reader)
{
    Material::ReadData(reader);

    MaterialValueGraph graph;
    reader.ReadDataStructure(graph, "IndexOfRefraction");

    IndexOfRefraction = graph.GetRootVals()[0];
}