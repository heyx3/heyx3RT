#pragma once


#include "DataSerialization.h"
#include "ThirdParty\json.hpp"


namespace JsonSerialization
{
    //Writes the given item to a JSON file, overwriting it if it already exists.
    //"compact" determines whether the file should forego nice formatting to save space.
    //If something went wrong, outputs an error message and returns false.
    //Otherwise, returns true.
    bool RT_API ToJSONFile(const std::string& filePath, const IWritable& toWrite,
                           bool compact, std::string& outErrorMsg);
    //Writes the given item to a JSON string, then outputs that string into "outString" using the given function to do it.
    //"compact" determines whether the file should forego nice formatting to save space.
    //If something went wrong, outputs an error message and returns false.
    //Otherwise, returns true.
    bool RT_API ToJSONString(const IWritable& toWrite, bool compact,
                             std::string& outJSON, std::string& outErrorMsg);
    //Reads the given item from a JSON file.
    //Returns an error message, or the empty string if everything went fine.
    //Otherwise, returns true.
    bool RT_API FromJSONFile(const std::string& filePath, IReadable& toRead, std::string& outErrorMsg);
}


class RT_API JsonWriter : public DataWriter
{
public:

    JsonWriter() { ClearData(); }


    //Saves all written data out to a file at the given path.
    //Returns an error message, or the empty string if the data was saved successfully.
    //"compact" determines whether the file should forego nice formatting to save space.
    std::string SaveData(const std::string& path, bool compact);
    //Gets the written data as a JSON string.
    //"compact" determines whether the file should forego nice formatting to save space.
    std::string GetData(bool compact) const { return GetToUse().dump((compact ? -1 : 0)); }

    //Removes all data from the JSON document.
    void ClearData() { doc = nlohmann::json::object(); }


    virtual void WriteBool(bool value, const std::string& name) override;
    virtual void WriteByte(unsigned char value, const std::string& name) override;
    virtual void WriteInt(int value, const std::string& name) override;
    virtual void WriteUInt(unsigned int value, const std::string& name) override;
    virtual void WriteFloat(float value, const std::string& name) override;
    virtual void WriteDouble(double value, const std::string& name) override;
    virtual void WriteString(const std::string& value, const std::string& name) override;
    virtual void WriteBytes(const unsigned char* bytes, size_t nBytes, const std::string& name) override;
    virtual void WriteDataStructure(const IWritable& toSerialize, const std::string& name) override;


private:

    nlohmann::json doc;
    nlohmann::json currentElement;


    //The below are used to write a sub-object in a JSON document.
    JsonWriter(nlohmann::json* _childDoc) : childDoc(_childDoc) { }
    nlohmann::json* childDoc = nullptr;

    nlohmann::json& GetToUse() { return (childDoc == nullptr ? doc : *childDoc); }
    const nlohmann::json& GetToUse() const { return (childDoc == nullptr ? doc : *childDoc); }
};


class RT_API JsonReader : public DataReader
{
public:

    //If there was an error reading to the given file,
    //    an error message is written to this instance's "ErrorMessage" field.
    //It will NOT throw an exception.
    JsonReader(const std::string& filePath);


    //Loads in new JSON data, resetting this reader.
    //Returns an error message, or the empty string if the file was loaded successfully.
    std::string Reload(const std::string& filePath);


    virtual void ReadBool(bool& outB, const std::string& name) override;
    virtual void ReadByte(unsigned char& outB, const std::string& name) override;
    virtual void ReadInt(int& outI, const std::string& name) override;
    virtual void ReadUInt(unsigned int& outU, const std::string& name) override;
    virtual void ReadFloat(float& outF, const std::string& name) override;
    virtual void ReadDouble(double& outD, const std::string& name) override;
    virtual void ReadString(std::string& outStr, const std::string& name) override;
    virtual void ReadBytes(std::vector<unsigned char>& outBytes, const std::string& name) override;
    virtual void ReadDataStructure(IReadable& outData, const std::string& name) override;


private:

    nlohmann::json doc;

    const nlohmann::json* subDoc = nullptr;


    JsonReader(const nlohmann::json* _subDoc) : subDoc(_subDoc) { }


    const nlohmann::json& GetToUse() const { return (subDoc == nullptr ? doc : *subDoc); }

    nlohmann::json::const_iterator GetItem(const std::string& name);

    void Assert(bool expr, const std::string& errorMsg);
};