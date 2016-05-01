#pragma once

#include "Vector3f.h"
#include "DataSerialization.h"


//A single point in a mesh triangle.
struct RT_API Vertex : public ISerializable
{
public:

    Vector3f Pos, Normal, Tangent, Bitangent;
    float UV[2];


    Vertex() { }
    Vertex(const Vector3f& pos, const Vector3f& norm,
           const Vector3f& tangent, const Vector3f& bitangent, float uvs[2])
        : Pos(pos), Normal(norm), Tangent(tangent), Bitangent(bitangent), UV()
    {
        UV[0] = uvs[0];
        UV[1] = uvs[1];
    }


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;
};