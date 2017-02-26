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

ADD_MVAL_REFLECTION_DATA_CPP(MV_Swizzle);

ADD_MVAL_REFLECTION_DATA_CPP(MV_PureNoise);


namespace
{
    template<typename T>
    T GetMin(const T& one, const T& two) { return (one < two) ? one : two; }
    template<typename T>
    T GetMax(const T& one, const T& two) { return (one > two) ? one : two; }
}


void MV_Constant::WriteData(DataWriter& data, const String& namePrefix,
                            const ConstMaterialValueToID& idLookup) const
{
    MaterialValue::WriteData(data, namePrefix, idLookup);
    data.WriteDataStructure(Vectorf_Writable(Value), namePrefix + "Value");
}
void MV_Constant::ReadData(DataReader& data, const String& namePrefix,
                           NodeToChildIDs& childIDLookup)
{
    MaterialValue::ReadData(data, namePrefix, childIDLookup);
    data.ReadDataStructure(Vectorf_Readable(Value), namePrefix + "Value");
}


MV_Tex2D::MV_Tex2D(const String& _filePath, String& errMsg,
                   Ptr uv, Texture2D::SupportedFileTypes type)
    : UV(uv)
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


Vectorf MV_Swizzle::GetValue(const Ray& ray, FastRand& prng,
                             const Shape* shpe, const Vertex* surface) const
{
    Vectorf in = Val->GetValue(ray, prng, shpe, surface);
    Vectorf out;

    out.NValues = (Dimensions)NValues;
    for (size_t i = 0; i < NValues; ++i)
        out[i] = in[Swizzle[i]];

    return out;
}

void MV_Swizzle::WriteData(DataWriter& data, const String& namePrefix,
                           const ConstMaterialValueToID& idLookup) const
{
    MaterialValue::WriteData(data, namePrefix, idLookup);

    data.WriteByte(NValues, namePrefix + "NValues");
    for (size_t i = 0; i < NValues; ++i)
        data.WriteByte(Swizzle[i], namePrefix + "Value" + RT::String(i));
}
void MV_Swizzle::ReadData(DataReader& data, const String& namePrefix,
                          NodeToChildIDs& childIDLookup)
{
    MaterialValue::ReadData(data, namePrefix, childIDLookup);

    data.ReadByte(NValues, namePrefix + "NValues");
    for (size_t i = 0; i < NValues; ++i)
    {
        unsigned char c;
        data.ReadByte(c, namePrefix + "Value" + RT::String(i));
        Swizzle[i] = (Components)c;
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
void MV_PureNoise::WriteData(DataWriter& data, const String& namePrefix,
                             const ConstMaterialValueToID& idLookup) const
{
    MaterialValue::WriteData(data, namePrefix, idLookup);
    data.WriteByte(NDims, namePrefix + "Dimensions");
}
void MV_PureNoise::ReadData(DataReader& data, const String& namePrefix,
                            NodeToChildIDs& childIDLookup)
{
    MaterialValue::ReadData(data, namePrefix, childIDLookup);
    unsigned char b;
    data.ReadByte(b, namePrefix + "Dimensions");
    NDims = (Dimensions)b;
}


//Use macros to define the implementation for many MaterialValues.

#pragma region Helper macro definitions

#define GET_VAL(ptr) (ptr->GetValue(ray, prng, shpe, surface))

#define COMMA(thing1, thing2) thing1, thing2
#define COMMA3(thing1, thing2, thing3) thing1, thing2, thing3
#define COMMA4(thing1, thing2, thing3, thing4) thing1, thing2, thing3, thing4

#define EQUALS3(thing1, thing2, thing3) (thing1 == thing2 && thing2 == thing3)


#define IMPL_MULTI_MV_FULL(name, listParamName, defaultVal, accumVal, finalizeVal) \
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
        finalizeVal; \
        return val; \
    } \
    void MV_##name::RemoveElement(const MaterialValue* ptr) \
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
    void MV_##name::SetChild(size_t i, const Ptr& newChild) \
    { \
        if (listParamName.size() > i) \
            listParamName[i] = newChild; \
        else \
        { \
            assert(listParamName.size() == i); \
            listParamName.push_back(newChild); \
        } \
    }
#define IMPL_MULTI_MV(name, listParamName, defaultVal, accumVal) \
    IMPL_MULTI_MV_FULL(name, listParamName, defaultVal, accumVal, );

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
IMPL_MULTI_MV_FULL(Average, ToAverage,
                   Vectorf(0.0f), val = val + GET_VAL(ToAverage[i]),
                   val = val / (float)ToAverage.size());

#pragma region MV_Append

ADD_MVAL_REFLECTION_DATA_CPP(MV_Append);
Dimensions MV_Append::GetNDims() const
{
    Dimensions d = (Dimensions)0;
    for (size_t i = 0; i < ToCombine.size(); ++i)
        d = (Dimensions)((unsigned char)d + ToCombine[i]->GetNDims());
    return d;
}
Vectorf MV_Append::GetValue(const Ray& ray, FastRand& prng,
                            const Shape* shpe, const Vertex* surface) const
{
    if (ToCombine.size() == 0)
        return 0.0f;

    Vectorf val = GET_VAL(ToCombine[0]);
    for (size_t i = 1; i < ToCombine.size(); ++i)
    {
        Vectorf tempVal = GET_VAL(ToCombine[1]);
        for (size_t componentI = 0; componentI < tempVal.NValues; ++componentI)
        {
            val.NValues = (Dimensions)((unsigned char)val.NValues + 1);
            val[val.NValues - 1] = tempVal[componentI];
        }
    }
    return val;
}
void MV_Append::RemoveElement(const MaterialValue* ptr)
{
    for (size_t i = 0; i < ToCombine.size(); ++i)
    {
        if (ToCombine[i].Get() == ptr)
        {
            ToCombine.erase(ToCombine.begin() + i);
            return;
        }
    }
}
void MV_Append::SetChild(size_t i, const Ptr& newChild)
{
    if (ToCombine.size() > i)
        ToCombine[i] = newChild;
    else
    {
        assert(ToCombine.size() == i);
        ToCombine.push_back(newChild);
    }
}

#pragma endregion

IMPL_SIMPLE_FUNC1(Normalize, return GET_VAL(X).Normalized(); );
IMPL_SIMPLE_FUNC1(Length, return GET_VAL(X).Length(););
IMPL_SIMPLE_FUNC(Distance,
                 return GET_VAL(A).Distance(GET_VAL(B)); ,
                 return One; );
IMPL_SIMPLE_FUNC(Dot, return GET_VAL(A).Dot(GET_VAL(B)); ,
                 return One; );
IMPL_SIMPLE_FUNC(Reflect, return GET_VAL(V).Reflect(GET_VAL(Normal)); ,
                 return Max(V->GetNDims(), Normal->GetNDims()); );
IMPL_SIMPLE_FUNC(Refract,
                 return GET_VAL(V).Refract(COMMA(GET_VAL(Normal),
                                                 (float)GET_VAL(IndexOfRefraction))); ,
                 return Max(V->GetNDims(), Normal->GetNDims()); );

IMPL_SIMPLE_FUNC1(Sqrt, return GET_VAL(X).OperateOn(&sqrtf); );
IMPL_SIMPLE_FUNC(Pow, return GET_VAL(Base).OperateOn(COMMA(&powf, GET_VAL(Exp))); ,
                 return Max(COMMA(Base->GetNDims(), Exp->GetNDims())); );
IMPL_SIMPLE_FUNC1(Ln, return GET_VAL(X).OperateOn(&logf); );

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