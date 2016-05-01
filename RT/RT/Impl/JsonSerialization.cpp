#include "../Headers/JsonSerialization.h"

#include "../Headers/ThirdParty/base64.h"
#include <fstream>


#pragma warning( disable : 4996 )


bool RT_API JsonSerialization::ToJSONFile(const char* filePath, const IWritable& toWrite,
                                          bool compact, char* outErrorMsg, size_t maxErrMsgSize)
{
    JsonWriter writer;
    std::string trying = "UNKNOWN";
    try
    {
        trying = "serializing data";
        writer.WriteDataStructure(toWrite, "data");
        trying = "writing data to file";
        std::string err = writer.SaveData(filePath, compact);
        if (!err.empty())
        {
            err = "Error writing file: " + err;
            if (err.size() > maxErrMsgSize)
            {
                err = err.substr(0, maxErrMsgSize);
            }
            strcpy(outErrorMsg, err.c_str());
            return false;
        }

        return true;
    }
    catch (int i)
    {
        std::string err;
        if (i == DataWriter::EXCEPTION_FAILURE)
        {
            err = std::string("Error while ") + trying + ": " + writer.ErrorMessage;
        }
        else
        {
            err = "Unknown error code while " + trying + ": " + std::to_string(i);
        }
        
        if (err.size() > maxErrMsgSize)
        {
            err = err.substr(0, maxErrMsgSize);
        }

        strcpy(outErrorMsg, err.c_str());
        return false;
    }
}
bool RT_API JsonSerialization::ToJSONString(const IWritable& toWrite, bool compact,
                                            void* outString,
                                            void(*setOutString)(void* outStr, const char* str, size_t strLen),
                                            char* outErrorMsg, size_t maxErrMsgSize)
{
    JsonWriter writer;
    std::string trying = "UNKNOWN";
    try
    {
        trying = "serializing data";
        writer.WriteDataStructure(toWrite, "data");
        std::string asStr = writer.GetData(compact);
        setOutString(outString, asStr.c_str(), asStr.size());
        return true;
    }
    catch (int i)
    {
        std::string err;
        if (i == DataWriter::EXCEPTION_FAILURE)
        {
            err = std::string("Error while ") + trying + ": " + writer.ErrorMessage;
        }
        else
        {
            err = "Unknown error code while " + trying + ": " + std::to_string(i);
        }

        if (err.size() > maxErrMsgSize)
        {
            err = err.substr(0, maxErrMsgSize);
        }

        strcpy(outErrorMsg, err.c_str());
        return false;
    }
}
bool RT_API JsonSerialization::FromJSONFile(const char* filePath, IReadable& toRead,
                                            char* outErrorMsg, size_t maxErrMsgSize)
{
    JsonReader reader(filePath);

    //If we had an error reading the file, stop.
    if (strcmp("", reader.ErrorMessage) != 0)
    {
        std::string err = std::string("Error reading file: ") + reader.ErrorMessage;
        if (err.size() > maxErrMsgSize)
            err = err.substr(0, maxErrMsgSize);

        strcpy(outErrorMsg, err.c_str());
    }

    try
    {
        reader.ReadDataStructure(toRead, "data");
        return true;
    }
    catch (int i)
    {
        std::string err;
        if (i == DataReader::EXCEPTION_FAILURE)
        {
            err = std::string("Error reading data: ") + reader.ErrorMessage;
        }
        else
        {
            err = "Unknown error code: " + std::to_string(i);
        }

        if (err.size() > maxErrMsgSize)
        {
            err = err.substr(0, maxErrMsgSize);
        }

        strcpy(outErrorMsg, err.c_str());
        return false;
    }
}


std::string JsonWriter::SaveData(const char* path, bool compact)
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
void JsonWriter::WriteBool(bool value, const char* name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteByte(unsigned char value, const char* name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteInt(int value, const char* name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteUInt(unsigned int value, const char* name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteFloat(float value, const char* name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteDouble(double value, const char* name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteString(const char* value, const char* name)
{
    GetToUse()[name] = value;
}
void JsonWriter::WriteBytes(const unsigned char* bytes, size_t nBytes, const char* name)
{
    std::string str = base64::encode(bytes, nBytes);
    WriteString(str.c_str(), name);
}
void JsonWriter::WriteDataStructure(const IWritable& toSerialize, const char* name)
{
    GetToUse()[name] = nlohmann::json::object();
    JsonWriter subWriter(&GetToUse()[name]);
    toSerialize.WriteData(subWriter);
}
void JsonWriter::WriteCollection(ElementWriter writerFunc, const char* name,
                                 unsigned int bytesPerElement,
                                 const void* collection, unsigned int collectionSize,
                                 void* userData)
{
    nlohmann::json& toUse = GetToUse();

    toUse[name] = nlohmann::json::object();
    nlohmann::json& obj = toUse[name];
    JsonWriter subWriter(&obj);

    subWriter.WriteUInt(collectionSize, "NumbElements");

    const char* collP = (const char*)collection;
    for (size_t i = 0; i < collectionSize; ++i)
    {
        writerFunc(subWriter, collP, i, userData);
        collP += bytesPerElement;
    }
}


JsonReader::JsonReader(const char* filePath)
{
    std::string err = Reload(filePath);
    if (!err.empty())
    {
        if (err.size() > MAX_ERROR_SIZE)
            err[MAX_ERROR_SIZE] = '\0';
        strcpy(ErrorMessage, err.c_str());
    }
}

std::string JsonReader::Reload(const char* filePath)
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
        std::string str = errorMsg;
        if (str.size() > MAX_ERROR_SIZE)
        {
            str = str.substr(0, MAX_ERROR_SIZE);
        }
        strcpy(ErrorMessage, str.c_str());

        throw EXCEPTION_FAILURE;
    }
}
nlohmann::json::const_iterator JsonReader::GetItem(const char* name)
{
    const nlohmann::json& jsn = GetToUse();
    auto element = jsn.find(name);
    Assert(element != jsn.end(),
           "Couldn't find the element.");
    return element;
}

void JsonReader::ReadBool(bool& outB, const char* name)
{
    auto& element = GetItem(name);
    Assert(element->is_boolean(), "Expected a boolean but got something else");
    outB = element->get<bool>();
}
void JsonReader::ReadByte(unsigned char& outB, const char* name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_unsigned(), "Expected a byte but got something else");
    outB = (unsigned char)element->get<unsigned int>();
}
void JsonReader::ReadInt(int& outI, const char* name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_integer(), "Expected an integer but got something else");
    outI = element->get<int>();
}
void JsonReader::ReadUInt(unsigned int& outU, const char* name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_unsigned(), "Expected an unsigned integer but got something else");
    outU = element->get<unsigned int>();
}
void JsonReader::ReadFloat(float& outF, const char* name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_float(), "Expected a float but got something else");
    outF = element->get<float>();
}
void JsonReader::ReadDouble(double& outD, const char* name)
{
    auto& element = GetItem(name);
    Assert(element->is_number_float(), "Expected a double but got something else");
    outD = element->get<double>();
}
void JsonReader::ReadString(std::string& outStr, const char* name)
{
    auto& element = GetItem(name);
    Assert(element->is_string(), "Expected a string but got something else");

    outStr = element->get<std::string>();
}
void JsonReader::ReadString(char* outStr, size_t maxStrSize, const char* name)
{
    std::string str;
    ReadString(str, name);

    Assert(str.size() <= maxStrSize, "String is too long to fit in the buffer");
    strcpy(outStr, str.c_str());
}
void JsonReader::ReadBytes(std::vector<unsigned char>& outBytes, const char* name)
{
    std::string str;
    ReadString(str, name);

    base64::decode(str, outBytes);
}
void JsonReader::ReadDataStructure(IReadable& outData, const char* name)
{
    auto& element = GetItem(name);
    Assert(element->is_object(), "Expected a data structure but got something else");
    
    JsonReader subReader(&(*element));
    outData.ReadData(subReader);
}
void JsonReader::ReadCollection(ElementReader readerFunc, CollectionResizer resizer,
                                const char* name,
                                void* pCollection, void* userData)
{
    auto& element = GetItem(name);
    Assert(element->is_object(), "Expected an object but got something else");

    JsonReader subReader(&(*element));

    unsigned int nElements;
    subReader.ReadUInt(nElements, "NumbElements");

    resizer(pCollection, nElements);

    for (size_t i = 0; i < nElements; ++i)
    {
        readerFunc(subReader, pCollection, i, userData);
    }
}


#pragma warning( default : 4996 )