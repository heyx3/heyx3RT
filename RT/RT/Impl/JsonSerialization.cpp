#include "../Headers/JsonSerialization.h"

#include "../Headers/ThirdParty/base64.h"
#include <fstream>

using namespace RT;


#pragma warning( disable : 4996 )


bool RT_API JsonSerialization::ToJSONFile(const String& filePath, const IWritable& toWrite,
                                          bool compact, String& outErrorMsg)
{
    JsonWriter writer;
    String trying = "UNKNOWN";
    try
    {
        trying = "serializing data";
        writer.WriteDataStructure(toWrite, "data");
        trying = "writing data to file";
        outErrorMsg = writer.SaveData(filePath, compact);
        if (!outErrorMsg.IsEmpty())
        {
            return false;
        }

        return true;
    }
    catch (int i)
    {
        if (i == DataWriter::EXCEPTION_FAILURE)
        {
            outErrorMsg = String("Error while ") + trying + ": " + writer.ErrorMessage;
        }
        else
        {
            outErrorMsg = String("Unknown error code while ") + trying + ": " + String(i);
        }
        return false;
    }
}
bool RT_API JsonSerialization::ToJSONString(const IWritable& toWrite, bool compact,
                                            String& outJSON, String& outErrorMsg)
{
    JsonWriter writer;
    String trying = "UNKNOWN";
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
            outErrorMsg = String("Error while ") + trying + ": " + writer.ErrorMessage;
        }
        else
        {
            outErrorMsg = String("Unknown error code while ") + trying + ": " + String(i);
        }
        return false;
    }
}
bool RT_API JsonSerialization::FromJSONFile(const String& filePath, IReadable& toRead,
                                            const String& rootObjectName, String& outErrorMsg)
{
    JsonReader reader(filePath);

    //If we had an error reading the file, stop.
    if (reader.ErrorMessage.GetSize() > 0)
    {
        outErrorMsg = String("Error reading file: ") + reader.ErrorMessage;
        return false;
    }

    try
    {
        reader.ReadDataStructure(toRead, rootObjectName);
        return true;
    }
    catch (int i)
    {
        if (i == DataReader::EXCEPTION_FAILURE)
        {
            outErrorMsg = String("Error reading data: ") + reader.ErrorMessage;
        }
        else
        {
            outErrorMsg = String("Unknown error code: ") + String(i);
        }
        return false;
    }
}


String JsonWriter::SaveData(const String& path, bool compact)
{
    std::ofstream file(path.CStr(), std::ios_base::trunc);
    if (file.is_open())
    {
        file << doc.dump(compact ? -1 : 4);
    }
    else
    {
        return "Couldn't open file";
    }

    return "";
}
void JsonWriter::WriteBool(bool value, const String& name)
{
    GetToUse()[name.CStr()] = value;
}
void JsonWriter::WriteByte(unsigned char value, const String& name)
{
    GetToUse()[name.CStr()] = value;
}
void JsonWriter::WriteInt(int value, const String& name)
{
    GetToUse()[name.CStr()] = value;
}
void JsonWriter::WriteUInt(size_t value, const String& name)
{
    GetToUse()[name.CStr()] = value;
}
void JsonWriter::WriteFloat(float value, const String& name)
{
    GetToUse()[name.CStr()] = value;
}
void JsonWriter::WriteDouble(double value, const String& name)
{
    GetToUse()[name.CStr()] = value;
}
void JsonWriter::WriteString(const String& value, const String& name)
{
    //Escape special characters.
    String newVal = value;
    for (size_t i = 0; i < newVal.GetSize(); ++i)
    {
        if (newVal[i] == '"' || newVal[i] == '\\')
        {
            newVal.Insert(i, '\\');
            i += 1;
        }
    }

    GetToUse()[name.CStr()] = value.CStr();
}
void JsonWriter::WriteBytes(const unsigned char* bytes, size_t nBytes, const String& name)
{
    String str = base64::encode(bytes, nBytes).c_str();
    WriteString(str, name);
}
void JsonWriter::WriteDataStructure(const IWritable& toSerialize, const String& name)
{
    GetToUse()[name.CStr()] = nlohmann::json::object();
    JsonWriter subWriter(&GetToUse()[name.CStr()]);
    toSerialize.WriteData(subWriter);
}


JsonReader::JsonReader(const String& filePath)
{
    ErrorMessage = Reload(filePath);
}

String JsonReader::Reload(const String& filePath)
{
    subDoc = nullptr;

    std::ifstream fileS(filePath.CStr());
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

void JsonReader::Assert(bool expr, const String& errorMsg)
{
    if (!expr)
    {
        ErrorMessage = errorMsg;
        throw EXCEPTION_FAILURE;
    }
}
nlohmann::json::const_iterator JsonReader::GetItem(const String& name)
{
    const nlohmann::json& jsn = GetToUse();
    auto element = jsn.find(name.CStr());
    Assert(element != jsn.end(),
           "Couldn't find the element.");
    return element;
}

void JsonReader::ReadBool(bool& outB, const String& name)
{
    auto& element = GetItem(name);
    Assert(element->is_boolean(), "Expected a boolean but got something else");
    outB = element->get<bool>();
}
void JsonReader::ReadByte(unsigned char& outB, const String& name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_unsigned(), "Expected a byte but got something else");
    outB = (unsigned char)element->get<size_t>();
}
void JsonReader::ReadInt(int& outI, const String& name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_integer(), "Expected an integer but got something else");
    outI = element->get<int>();
}
void JsonReader::ReadUInt(size_t& outU, const String& name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_unsigned(), "Expected an unsigned integer but got something else");
    outU = element->get<size_t>();
}
void JsonReader::ReadFloat(float& outF, const String& name)
{
    //Note that integers make valid floats.
    auto& element = GetItem(name);
    Assert(element->is_number_float() || element->is_number_integer(),
           "Expected a float but got something else");
    if (element->is_number_float())
        outF = element->get<float>();
    else
        outF = (float)element->get<int>();
}
void JsonReader::ReadDouble(double& outD, const String& name)
{
    //Note that integers make valid doubles.
    auto& element = GetItem(name);
    Assert(element->is_number_float() || element->is_number_integer(),
           "Expected a double but got something else");
    if (element->is_number_float())
        outD = element->get<double>();
    else
        outD = (double)element->get<int>();
}
void JsonReader::ReadString(String& outStr, const String& name)
{
    auto& element = GetItem(name);
    Assert(element->is_string(), "Expected a string but got something else");

    outStr = element->get<std::string>().c_str();

    //Fix escaped characters.
    for (size_t i = 0; i < outStr.GetSize(); ++i)
    {
        if (outStr[i] == '\\')
        {
            outStr.Erase(i);
            //Normally we would subtract 1 from "i" here, but we *want* to skip the next character --
            //    it's the one being escaped.
        }
    }
}
void JsonReader::ReadBytes(List<unsigned char>& outBytes, const String& name)
{
    String str;
    ReadString(str, name);

    std::vector<unsigned char> _outBytes;
    base64::decode(str.CStr(), _outBytes);

    outBytes.Resize(_outBytes.size());
    memcpy_s(outBytes.GetData(), outBytes.GetSize(),
             _outBytes.data(), _outBytes.size());
}
void JsonReader::ReadDataStructure(IReadable& outData, const String& name)
{
    auto& element = GetItem(name);
    Assert(element->is_object(), "Expected a data structure but got something else");
    
    JsonReader subReader(&(*element));
    outData.ReadData(subReader);
}


#pragma warning( default : 4996 )