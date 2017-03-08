#include "../Headers/Material_Metal.h"

#include "../Headers/MaterialValueGraph.h"
using namespace RT;


ADD_MATERIAL_REFLECTION_DATA_CPP(Material_Metal);


bool Material_Metal::Scatter(const Ray& rIn, const Vertex& surf, const Shape& shpe,
                             FastRand& prng, Vector3f& atten, Vector3f& emission,
                             Ray& rOut) const
{
    atten = Albedo->GetValue(rIn, prng, &shpe, &surf);
    emission = Emissive->GetValue(rIn, prng, &shpe, &surf);

    //TODO: See if this "normalize" is necessary.
    Vector3f reflected = rIn.GetDir().Reflect(surf.Normal).Normalize();
    //Add randomness based on roughness.
    reflected += (prng.NextUnitVector3() * (float)Roughness->GetValue(rIn, prng, &shpe, &surf));

    //If the ray is pointing into the surface, count it as absorbed.
    if (reflected.Dot(surf.Normal) > 0.0f)
    {
        rOut = Ray(surf.Pos + (surf.Normal * PushoffDist),
                   reflected);
        return true;
    }
    else
    {
        return false;
    }
}
void Material_Metal::WriteData(DataWriter& writer) const
{
    Material::WriteData(writer);

    MaterialValueGraph graph(List<const MaterialValue*>(Albedo.Get(), Roughness.Get(), Emissive.Get()));
    writer.WriteDataStructure(graph, "Albedo_Roughness_Emissive");
}
void Material_Metal::ReadData(DataReader& reader)
{
    Material::ReadData(reader);

    MaterialValueGraph graph;
    reader.ReadDataStructure(graph, "Albedo_Roughness_Emissive");

    Albedo = graph.GetRootVals()[0];
    Roughness = graph.GetRootVals()[1];
    Emissive = graph.GetRootVals()[2];
}