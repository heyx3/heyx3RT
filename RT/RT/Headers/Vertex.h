#pragma once

#include "DataSerialization.h"
#include "Vectors.h"


namespace RT
{
    //A single point in a mesh triangle.
    struct RT_API Vertex
    {
    public:

        Vector3f Pos, Normal, Tangent, Bitangent;
        Vector2f UV;


        Vertex() { }
        Vertex(const Vector3f& pos, const Vector3f& norm,
               const Vector3f& tangent, const Vector3f& bitangent, Vector2f uv)
            : Pos(pos), Normal(norm), Tangent(tangent), Bitangent(bitangent), UV(uv) { }
    };


    //Allows serialization of a vertex.
    //The vertex class itself doesn't inherit from ISerializable
    //    in an attempt to reduce the size of the data structure.
    struct RT_API Vertex_Writable : public IWritable
    {
    public:
        const Vertex& V;
        Vertex_Writable(const Vertex& v) : V(v) { }
        virtual void WriteData(DataWriter& writer) const override
        {
            writer.WriteVec3f(V.Pos, "Pos");
            writer.WriteVec3f(V.Normal, "Normal");
            writer.WriteVec3f(V.Tangent, "Tangent");
            writer.WriteVec3f(V.Bitangent, "Bitangent");
            writer.WriteVec2f(V.UV, "UV");
        }
    };
    //Allows deserialization of a vertex.
    //The vertex class itself doesn't inherit from ISerializable
    //    in an attempt to reduce the size of the data structure.
    struct RT_API Vertex_Readable : public IReadable
    {
    public:
        Vertex& V;
        Vertex_Readable(Vertex& v) : V(v) { }
        virtual void ReadData(DataReader& reader) override
        {
            reader.ReadVec3f(V.Pos, "Pos");
            reader.ReadVec3f(V.Normal, "Normal");
            reader.ReadVec3f(V.Tangent, "Tangent");
            reader.ReadVec3f(V.Bitangent, "Bitangent");
            reader.ReadVec2f(V.UV, "UV");
        }
    };
}