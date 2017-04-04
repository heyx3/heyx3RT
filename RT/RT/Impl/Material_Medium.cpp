#include "../Headers/Material_Medium.h"

#include "../Headers/MaterialValueGraph.h"
using namespace RT;


ADD_MATERIAL_REFLECTION_DATA_CPP(Material_Medium);


bool Material_Medium::Scatter(const Ray& rIn, const Vertex& surface,
                              const Shape& shpe, FastRand& prng,
                              Vector3f& attenuation, Vector3f& emission,
                              Ray& rOut) const
{
    attenuation = Albedo->GetValue(rIn, prng, &shpe, &surface);
    rOut = Ray(rIn.GetPos(), prng.NextUnitVector3());
    return true;
}

void Material_Medium::WriteData(DataWriter& writer) const
{
    Material::WriteData(writer);

    MaterialValueGraph graph(List<const MaterialValue*>(Albedo.Get()));
    writer.WriteDataStructure(graph, "Albedo");
}
void Material_Medium::ReadData(DataReader& reader)
{
    Material::ReadData(reader);

    MaterialValueGraph graph;
    reader.ReadDataStructure(graph, "Albedo");

    Albedo = graph.GetRootVals()[0];
}