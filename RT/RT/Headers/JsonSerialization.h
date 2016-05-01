#pragma once


#include "DataSerialization.h"
#include "ThirdParty\json.hpp"


namespace JsonSerialization
{
    //Writes the given item to a JSON file, overwriting it if it already exists.
    //"compact" determines whether the file should forego nice formatting to save space.
    //If something went wrong, outputs an error message and returns false.
    //Otherwise, returns true.
    bool RT_API ToJSONFile(const char* filePath, const IWritable& toWrite,
                           bool compact, char* outErrorMsg, size_t maxErrMsgSize);
    //Writes the given item to a JSON string, then outputs that string into "outString" using the given function to do it.
    //"compact" determines whether the file should forego nice formatting to save space.
    //If something went wrong, outputs an error message and returns false.
    //Otherwise, returns true.
    bool RT_API ToJSONString(const IWritable& toWrite, bool compact,
                             void* outString,
                             void(*setOutString)(void* outStr, const char* str, size_t strLen),
                             char* outErrorMsg, size_t maxErrMsgSize);
    //Reads the given item from a JSON file.
    //Returns an error message, or the empty string if everything went fine.
    //Otherwise, returns true.
    bool RT_API FromJSONFile(const char* filePath, IReadable& toRead,
                             char* outErrorMsg, size_t maxErrMsgSize);
}


class JsonWriter : public DataWriter
{
public:

    JsonWriter() { ClearData(); }


    //Saves all written data out to a file at the given path.
    //Returns an error message, or the empty string if the data was saved successfully.
    //"compact" determines whether the file should forego nice formatting to save space.
    std::string SaveData(const char* path, bool compact);
    //Gets the written data as a JSON string.
    //"compact" determines whether the file should forego nice formatting to save space.
    std::string GetData(bool compact) const { return GetToUse().dump((compact ? -1 : 0)); }

    //Removes all data from the JSON document.
    void ClearData() { doc = nlohmann::json::object(); }


    virtual void WriteBool(bool value, const char* name) override;
    virtual void WriteByte(unsigned char value, const char* name) override;
    virtual void WriteInt(int value, const char* name) override;
    virtual void WriteUInt(unsigned int value, const char* name) override;
    virtual void WriteFloat(float value, const char* name) override;
    virtual void WriteDouble(double value, const char* name) override;
    virtual void WriteString(const char* value, const char* name) override;
    virtual void WriteBytes(const unsigned char* bytes, size_t nBytes, const char* name) override;
    virtual void WriteDataStructure(const IWritable& toSerialize, const char* name) override;
    virtual void WriteCollection(ElementWriter writerFunc, const char* name,
                                 size_t bytesPerElement,
                                 const void* collection, size_t collectionSize,
                                 void* userData = 0) override;

private:

    nlohmann::json doc;
    nlohmann::json currentElement;


    //The below are used to write a sub-object in a JSON document.
    JsonWriter(nlohmann::json* _childDoc) : childDoc(_childDoc) { }
    nlohmann::json* childDoc = nullptr;

    nlohmann::json& GetToUse() { return (childDoc == nullptr ? doc : *childDoc); }
    const nlohmann::json& GetToUse() const { return (childDoc == nullptr ? doc : *childDoc); }
};


class JsonReader : public DataReader
{
public:

    //If there was an error reading to the given file,
    //    an error message is written to this instance's "ErrorMessage" field.
    //It will NOT throw an exception.
    JsonReader(const char* filePath);


    //Loads in new JSON data, resetting this reader.
    //Returns an error message, or the empty string if the file was loaded successfully.
    std::string Reload(const char* filePath);


    virtual void ReadBool(bool& outB, const char* name) override;
    virtual void ReadByte(unsigned char& outB, const char* name) override;
    virtual void ReadInt(int& outI, const char* name) override;
    virtual void ReadUInt(unsigned int& outU, const char* name) override;
    virtual void ReadFloat(float& outF, const char* name) override;
    virtual void ReadDouble(double& outD, const char* name) override;
    virtual void ReadString(char* outStr, size_t maxStrSize, const char* name) override;
            void ReadString(std::string& outStr, const char* name);
    virtual void ReadBytes(std::vector<unsigned char>& outBytes, const char* name) override;
    virtual void ReadDataStructure(IReadable& outData, const char* name) override;
    virtual void ReadCollection(ElementReader readerFunc, CollectionResizer resizer,
                                const char* name,
                                void* pCollection, void* userData = 0) override;

private:

    nlohmann::json doc;

    const nlohmann::json* subDoc = nullptr;


    JsonReader(const nlohmann::json* _subDoc) : subDoc(_subDoc) { }


    const nlohmann::json& GetToUse() const { return (subDoc == nullptr ? doc : *subDoc); }

    nlohmann::json::const_iterator GetItem(const char* name);

    void Assert(bool expr, const std::string& errorMsg);
};