#include "../Headers/SkyMaterial_SimpleColor.h"

#include "../Headers/MaterialValueGraph.h"
using namespace RT;


ADD_SKYMAT_REFLECTION_DATA_CPP(SkyMaterial_SimpleColor);


Vector3f SkyMaterial_SimpleColor::GetColor(const Ray& ray, FastRand& prng) const
{
    return Color->GetValue(ray, prng);
}

void SkyMaterial_SimpleColor::WriteData(DataWriter& writer) const
{
    SkyMaterial::WriteData(writer);

    MaterialValueGraph graph(List<const MaterialValue*>(Color.Get()));
    writer.WriteDataStructure(graph, "Color");
}
void SkyMaterial_SimpleColor::ReadData(DataReader& reader)
{
    SkyMaterial::ReadData(reader);

    MaterialValueGraph graph;
    reader.ReadDataStructure(graph, "Color");

    Color = graph.GetRootVals()[0];
}