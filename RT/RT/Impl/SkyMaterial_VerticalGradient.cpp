#include "../Headers/SkyMaterial_VerticalGradient.h"

#include "../Headers/MaterialValueGraph.h"
#include "../Headers/Mathf.h"

using namespace RT;


ADD_SKYMAT_REFLECTION_DATA_CPP(SkyMaterial_VerticalGradient);


Vector3f SkyMaterial_VerticalGradient::GetColor(const Ray& ray, FastRand& prng) const
{
    Vector3f dir = SkyDir->GetValue(ray, prng);
    float dirValue = ray.GetDir().Dot(dir.Normalize());

    return Vector3f::Lerp(BottomCol->GetValue(ray, prng),
                          TopCol->GetValue(ray, prng),
                          0.5f + (0.5f * dirValue));
}
void SkyMaterial_VerticalGradient::WriteData(DataWriter& writer) const
{
    SkyMaterial::WriteData(writer);

    MaterialValueGraph graph(List<const MaterialValue*>(BottomCol.Get(), TopCol.Get(), SkyDir.Get()));
    writer.WriteDataStructure(graph, "BottomCol_TopCol_SkyDir");
}
void SkyMaterial_VerticalGradient::ReadData(DataReader& reader)
{
    SkyMaterial::ReadData(reader);

    MaterialValueGraph graph;
    reader.ReadDataStructure(graph, "BottomCol_TopCol_SkyDir");

    BottomCol = graph.GetRootVals()[0];
    TopCol = graph.GetRootVals()[1];
    SkyDir = graph.GetRootVals()[2];
}