#include "../Headers/MaterialValues.h"

#include "../Headers/ThirdParty/bmp_io.hpp"
#include "../Headers/ThirdParty/lodepng.h"



ADD_MVAL_REFLECTION_DATA_CPP(MaterialValue_Constant);
ADD_MVAL_REFLECTION_DATA_CPP(MaterialValue_Tex2D);


void MaterialValue_Constant::WriteData(DataWriter& writer) const
{
    MaterialValue::WriteData(writer);
    writer.WriteDataStructure(Vectorf_Writable(Value), "Value");
}
void MaterialValue_Constant::ReadData(DataReader& reader)
{
    MaterialValue::ReadData(reader);
    reader.ReadDataStructure(Vectorf_Readable(Value), "Value");
}


MaterialValue_Tex2D::MaterialValue_Tex2D(const std::string& _filePath, std::string& errMsg,
                                         SupportedFileTypes type)
{
    errMsg = Reload(_filePath, type);
}

std::string MaterialValue_Tex2D::Reload(const std::string& _filePath, SupportedFileTypes type)
{
    filePath = _filePath;
    fileType = type;

    if (fileType == UNKNOWN)
    {
        if (filePath.size() < 4)
            return "File-name was too short to infer a type";
        
        std::string extension = filePath.substr(filePath.size() - 4, 4);

        if (extension == ".png")
            fileType == PNG;
        else if (extension == ".bmp")
            fileType == BMP;
        else
            return "Couldn't infer type from file name";
    }

    switch (fileType)
    {
        case BMP: {

            unsigned long width;
            long height;
            unsigned char *reds,
                          *greens,
                          *blues;
            if (bmp_read(filePath.c_str(), &width, &height, &reds, &greens, &blues))
            {
                return "Error reading the BMP file";
            }

            Tex->Resize(width, height);

            //Convert to texture data.
            float invMaxVal = 1.0f / (float)std::numeric_limits<unsigned char>::max();
            for (long y = 0; y < height; ++y)
            {
                for (unsigned long x = 0; x < width; ++x)
                {
                    long i = x + (y * width);
                    Vector3f col((float)reds[i] * invMaxVal,
                                 (float)greens[i] * invMaxVal,
                                 (float)blues[i] * invMaxVal);
                    Tex->SetColor(x, y, col);
                }
            }
            } break;

        case PNG: {

            std::vector<unsigned char> bytes;
            unsigned int width, height;
            unsigned int errCode = lodepng::decode(bytes, width, height, filePath);
            if (errCode != 0)
            {
                return std::string("Error reading the PNG file: ") + lodepng_error_text(errCode);
            }

            Tex->Resize(width, height);

            //Convert to texture data.
            float invMaxVal = 1.0f / (float)std::numeric_limits<unsigned char>::max();
            for (unsigned int y = 0; y < height; ++y)
            {
                unsigned int indexOffset = y * width * 4;

                for (unsigned int x = 0; x < width; ++x)
                {
                    unsigned int indexOffset2 = indexOffset + (x * 4);

                    Tex->SetColor(x, y, Vector3f((float)bytes[indexOffset2] * invMaxVal,
                                                 (float)bytes[indexOffset2 + 1] * invMaxVal,
                                                 (float)bytes[indexOffset2 + 2] * invMaxVal));
                }
            }
            } break;

        default:
            return std::string("Unexpected file type enum value: ") + std::to_string(type);
    }

    return "";
}

void MaterialValue_Tex2D::WriteData(DataWriter& writer) const
{
    MaterialValue::WriteData(writer);
    writer.WriteString(filePath, "FilePath");
}
void MaterialValue_Tex2D::ReadData(DataReader& reader)
{
    MaterialValue::ReadData(reader);
    reader.ReadString(filePath, "FilePath");

    std::string err = Reload();
    if (err.size() > 0)
    {
        reader.ErrorMessage = std::string("Couldn't load tex file '") + filePath + "': " + err;
        throw DataReader::EXCEPTION_FAILURE;
    }
}


//Use macros to define the implementation for all the various simple MaterialValues.


#define GET_VAL(ptr) (ptr->GetValue(shpe, surface, ray, prng))

#define COMMA(thing1, thing2) thing1, thing2
#define COMMA3(thing1, thing2, thing3) thing1, thing2, thing3
#define COMMA4(thing1, thing2, thing3, thing4) thing1, thing2, thing3, thing4

#define EQUALS3(thing1, thing2, thing3) (thing1 == thing2 && thing2 == thing3)


#define IMPL_MULTI_MV(name, listParamName, startVal, accumVal) \
    ADD_MVAL_REFLECTION_DATA_CPP(MaterialValue_##name); \
    Dimensions MaterialValue_##name::GetNDims() const \
    { \
        Dimensions d = One; \
        for (size_t i = 0; i < listParamName.size(); ++i) \
            d = MaxIgnoring1D(d, listParamName[i]->GetNDims()); \
        return d; \
    } \
    Vectorf MaterialValue_##name::GetValue(const Shape& shpe, const Vertex& surface, \
                                           const Ray& ray, FastRand& prng) const \
    { \
        Vectorf val = startVal; \
        for (size_t i = 0; i < listParamName.size(); ++i) \
            accumVal; \
        return val; \
    } \
    void MaterialValue_##name::WriteData(DataWriter& writer) const \
    { \
        MaterialValue::WriteData(writer); \
        writer.WriteList<Ptr>(listParamName.data(), listParamName.size(), \
                              [](DataWriter& wr, const Ptr& p, const std::string& n) \
                              { wr.WriteString(p->GetTypeName(), n + "Type"); \
                                wr.WriteDataStructure(*p, n + "Value"); }, \
                              "Items"); \
    } \
    void MaterialValue_##name::ReadData(DataReader& reader) \
    { \
        MaterialValue::ReadData(reader); \
        reader.ReadList<Ptr>(&listParamName, \
                             [](void* pList, size_t nElements) \
                                { ((std::vector<Ptr>*)pList)->resize(nElements); }, \
                             [](DataReader& rdr, void* pList, size_t listIndex, const std::string& n) \
                             { \
                                 std::vector<Ptr>& list = *(std::vector<Ptr>*)pList; \
                                 std::string typeName; \
                                 rdr.ReadString(typeName, n + "Type"); \
                                 list.push_back(Create(typeName)); \
                                 rdr.ReadDataStructure(*list[list.size() - 1], n + "Value"); \
                             }, \
                             "Items"); \
    }
#define IMPL_SIMPLE_FUNC1(name, valFuncBody) \
    ADD_MVAL_REFLECTION_DATA_CPP(MaterialValue_##name); \
    Vectorf MaterialValue_##name::GetValue(const Shape& shpe, const Vertex& surface, \
                                           const Ray& ray, FastRand& prng) const \
    { \
        valFuncBody \
    }
#define IMPL_SIMPLE_FUNC(name, valFuncBody, dimsFuncBody) \
    ADD_MVAL_REFLECTION_DATA_CPP(MaterialValue_##name); \
    Vectorf MaterialValue_##name::GetValue(const Shape& shpe, const Vertex& surface, \
                                           const Ray& ray, FastRand& prng) const \
    { \
        valFuncBody \
    } \
    Dimensions MaterialValue_##name::GetNDims() const \
    { \
        dimsFuncBody \
    }

IMPL_MULTI_MV(Add, ToAdd, Vectorf(0.0f), val = val + GET_VAL(ToAdd[i]));
IMPL_MULTI_MV(Sub, ToSub, Vectorf(0.0f), val = (i == 0 ? GET_VAL(ToSub[0]) : (val - GET_VAL(ToSub[i]))));
IMPL_MULTI_MV(Mult, ToMultiply, Vectorf(1.0f), val = val + GET_VAL(ToMultiply[i]));
IMPL_MULTI_MV(Div, ToDivide, Vectorf(1.0f), val = (i == 0 ? GET_VAL(ToDivide[0]) : (val / GET_VAL(ToDivide[i]))));

IMPL_SIMPLE_FUNC1(Sin, return GET_VAL(Input).OperateOn([](float f) { return sinf(f); }););
IMPL_SIMPLE_FUNC1(Cos, return GET_VAL(Input).OperateOn([](float f) { return cosf(f); }););
IMPL_SIMPLE_FUNC1(Tan, return GET_VAL(Input).OperateOn([](float f) { return tanf(f); }););
IMPL_SIMPLE_FUNC1(Asin, return GET_VAL(Input).OperateOn([](float f) { return asinf(f); }););
IMPL_SIMPLE_FUNC1(Acos, return GET_VAL(Input).OperateOn([](float f) { return acosf(f); }););
IMPL_SIMPLE_FUNC1(Atan, return GET_VAL(Input).OperateOn([](float f) { return atanf(f); }););
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
                 return MinIgnoring1D(COMMA(A->GetNDims(), MinIgnoring1D(COMMA(B->GetNDims(), T->GetNDims()))));
                 );
IMPL_SIMPLE_FUNC1(Smoothstep, return GET_VAL(T).OperateOn([](float f) { return f * f * (3.0 - (2.0 * f)); }););
IMPL_SIMPLE_FUNC1(Smootherstep,
                  return GET_VAL(T).OperateOn([](float f)
                                              { return f * f * f * (10.0 + (f * (-15.0f + (6.0f * f)))); }););

IMPL_SIMPLE_FUNC(Clamp,
                 return GET_VAL(X).OperateOn(COMMA3([](COMMA3(float x, float a, float b)) { return (x < a ? a : (x > b ? b : x)); },
                                                    GET_VAL(Min),
                                                    GET_VAL(Max))); ,
                 return MinIgnoring1D(COMMA(Min->GetNDims(), MinIgnoring1D(Max->GetNDims(), X->GetNDims())));
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