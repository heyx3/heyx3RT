#include "../Headers/DataSerialization.h"

#include "../Headers/Vector3f.h"
#include "../Headers/Quaternion.h"


namespace
{
    class Vector3f_Writable : public IWritable
    {
    public:

        Vector3f V;

        Vector3f_Writable(const Vector3f& v) : V(v) { }

        virtual void WriteData(DataWriter& writer) const override
        {
            writer.WriteFloat(V.x, "x");
            writer.WriteFloat(V.y, "y");
            writer.WriteFloat(V.z, "z");
        }
    };
    class Vector3f_Readable : public IReadable
    {
    public:

        Vector3f& V;

        Vector3f_Readable(Vector3f& v) : V(v) { }

        virtual void ReadData(DataReader& reader) override
        {
            reader.ReadFloat(V.x, "x");
            reader.ReadFloat(V.y, "y");
            reader.ReadFloat(V.z, "z");
        }
    };

    class Quaternion_Writable : public IWritable
    {
    public:

        Quaternion Q;

        Quaternion_Writable(const Quaternion& q) : Q(q) { }

        virtual void WriteData(DataWriter& writer) const override
        {
            writer.WriteFloat(Q.x, "x");
            writer.WriteFloat(Q.y, "y");
            writer.WriteFloat(Q.z, "z");
            writer.WriteFloat(Q.w, "w");
        }
    };
    class Quaternion_Readable : public IReadable
    {
    public:

        Quaternion& Q;

        Quaternion_Readable(Quaternion& q) : Q(q) { }

        virtual void ReadData(DataReader& reader) override
        {
            reader.ReadFloat(Q.x, "x");
            reader.ReadFloat(Q.y, "y");
            reader.ReadFloat(Q.z, "z");
            reader.ReadFloat(Q.w, "w");
        }
    };
}

void DataWriter::WriteVec3f(const Vector3f& v, const char* name)
{
    WriteDataStructure(Vector3f_Writable(v), name);
}
void DataReader::ReadVec3f(Vector3f& v, const char* name)
{
    ReadDataStructure(Vector3f_Readable(v), name);
}

void DataWriter::WriteQuaternion(const Quaternion& q, const char* name)
{
    WriteDataStructure(Quaternion_Writable(q), name);
}
void DataReader::ReadQuaternion(Quaternion& q, const char* name)
{
    ReadDataStructure(Quaternion_Readable(q), name);
}