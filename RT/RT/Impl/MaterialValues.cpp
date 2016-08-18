#include "../Headers/MaterialValues.h"

#include "../Headers/ThirdParty/bmp_io.hpp"
#include "../Headers/ThirdParty/lodepng.h"

using namespace RT;



ADD_MVAL_REFLECTION_DATA_CPP(MV_Constant);
ADD_MVAL_REFLECTION_DATA_CPP(MV_Tex2D);

ADD_MVAL_REFLECTION_DATA_CPP(MV_SurfUV);
ADD_MVAL_REFLECTION_DATA_CPP(MV_SurfPos);
ADD_MVAL_REFLECTION_DATA_CPP(MV_SurfNormal);
ADD_MVAL_REFLECTION_DATA_CPP(MV_SurfTangent);
ADD_MVAL_REFLECTION_DATA_CPP(MV_SurfBitangent);
ADD_MVAL_REFLECTION_DATA_CPP(MV_RayStartPos);
ADD_MVAL_REFLECTION_DATA_CPP(MV_RayPos);
ADD_MVAL_REFLECTION_DATA_CPP(MV_RayDir);
ADD_MVAL_REFLECTION_DATA_CPP(MV_ShapePos);
ADD_MVAL_REFLECTION_DATA_CPP(MV_ShapeScale);
ADD_MVAL_REFLECTION_DATA_CPP(MV_ShapeRot);

ADD_MVAL_REFLECTION_DATA_CPP(MV_PureNoise);


namespace
{
    template<typename T>
    T GetMin(const T& one, const T& two) { return (one < two) ? one : two; }
    template<typename T>
    T GetMax(const T& one, const T& two) { return (one > two) ? one : two; }
}


void MV_Constant::WriteData(DataWriter& writer) const
{
    MaterialValue::WriteData(writer);
    writer.WriteDataStructure(Vectorf_Writable(Value), "Value");
}
void MV_Constant::ReadData(DataReader& reader)
{
    MaterialValue::ReadData(reader);
    reader.ReadDataStructure(Vectorf_Readable(Value), "Value");
}


MV_Tex2D::MV_Tex2D(const String& _filePath, String& errMsg,
                   Ptr uv, Texture2D::SupportedFileTypes type)
    : UV(uv.Release())
{
    errMsg = Reload(_filePath, type);
}

String MV_Tex2D::Reload(const String& _filePath,
                        Texture2D::SupportedFileTypes type)
{
    filePath = _filePath;
    fileType = type;

    if (Tex.Get() == nullptr)
    {
        String errMsg = "";
        Tex = new Texture2D(filePath, errMsg, type);
        return errMsg;
    }
    else
    {
        return Tex->Reload(filePath, fileType);
    }
}

void MV_Tex2D::WriteData(DataWriter& writer) const
{
    MaterialValue::WriteData(writer);
    writer.WriteString(filePath, "FilePath");
    switch (fileType)
    {
        case Texture2D::BMP:
            writer.WriteString("BMP", "FileType");
            break;
        case Texture2D::PNG:
            writer.WriteString("PNG", "FileType");
            break;
        case Texture2D::UNKNOWN:
            writer.WriteString("Automatic", "FileType");
            break;
        default:
            writer.ErrorMessage = "Unknown Texture2D-supported file-type: ";
            writer.ErrorMessage += String(fileType);
            throw DataWriter::EXCEPTION_FAILURE;
    }
    WriteValue(UV, writer, "UV");
}
void MV_Tex2D::ReadData(DataReader& reader)
{
    MaterialValue::ReadData(reader);
    reader.ReadString(filePath, "FilePath");

    String typeStr;
    reader.ReadString(typeStr, "FileType");

    if (typeStr == "BMP")
    {
        fileType = Texture2D::BMP;
    }
    else if (typeStr == "PNG")
    {
        fileType = Texture2D::PNG;
    }
    else if (typeStr == "Automatic")
    {
        fileType = Texture2D::UNKNOWN;
    }
    else
    {
        reader.ErrorMessage = "Unknown Texture2D-supported file-type: ";
        reader.ErrorMessage += typeStr;
        throw DataReader::EXCEPTION_FAILURE;
    }

    ReadValue(UV, reader, "UV");

    //Try loading the texture.
    String err = Reload();
    if (err.GetSize() > 0)
    {
        reader.ErrorMessage = String("Couldn't load tex file '") + filePath + "': " + err;
        throw DataReader::EXCEPTION_FAILURE;
    }
}


Vectorf MV_PureNoise::GetValue(const Ray& ray, FastRand& prng,
                               const Shape* shpe,
                               const Vertex* surface) const
{
    Vectorf noiseVal;
    noiseVal.NValues = NDims;
    for (size_t i = 0; i < NDims; ++i)
        noiseVal[i] = prng.NextFloat();
    return noiseVal;
}
void MV_PureNoise::WriteData(DataWriter& writer) const
{
    MaterialValue::WriteData(writer);
    writer.WriteByte(NDims, "Dimensions");
}
void MV_PureNoise::ReadData(DataReader& reader)
{
    MaterialValue::ReadData(reader);
    unsigned char b;
    reader.ReadByte(b, "Dimensions");
    NDims = (Dimensions)b;
}


//Use macros to define the implementation for many MaterialValues.

#pragma region Helper macro definitions

#define GET_VAL(ptr) (ptr->GetValue(ray, prng, shpe, surface))

#define COMMA(thing1, thing2) thing1, thing2
#define COMMA3(thing1, thing2, thing3) thing1, thing2, thing3
#define COMMA4(thing1, thing2, thing3, thing4) thing1, thing2, thing3, thing4

#define EQUALS3(thing1, thing2, thing3) (thing1 == thing2 && thing2 == thing3)


#define IMPL_MULTI_MV(name, listParamName, defaultVal, accumVal) \
    ADD_MVAL_REFLECTION_DATA_CPP(MV_##name); \
    Dimensions MV_##name::GetNDims() const \
    { \
        Dimensions d = One; \
        for (size_t i = 0; i < listParamName.size(); ++i) \
            d = Max(d, listParamName[i]->GetNDims()); \
        return d; \
    } \
    Vectorf MV_##name::GetValue(const Ray& ray, FastRand& prng, \
                                const Shape* shpe, const Vertex* surface) const \
    { \
        if (listParamName.size() == 0) \
            return defaultVal; \
        Vectorf val = GET_VAL(listParamName[0]); \
        for (size_t i = 1; i < listParamName.size(); ++i) \
            accumVal; \
        return val; \
    } \
    void MV_##name::RemoveElement(MaterialValue* ptr) \
    { \
        for (size_t i = 0; i < listParamName.size(); ++i) \
        { \
            if (listParamName[i].Get() == ptr) \
            { \
                listParamName.erase(listParamName.begin() + i); \
                return; \
            } \
        } \
    } \
    void MV_##name::WriteData(DataWriter& writer) const \
    { \
        MaterialValue::WriteData(writer); \
        writer.WriteList<Ptr>(listParamName.data(), listParamName.size(), \
                              [](DataWriter& wr, const Ptr& p, const String& n) \
                              { wr.WriteString(p->GetTypeName(), n + "Type"); \
                                wr.WriteDataStructure(*p, n + "Value"); }, \
                              "Items"); \
    } \
    void MV_##name::ReadData(DataReader& reader) \
    { \
        MaterialValue::ReadData(reader); \
        reader.ReadList<Ptr>(&listParamName, \
                             [](void* pList, size_t nElements) \
                                { ((std::vector<Ptr>*)pList)->resize(nElements); }, \
                             [](DataReader& rdr, void* pList, size_t listIndex, const String& n) \
                             { \
                                 std::vector<Ptr>& list = *(std::vector<Ptr>*)pList; \
                                 String typeName; \
                                 rdr.ReadString(typeName, n + "Type"); \
                                 list.push_back(Create(typeName)); \
                                 rdr.ReadDataStructure(*list[list.size() - 1], n + "Value"); \
                             }, \
                             "Items"); \
    }
#define IMPL_SIMPLE_FUNC1(name, valFuncBody) \
    ADD_MVAL_REFLECTION_DATA_CPP(MV_##name); \
    Vectorf MV_##name::GetValue(const Ray& ray, FastRand& prng, \
                                const Shape* shpe, const Vertex* surface) const \
    { \
        valFuncBody \
    }
#define IMPL_SIMPLE_FUNC(name, valFuncBody, dimsFuncBody) \
    ADD_MVAL_REFLECTION_DATA_CPP(MV_##name); \
    Vectorf MV_##name::GetValue(const Ray& ray, FastRand& prng, \
                                const Shape* shpe, const Vertex* surface) const \
    { \
        valFuncBody \
    } \
    Dimensions MV_##name::GetNDims() const \
    { \
        dimsFuncBody \
    }

#pragma endregion

IMPL_MULTI_MV(Add, ToAdd, Vectorf(0.0f), val = val + GET_VAL(ToAdd[i]));
IMPL_MULTI_MV(Subtract, ToSub, Vectorf(0.0f), val = val - GET_VAL(ToSub[i]));
IMPL_MULTI_MV(Multiply, ToMultiply, Vectorf(0.0f), val = val * GET_VAL(ToMultiply[i]));
IMPL_MULTI_MV(Divide, ToDivide, Vectorf(0.0f), val = val / GET_VAL(ToDivide[i]));
IMPL_MULTI_MV(Min, ToUse, Vectorf(0.0f),
              val = GET_VAL(ToUse[i]).OperateOn(COMMA3(&GetMin<float>, val,
                                                       std::numeric_limits<float>::max())); );
IMPL_MULTI_MV(Max, ToUse, Vectorf(0.0f),
              val = GET_VAL(ToUse[i]).OperateOn(COMMA3(&GetMax<float>, val,
                                                       std::numeric_limits<float>::max())); );

IMPL_SIMPLE_FUNC1(Normalize, return GET_VAL(X).Normalized(); );
IMPL_SIMPLE_FUNC1(Length, return GET_VAL(X).Length(););
IMPL_SIMPLE_FUNC(Distance,
                 return GET_VAL(A).Distance(GET_VAL(B)); ,
                 return One; );

IMPL_SIMPLE_FUNC1(Sqrt, return GET_VAL(X).OperateOn(&sqrtf); );
IMPL_SIMPLE_FUNC1(Sin, return GET_VAL(X).OperateOn(&sinf); );
IMPL_SIMPLE_FUNC1(Cos, return GET_VAL(X).OperateOn(&cosf); );
IMPL_SIMPLE_FUNC1(Tan, return GET_VAL(X).OperateOn(&tanf); );
IMPL_SIMPLE_FUNC1(Asin, return GET_VAL(X).OperateOn(&asinf); );
IMPL_SIMPLE_FUNC1(Acos, return GET_VAL(X).OperateOn(&acosf); );
IMPL_SIMPLE_FUNC1(Atan, return GET_VAL(X).OperateOn(&atanf); );
IMPL_SIMPLE_FUNC(Atan2,
                 return GET_VAL(Y).OperateOn(COMMA([](COMMA(float y, float x)) { return atan2f(COMMA(y, x)); },
                                                    GET_VAL(X))); ,
                 return MinIgnoring1D(COMMA(X->GetNDims(), Y->GetNDims()));
                 );
IMPL_SIMPLE_FUNC(Step,
                 return GET_VAL(Edge).OperateOn(COMMA([](float edge, float x) { return (x < edge ? 0.0f : 1.0f); },
                                                      GET_VAL(X))); ,
                 return MinIgnoring1D(COMMA(Edge->GetNDims(), X->GetNDims()));
                 );
IMPL_SIMPLE_FUNC(Lerp,
                 return GET_VAL(T).OperateOn(COMMA3([](COMMA3(float t, float a, float b)) { return a + (t * (b - a)); },
                                                    GET_VAL(A),
                                                    GET_VAL(B))); ,
                 return MinIgnoring1D(COMMA3(A->GetNDims(), B->GetNDims(), T->GetNDims()));
                 );
IMPL_SIMPLE_FUNC1(Smoothstep, return GET_VAL(T).OperateOn([](float f) { return f * f * (3.0f - (2.0f * f)); }););
IMPL_SIMPLE_FUNC1(Smootherstep,
                  return GET_VAL(T).OperateOn([](float f)
                                              { return f * f * f * (10.0f + (f * (-15.0f + (6.0f * f)))); }););

IMPL_SIMPLE_FUNC1(Abs, return GET_VAL(X).OperateOn(&fabsf); );
IMPL_SIMPLE_FUNC1(Floor, return GET_VAL(X).OperateOn(&floorf); );
IMPL_SIMPLE_FUNC1(Ceil, return GET_VAL(X).OperateOn(&ceilf); );

IMPL_SIMPLE_FUNC(Clamp,
                 return GET_VAL(X).OperateOn(COMMA3([](COMMA3(float x, float a, float b)) { return (x < a ? a : (x > b ? b : x)); },
                                                    GET_VAL(Min),
                                                    GET_VAL(Max))); ,
                 return MinIgnoring1D(COMMA3(Min->GetNDims(), Max->GetNDims(), X->GetNDims()));
                 );


#undef GET_VAL
#undef COMMA
#undef COMMA3
#undef COMMA4
#undef EQUALS3
#undef SMALLEST_NDIMS2
#undef SMALLEST_NDIMS3
#undef LARGEST_NDIMS2
#undef LARGEST_NDIMS3
#undef BODY
#undef IMPL_MULTI_MV
#undef IMPL_SIMPLE_FUNC1
#undef IMPL_SIMPLE_FUNC