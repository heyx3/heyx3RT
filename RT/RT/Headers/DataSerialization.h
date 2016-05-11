#pragma once

#include "Main.hpp"

#include <string.h>
#include <vector>


struct Vector2f;
struct Vector3f;
struct Vector4f;
struct Quaternion;


class DataReader;
class DataWriter;


//A data structure that can read its data from a DataReader.
struct RT_API IReadable
{
public:
    virtual ~IReadable(void) { }
    virtual void ReadData(DataReader& data) = 0;
};

//A data structure that can write its data to a DataWriter.
struct RT_API IWritable
{
public:
    virtual ~IWritable(void) { }
    virtual void WriteData(DataWriter& data) const = 0;
};

//A data structure that can both read and write its data.
struct RT_API ISerializable : public IReadable, IWritable { };



//Writes data to some kind of stream. Classes inherit from this class to provide specific behavior,
//     e.x. XML files or binary files.
//Names can be provided when writing data, but those names are only cosmetic and may be ignored.
//The order that data is written in must match the order it will be read out from!
class RT_API DataWriter
{
public:

    //The exception value that's thrown if some operation fails.
    static const int EXCEPTION_FAILURE = 666;

    //An error message that may be set when EXCEPTION_FAILURE is thrown.
    std::string ErrorMessage = "";


    DataWriter() { }
    virtual ~DataWriter(void) { }


    virtual void WriteBool(bool value, const std::string& name) = 0;
    virtual void WriteByte(unsigned char value, const std::string& name) = 0;
    virtual void WriteInt(int value, const std::string& name) = 0;
    virtual void WriteUInt(unsigned int value, const std::string& name) = 0;
    virtual void WriteFloat(float value, const std::string& name) = 0;
    virtual void WriteDouble(double value, const std::string& name) = 0;
    virtual void WriteString(const std::string& value, const std::string& name) = 0;
    virtual void WriteBytes(const unsigned char* bytes, size_t nBytes, const std::string& name) = 0;

    virtual void WriteVec2f(const Vector2f& v, const std::string& name);
    virtual void WriteVec3f(const Vector3f& v, const std::string& name);
    virtual void WriteVec4f(const Vector4f& v, const std::string& name);
    virtual void WriteQuaternion(const Quaternion& q, const std::string& name);


    //Writes a data structure that implements the IWritable interface.
    virtual void WriteDataStructure(const IWritable& toSerialize, const std::string& name) = 0;


    template<typename T>
    //Writes a given element into the given DataWriter with the given name.
    using ElementWriter = void(*)(DataWriter& writer, const T& t, const std::string& name);

    template<typename T>
    void WriteList(const T* listData, size_t nValues,
                   ElementWriter<T> elementWriter,
                   const std::string& name)
    {
        CollectionWrite<T> helper(listData, nValues, elementWriter);
        WriteDataStructure(helper, name);
    }

private:

    #pragma region Helper data structure for "WriteList()"

    template<typename T>
    struct CollectionWrite : public IWritable
    {
        const T* Values;
        size_t NValues;
        ElementWriter<T> Writer;

        CollectionWrite(const T* values, size_t nValues, ElementWriter<T> writer)
            : Values(values), NValues(nValues), Writer(writer) { }

        virtual void WriteData(DataWriter& writer) const override
        {
            writer.WriteUInt(NValues, "NValues");
            for (size_t i = 0; i < NValues; ++i)
            {
                Writer(writer, Values[i], std::to_string(i + 1));
            }
        }
    };

    #pragma endregion


    DataWriter(const DataWriter& cpy) = delete;
    DataWriter& operator=(const DataWriter& cpy) = delete;
};



//Reads data from some kind of stream. Classes inherit from this class to provide specific behavior,
//    e.x. XML files or binary files.
//The order that data is read out must match the order it was written in!
class RT_API DataReader
{
public:

    //The exception value that's thrown if some operation fails.
    static const int EXCEPTION_FAILURE = 667;

    //An error message that may be set when EXCEPTION_FAILURE is thrown.
    std::string ErrorMessage = "";


    DataReader() { }
    virtual ~DataReader(void) { }


    virtual void ReadBool(bool& outB, const std::string& name) = 0;
    virtual void ReadByte(unsigned char& outB, const std::string& name) = 0;
    virtual void ReadInt(int& outI, const std::string& name) = 0;
    virtual void ReadUInt(unsigned int& outU, const std::string& name) = 0;
    virtual void ReadFloat(float& outF, const std::string& name) = 0;
    virtual void ReadDouble(double& outD, const std::string& name) = 0;
    virtual void ReadString(std::string& outStr, const std::string& name) = 0;
    virtual void ReadBytes(std::vector<unsigned char>& outBytes, const std::string& name) = 0;

    virtual void ReadVec2f(Vector2f& v, const std::string& name);
    virtual void ReadVec3f(Vector3f& v, const std::string& name);
    virtual void ReadVec4f(Vector4f& v, const std::string& name);
    virtual void ReadQuaternion(Quaternion& q, const std::string& name);

    //Reads a data structure that implements the IReadable interface.
    virtual void ReadDataStructure(IReadable& outData, const std::string& name) = 0;


    //A function that resizes a collection to store at least the given number of elements.
    typedef void(*ListResizer)(void* pList, size_t nElements);

    template<typename T>
    //Reads a given element from the given DataReader with the given name.
    using ElementReader = void(*)(DataReader& reader, void* pList, size_t listIndex, const std::string& name);

    template<typename T>
    void ReadList(void* list,
                  ListResizer listResizer,
                  ElementReader<T> elementReader,
                  const std::string& name)
    {
        CollectionRead<T> helper(list, listResizer, elementReader);
        ReadDataStructure(helper, name);
    }


private:
    
    #pragma region Helper data structure for "ReadList()"

    template<typename T>
    struct CollectionRead : public IReadable
    {
        void* List;
        ListResizer Resizer;
        ElementReader<T> Reader;

        CollectionRead(void* list,
                       ListResizer resizer,
                       ElementReader<T> reader)
            : List(list), Resizer(resizer), Reader(reader) { }

        virtual void ReadData(DataReader& reader) override
        {
            size_t nValues;
            reader.ReadUInt(nValues, "NValues");
            Resizer(List, nValues);

            for (size_t i = 0; i < nValues; ++i)
            {
                Reader(reader, List, i, std::to_string(i + 1));
            }
        }
    };

    #pragma endregion


    DataReader(const DataReader& cpy) = delete;
    DataReader& operator=(const DataReader& cpy) = delete;
};