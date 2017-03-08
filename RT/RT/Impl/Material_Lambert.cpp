#include "../Headers/Material_Lambert.h"

#include "../Headers/MaterialValueGraph.h"
using namespace RT;


ADD_MATERIAL_REFLECTION_DATA_CPP(Material_Lambert);


bool Material_Lambert::Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                               FastRand& prng, Vector3f& attenuation, Vector3f& emission,
                               Ray& rOut) const
{
    attenuation = Albedo->GetValue(rIn, prng, &shpe, &surface);
    emission = Emissive->GetValue(rIn, prng, &shpe, &surface);

    Vector3f newPos = surface.Pos + (surface.Normal * PushoffDist);
    Vector3f targetPos = surface.Pos + surface.Normal + prng.NextUnitVector3();
    rOut = Ray(newPos, (targetPos - newPos).Normalize());

    return true;
}

void Material_Lambert::WriteData(DataWriter& writer) const
{
    Material::WriteData(writer);

    MaterialValueGraph graph(List<const MaterialValue*>(Albedo.Get(), Emissive.Get()));
    writer.WriteDataStructure(graph, "Albedo_Emissive");
}
void Material_Lambert::ReadData(DataReader& reader)
{
    Material::ReadData(reader);

    MaterialValueGraph graph;
    reader.ReadDataStructure(graph, "Albedo_Emissive");

    Albedo = graph.GetRootVals()[0];
    Emissive = graph.GetRootVals()[1];
}