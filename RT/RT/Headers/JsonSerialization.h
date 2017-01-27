#pragma once


#include "DataSerialization.h"
#include "ThirdParty\json.hpp"


namespace RT
{
    namespace JsonSerialization
    {
        //Writes the given item to a JSON file, overwriting it if it already exists.
        //"compact" determines whether the file should forego nice formatting to save space.
        //If something went wrong, outputs an error message and returns false.
        //Otherwise, returns true.
        bool RT_API ToJSONFile(const String& filePath, const IWritable& toWrite,
                               bool compact, String& outErrorMsg);
        //Writes the given item to a JSON string, then outputs that string into "outString"
        //    using the given function to do it.
        //"compact" determines whether the file should forego nice formatting to save space.
        //If something went wrong, outputs an error message and returns false.
        //Otherwise, returns true.
        bool RT_API ToJSONString(const IWritable& toWrite, bool compact,
                                 String& outJSON, String& outErrorMsg);
        //Reads the given item from a JSON file.
        //Returns an error message, or the empty string if everything went fine.
        //Otherwise, returns true.
        bool RT_API FromJSONFile(const String& filePath, IReadable& toRead, String& outErrorMsg);
    }

    #pragma warning(disable: 4251)

    class RT_API JsonWriter : public DataWriter
    {
    public:

        JsonWriter() { ClearData(); }


        //Saves all written data out to a file at the given path.
        //Returns an error message, or the empty string if the data was saved successfully.
        //"compact" determines whether the file should forego nice formatting to save space.
        String SaveData(const String& path, bool compact);
        //Gets the written data as a JSON string.
        //"compact" determines whether the file should forego nice formatting to save space.
        String GetData(bool compact) const { return GetToUse().dump((compact ? -1 : 0)).c_str(); }

        //Removes all data from the JSON document.
        void ClearData() { doc = nlohmann::json::object(); }


        virtual void WriteBool(bool value, const String& name) override;
        virtual void WriteByte(unsigned char value, const String& name) override;
        virtual void WriteInt(int value, const String& name) override;
        virtual void WriteUInt(size_t value, const String& name) override;
        virtual void WriteFloat(float value, const String& name) override;
        virtual void WriteDouble(double value, const String& name) override;
        virtual void WriteString(const String& value, const String& name) override;
        virtual void WriteBytes(const unsigned char* bytes, size_t nBytes, const String& name) override;
        virtual void WriteDataStructure(const IWritable& toSerialize, const String& name) override;


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
        JsonReader(const String& filePath);


        //Loads in new JSON data, resetting this reader.
        //Returns an error message, or the empty string if the file was loaded successfully.
        String Reload(const String& filePath);


        virtual void ReadBool(bool& outB, const String& name) override;
        virtual void ReadByte(unsigned char& outB, const String& name) override;
        virtual void ReadInt(int& outI, const String& name) override;
        virtual void ReadUInt(size_t& outU, const String& name) override;
        virtual void ReadFloat(float& outF, const String& name) override;
        virtual void ReadDouble(double& outD, const String& name) override;
        virtual void ReadString(String& outStr, const String& name) override;
        virtual void ReadBytes(List<unsigned char>& outBytes, const String& name) override;
        virtual void ReadDataStructure(IReadable& outData, const String& name) override;


    private:

        nlohmann::json doc;

        const nlohmann::json* subDoc = nullptr;


        JsonReader(const nlohmann::json* _subDoc) : subDoc(_subDoc) { }


        const nlohmann::json& GetToUse() const { return (subDoc == nullptr ? doc : *subDoc); }
        nlohmann::json::const_iterator GetItem(const String& name);
        void Assert(bool expr, const String& errorMsg);
    };
}

#pragma warning(default: 4251)