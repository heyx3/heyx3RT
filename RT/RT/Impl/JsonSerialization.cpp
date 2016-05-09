#include "../Headers/JsonSerialization.h"

#include "../Headers/ThirdParty/base64.h"
#include <fstream>


#pragma warning( disable : 4996 )


bool RT_API JsonSerialization::ToJSONFile(const std::string& filePath, const IWritable& toWrite,
                                          bool compact, std::string& outErrorMsg)
{
    JsonWriter writer;
    std::string trying = "UNKNOWN";
    try
    {
        trying = "serializing data";
        writer.WriteDataStructure(toWrite, "data");
        trying = "writing data to file";
        outErrorMsg = writer.SaveData(filePath, compact);
        if (!outErrorMsg.empty())
        {
            return false;
        }

        return true;
    }
    catch (int i)
    {
        if (i == DataWriter::EXCEPTION_FAILURE)
        {
            outErrorMsg = std::string("Error while ") + trying + ": " + writer.ErrorMessage;
        }
        else
        {
            outErrorMsg = "Unknown error code while " + trying + ": " + std::to_string(i);
        }
        return false;
    }
}
bool RT_API JsonSerialization::ToJSONString(const IWritable& toWrite, bool compact,
                                            std::string& outJSON, std::string& outErrorMsg)
{
    JsonWriter writer;
    std::string trying = "UNKNOWN";
    try
    {
        trying = "serializing data";
        writer.WriteDataStructure(toWrite, "data");
        outJSON = writer.GetData(compact);
        return true;
    }
    catch (int i)
    {
        if (i == DataWriter::EXCEPTION_FAILURE)
        {
            outErrorMsg = std::string("Error while ") + trying + ": " + writer.ErrorMessage;
        }
        else
        {
            outErrorMsg = "Unknown error code while " + trying + ": " + std::to_string(i);
        }
        return false;
    }
}
bool RT_API JsonSerialization::FromJSONFile(const std::string& filePath, IReadable& toRead,
                                            std::string& outErrorMsg)
{
    JsonReader reader(filePath);

    //If we had an error reading the file, stop.
    if (reader.ErrorMessage.size() > 0)
    {
        outErrorMsg = std::string("Error reading file: ") + reader.ErrorMessage;
        return false;
    }

    try
    {
        reader.ReadDataStructure(toRead, "data");
        return true;
    }
    catch (int i)
    {
        if (i == DataReader::EXCEPTION_FAILURE)
        {
            outErrorMsg = std::string("Error reading data: ") + reader.ErrorMessage;
        }
        else
        {
            outErrorMsg = "Unknown error code: " + std::to_string(i);
        }
        return false;
    }
}


std::string JsonWriter::SaveData(const std::string& path, bool compact)
{
    std::ofstream file(path, std::ios_base::trunc);
    if (file.is_open())
    {
        file << doc.dump(compact ? -1 : 0);
    }
    else
    {
        return "Couldn't open file";
    }

    return "";
}
void JsonWriter::WriteBool(bool value, const std::string& name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteByte(unsigned char value, const std::string& name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteInt(int value, const std::string& name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteUInt(unsigned int value, const std::string& name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteFloat(float value, const std::string& name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteDouble(double value, const std::string& name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteString(const std::string& value, const std::string& name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteBytes(const unsigned char* bytes, size_t nBytes, const std::string& name)
{
    std::string str = base64::encode(bytes, nBytes);
    WriteString(str, name);
}
void JsonWriter::WriteDataStructure(const IWritable& toSerialize, const std::string& name)
{
    GetToUse()[name] = nlohmann::json::object();
    JsonWriter subWriter(&GetToUse()[name]);
    toSerialize.WriteData(subWriter);
}


JsonReader::JsonReader(const std::string& filePath)
{
    ErrorMessage = Reload(filePath);
}

std::string JsonReader::Reload(const std::string& filePath)
{
    subDoc = nullptr;

    std::ifstream fileS(filePath);
    if (!fileS.is_open())
    {
        return "Couldn't open the file";
    }

    fileS.seekg(0, std::ios::end);
    std::streampos size = fileS.tellg();
    fileS.seekg(0, std::ios::beg);

    std::vector<char> fileData;
    fileData.resize((size_t)size);
    fileS.read(fileData.data(), size);

    doc = nlohmann::json::parse(std::string(fileData.data()));

    return "";
}

void JsonReader::Assert(bool expr, const std::string& errorMsg)
{
    if (!expr)
    {
        ErrorMessage = errorMsg;
        throw EXCEPTION_FAILURE;
    }
}
nlohmann::json::const_iterator JsonReader::GetItem(const std::string& name)
{
    const nlohmann::json& jsn = GetToUse();
    auto element = jsn.find(name);
    Assert(element != jsn.end(),
           "Couldn't find the element.");
    return element;
}

void JsonReader::ReadBool(bool& outB, const std::string& name)
{
    auto& element = GetItem(name);
    Assert(element->is_boolean(), "Expected a boolean but got something else");
    outB = element->get<bool>();
}
void JsonReader::ReadByte(unsigned char& outB, const std::string& name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_unsigned(), "Expected a byte but got something else");
    outB = (unsigned char)element->get<unsigned int>();
}
void JsonReader::ReadInt(int& outI, const std::string& name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_integer(), "Expected an integer but got something else");
    outI = element->get<int>();
}
void JsonReader::ReadUInt(unsigned int& outU, const std::string& name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_unsigned(), "Expected an unsigned integer but got something else");
    outU = element->get<unsigned int>();
}
void JsonReader::ReadFloat(float& outF, const std::string& name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_float(), "Expected a float but got something else");
    outF = element->get<float>();
}
void JsonReader::ReadDouble(double& outD, const std::string& name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_float(), "Expected a double but got something else");
    outD = element->get<double>();
}
void JsonReader::ReadString(std::string& outStr, const std::string& name)
{
    auto& element = GetItem(name);
    Assert(element->is_string(), "Expected a string but got something else");

    outStr = element->get<std::string>();
}
void JsonReader::ReadBytes(std::vector<unsigned char>& outBytes, const std::string& name)
{
    std::string str;
    ReadString(str, name);

    base64::decode(str, outBytes);
}
void JsonReader::ReadDataStructure(IReadable& outData, const std::string& name)
{
    auto& element = GetItem(name);
    Assert(element->is_object(), "Expected a data structure but got something else");
    
    JsonReader subReader(&(*element));
    outData.ReadData(subReader);
}


#pragma warning( default : 4996 )