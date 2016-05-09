#include "../Headers/DataSerialization.h"

#include "../Headers/Quaternion.h"


namespace
{
#define COMMA(thing1, thing2) thing1, thing2

#define MAKE_SERIALIZER(type, writeCode, readCode) \
    class type##_Writable : public IWritable \
    { \
    public: \
        const type& Value; \
        type##_Writable(const type& value) : Value(value) { } \
        virtual void WriteData(DataWriter& writer) const override { writeCode } \
    }; \
    class type##_Readable : public IReadable \
    { \
    public: \
        type& Value; \
        type##_Readable(type& value) : Value(value) { } \
        virtual void ReadData(DataReader& reader) override { readCode } \
    }
    
    MAKE_SERIALIZER(Vector2f,
                        writer.WriteFloat(COMMA(Value.x, "x"));
                        writer.WriteFloat(COMMA(Value.y, "y")); ,
                        
                        reader.ReadFloat(COMMA(Value.x, "x"));
                        reader.ReadFloat(COMMA(Value.y, "y"));
                    );
    MAKE_SERIALIZER(Vector3f,
                        writer.WriteFloat(COMMA(Value.x, "x"));
                        writer.WriteFloat(COMMA(Value.y, "y"));
                        writer.WriteFloat(COMMA(Value.z, "z")); ,
                        
                        reader.ReadFloat(COMMA(Value.x, "x"));
                        reader.ReadFloat(COMMA(Value.y, "y"));
                        reader.ReadFloat(COMMA(Value.z, "z"));
                    );
    MAKE_SERIALIZER(Vector4f,
                        writer.WriteFloat(COMMA(Value.x, "x"));
                        writer.WriteFloat(COMMA(Value.y, "y"));
                        writer.WriteFloat(COMMA(Value.z, "z"));
                        writer.WriteFloat(COMMA(Value.w, "w")); ,
                        
                        reader.ReadFloat(COMMA(Value.x, "x"));
                        reader.ReadFloat(COMMA(Value.y, "y"));
                        reader.ReadFloat(COMMA(Value.z, "z"));
                        reader.ReadFloat(COMMA(Value.w, "w"));
                    );
    MAKE_SERIALIZER(Quaternion,
                        writer.WriteFloat(COMMA(Value.x, "x"));
                        writer.WriteFloat(COMMA(Value.y, "y"));
                        writer.WriteFloat(COMMA(Value.z, "z"));
                        writer.WriteFloat(COMMA(Value.w, "w")); ,
                        
                        reader.ReadFloat(COMMA(Value.x, "x"));
                        reader.ReadFloat(COMMA(Value.y, "y"));
                        reader.ReadFloat(COMMA(Value.z, "z"));
                        reader.ReadFloat(COMMA(Value.w, "w"));
                    );
#undef COMMA
#undef MAKE_SERIALIZER
}


void DataWriter::WriteVec2f(const Vector2f& v, const std::string& name)
{
    WriteDataStructure(Vector2f_Writable(v), name);
}
void DataWriter::WriteVec3f(const Vector3f& v, const std::string& name)
{
    WriteDataStructure(Vector3f_Writable(v), name);
}
void DataWriter::WriteVec4f(const Vector4f& v, const std::string& name)
{
    WriteDataStructure(Vector4f_Writable(v), name);
}
void DataWriter::WriteQuaternion(const Quaternion& q, const std::string& name)
{
    WriteDataStructure(Quaternion_Writable(q), name);
}

void DataReader::ReadVec2f(Vector2f& v, const std::string& name)
{
    ReadDataStructure(Vector2f_Readable(v), name);
}
void DataReader::ReadVec3f(Vector3f& v, const std::string& name)
{
    ReadDataStructure(Vector3f_Readable(v), name);
}
void DataReader::ReadVec4f(Vector4f& v, const std::string& name)
{
    ReadDataStructure(Vector4f_Readable(v), name);
}
void DataReader::ReadQuaternion(Quaternion& q, const std::string& name)
{
    ReadDataStructure(Quaternion_Readable(q), name);
}