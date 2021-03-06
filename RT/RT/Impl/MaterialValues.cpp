#include "../Headers/MaterialValues.h"

#include "../Headers/Mathf.h"

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
ADD_MVAL_REFLECTION_DATA_CPP(MV_Map);
ADD_MVAL_REFLECTION_DATA_CPP(MV_Cross);

ADD_MVAL_REFLECTION_DATA_CPP(MV_PureNoise);
ADD_MVAL_REFLECTION_DATA_CPP(MV_PerlinNoise);
ADD_MVAL_REFLECTION_DATA_CPP(MV_WorleyNoise);


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

Dimensions MV_Map::GetNDims() const
{
    return Max(X->GetNDims(),
               Max(SrcMin->GetNDims(),
                   Max(SrcMax->GetNDims(),
                       Max(DestMin->GetNDims(),
                           DestMax->GetNDims()))));
}
Vectorf MV_Map::GetValue(const Ray& ray, FastRand& prng,
                         const Shape* shpe, const Vertex* surface) const
{
    auto x = X->GetValue(ray, prng, shpe, surface),
         srcMin = SrcMin->GetValue(ray, prng, shpe, surface),
         srcMax = SrcMax->GetValue(ray, prng, shpe, surface),
         destMin = DestMin->GetValue(ray, prng, shpe, surface),
         destMax = DestMax->GetValue(ray, prng, shpe, surface);

    auto t = srcMin.OperateOn(Mathf::InvLerp, srcMax, x);
    return destMin.OperateOn(Mathf::Lerp, destMax, t);
}
const MaterialValue* MV_Map::GetChild(size_t i) const
{
    switch (i)
    {
        case 0: return X.Get();
        case 1: return SrcMin.Get();
        case 2: return SrcMax.Get();
        case 3: return DestMin.Get();
        case 4: return DestMax.Get();
        default: assert(false); return X.Get();
    }
}
void MV_Map::SetChild(size_t i, const Ptr& newChild)
{
    switch (i)
    {
        case 0: X = newChild; return;
        case 1: SrcMin = newChild; return;
        case 2: SrcMax = newChild; return;
        case 3: DestMin = newChild; return;
        case 4: DestMax = newChild; return;
        default: assert(false);
    }
}

Vectorf MV_Cross::GetValue(const Ray& ray, FastRand& prng,
                           const Shape* shpe, const Vertex* surface) const
{
    Vector3f a = A->GetValue(ray, prng, shpe, surface),
             b = B->GetValue(ray, prng, shpe, surface);
    return a.Cross(b);
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

namespace NoiseFuncs
{
    //Returns a pseudo-random value between 0 and 1 using the given seed.
    float Hash(const Vectorf& v)
    {
        switch (v.NValues)
        {
            case RT::Dimensions::One: return FastRand(v.x).NextFloat();
            case RT::Dimensions::Two: return FastRand(v.x, v.y).NextFloat();
            case RT::Dimensions::Three: return FastRand(v.x, v.y, v.z).NextFloat();
            case RT::Dimensions::Four: return FastRand(v.x, v.y, v.z, v.w).NextFloat();
            
            default: assert(false); return FastRand(v.x).NextFloat();
        }
    }

    float Perlin(float x)
    {
        float minX = floorf(x),
              maxX = minX + 1.0f,
              t = x - minX;

        float minX_V = -1.0 + (2.0 * Hash(minX));
        float toMin = -t;

        float maxX_V = -1.0 + (2.0 * Hash(maxX));
        float toMax = maxX - x;

        float outVal = Mathf::Lerp(minX_V * toMin,
                                   maxX_V * toMax,
                                   Mathf::SmoothLerp(t));
        return 0.5 + (0.5 * outVal);
    }
    float Perlin(Vector2f v)
    {
        Vector2f minXY(floorf(v.x), floorf(v.y)),
                 maxXY = minXY + 1.0f,
                 minXMaxY(minXY.x, maxXY.y),
                 maxXMinY(maxXY.x, minXY.y);
        Vector2f t = v - minXY;

        float temp;

        temp = Hash(minXY);
        Vector2f minXY_V = (Vector2f(temp, Hash(temp)) * 2.0f) - 1.0f,
                 toMinXY = -t;

        temp = Hash(maxXY);
        Vector2f maxXY_V = (Vector2f(temp, Hash(temp)) * 2.0f) - 1.0f,
                 toMaxXY = maxXY - v;

        temp = Hash(minXMaxY);
        Vector2f minXMaxY_V = (Vector2f(temp, Hash(temp)) * 2.0f) - 1.0f,
                 toMinXMaxY = minXMaxY - v;

        temp = Hash(maxXMinY);
        Vector2f maxXMinY_V = (Vector2f(temp, Hash(temp)) * 2.0f) - 1.0f,
                 toMaxXMinY = maxXMinY - v;

        t = Vector2f(Mathf::SmoothLerp(t.x), Mathf::SmoothLerp(t.y));
        float outVal = Mathf::Lerp(Mathf::Lerp(minXY_V.Dot(toMinXY),
                                               maxXMinY_V.Dot(toMaxXMinY),
                                               t.x),
                                   Mathf::Lerp(minXMaxY_V.Dot(toMinXMaxY),
                                               maxXY_V.Dot(toMaxXY),
                                               t.x),
                                   t.y);
        return 0.5 + (0.5 * outVal);
    }
    float Perlin(Vector3f v)
    {
        Vector3f minXYZ(floorf(v.x), floorf(v.y), floorf(v.z)),
                 maxXYZ = minXYZ + 1.0f,
                 maxXMinYZ    (maxXYZ.x, minXYZ.y, minXYZ.z),
                 minXMaxYMinZ (minXYZ.x, maxXYZ.y, minXYZ.z),
                 minXYMaxZ    (minXYZ.x, minXYZ.y, maxXYZ.z),
                 maxXYMinZ    (maxXYZ.x, maxXYZ.y, minXYZ.z),
                 maxXMinYMaxZ (maxXYZ.x, minXYZ.y, maxXYZ.z),
                 minXMaxYZ    (minXYZ.x, maxXYZ.y, maxXYZ.z);
        Vector3f t = v - minXYZ;

        Vector3f temp;
#define DO(toDo) \
    temp.x = Hash(toDo); \
    temp.y = Hash(temp.x); \
    temp.z = Hash(temp.y); \
    Vector3f toDo##_V = (temp * 2.0f) - 1.0f, \
             to_##toDo = toDo - v;

        DO(minXYZ);
        DO(maxXMinYZ);
        DO(minXMaxYMinZ);
        DO(minXYMaxZ);
        DO(maxXYMinZ);
        DO(maxXMinYMaxZ);
        DO(minXMaxYZ);
        DO(maxXYZ);
#undef DO

        t = Vector3f(Mathf::SmoothLerp(t.x),
                     Mathf::SmoothLerp(t.y),
                     Mathf::SmoothLerp(t.z));

#define DOT(a) a##_V.Dot(to_##a)
        float outVal = Mathf::Lerp(Mathf::Lerp(Mathf::Lerp(DOT(minXYZ), DOT(maxXMinYZ), t.x),
                                               Mathf::Lerp(DOT(minXMaxYMinZ), DOT(maxXYMinZ), t.x),
                                               t.y),
                                   Mathf::Lerp(Mathf::Lerp(DOT(minXYMaxZ), DOT(maxXMinYMaxZ), t.x),
                                               Mathf::Lerp(DOT(minXMaxYZ), DOT(maxXYZ), t.x),
                                               t.y),
                                   t.z);
#undef DOT

        return 0.5 + (0.5 * outVal);
    }
    float Perlin(Vector4f v)
    {
        Vector4f min(floorf(v.x), floorf(v.y), floorf(v.z), floorf(v.w)),
                 max = min + 1.0f,
                 minXminYminZminW = min,
                 minXminYminZmaxW (min.x, min.y, min.z, max.w),
                 minXminYmaxZminW (min.x, min.y, max.z, min.w),
                 minXminYmaxZmaxW (min.x, min.y, max.z, max.w),
                 minXmaxYminZminW (min.x, max.y, min.z, min.w),
                 minXmaxYminZmaxW (min.x, max.y, min.z, max.w),
                 minXmaxYmaxZminW (min.x, max.y, max.z, min.w),
                 minXmaxYmaxZmaxW (min.x, max.y, max.z, max.w),
                 maxXminYminZminW (max.x, min.y, min.z, min.w),
                 maxXminYminZmaxW (max.x, min.y, min.z, max.w),
                 maxXminYmaxZminW (max.x, min.y, max.z, min.w),
                 maxXminYmaxZmaxW (max.x, min.y, max.z, max.w),
                 maxXmaxYminZminW (max.x, max.y, min.z, min.w),
                 maxXmaxYminZmaxW (max.x, max.y, min.z, max.w),
                 maxXmaxYmaxZminW (max.x, max.y, max.z, min.w),
                 maxXmaxYmaxZmaxW = max;

        Vector4f t = v - min;

        Vector4f temp;
#define DO(toDo) \
    temp.x = Hash(toDo); \
    temp.y = Hash(temp.x); \
    temp.z = Hash(temp.y); \
    temp.w = Hash(temp.z); \
    Vector4f toDo##_V = (temp * 2.0f) - 1.0f, \
             to_##toDo = toDo - v;

        DO(minXminYminZminW);
        DO(minXminYminZmaxW);
        DO(minXminYmaxZminW);
        DO(minXminYmaxZmaxW);
        DO(minXmaxYminZminW);
        DO(minXmaxYminZmaxW);
        DO(minXmaxYmaxZminW);
        DO(minXmaxYmaxZmaxW);
        DO(maxXminYminZminW);
        DO(maxXminYminZmaxW);
        DO(maxXminYmaxZminW);
        DO(maxXminYmaxZmaxW);
        DO(maxXmaxYminZminW);
        DO(maxXmaxYminZmaxW);
        DO(maxXmaxYmaxZminW);
        DO(maxXmaxYmaxZmaxW);
#undef DO

        t = Vector4f(Mathf::SmoothLerp(t.x),
                     Mathf::SmoothLerp(t.y),
                     Mathf::SmoothLerp(t.z),
                     Mathf::SmoothLerp(t.w));

#define DOT(a) (a##_V.Dot(to_##a))
#define LERP(x1, y1, z1, w1, x2, y2, z2, w2, tComponent) \
        (Mathf::Lerp(DOT(x1##X##y1##Y##z1##Z##w1##W), \
                     DOT(x2##X##y2##Y##z2##Z##w2##W), \
                     tComponent))

        float outVal = Mathf::Lerp(Mathf::Lerp(Mathf::Lerp(LERP(min, min, min, min,
                                                                max, min, min, min,
                                                                t.x),
                                                           LERP(min, max, min, min,
                                                                max, max, min, min,
                                                                t.x),
                                                           t.y),
                                               Mathf::Lerp(LERP(min, min, max, min,
                                                                max, min, max, min,
                                                                t.x),
                                                           LERP(min, max, max, min,
                                                                max, max, max, min,
                                                                t.x),
                                                           t.y),
                                               t.z),
                                   Mathf::Lerp(Mathf::Lerp(LERP(min, min, min, max,
                                                                max, min, min, max,
                                                                t.x),
                                                           LERP(min, max, min, max,
                                                                max, max, min, max,
                                                                t.x),
                                                           t.y),
                                               Mathf::Lerp(LERP(min, min, max, max,
                                                                max, min, max, max,
                                                                t.x),
                                                           LERP(min, max, max, max,
                                                                max, max, max, max,
                                                                t.x),
                                                           t.y),
                                               t.z),
                                   t.w);
#undef LERP
#undef DOT

        return 0.5 + (0.5 * outVal);
    }

    struct WorleyResult { float Dists[2]; };
    WorleyResult Worley(float x, float variance, float(*distFunc)(float p1, float p2))
    {
        float cellThis = floorf(x),
              cellLess = cellThis - 1.0f,
              cellMore = cellThis + 1.0f;

        float minPos = 0.5f - variance,
              maxPos = 0.5f + variance;
#define POINT(var) (var + Mathf::Lerp(minPos, maxPos, Hash(var)))
        const int N_CELLS = 3;
        float distances[N_CELLS] = { distFunc(x, POINT(cellThis)),
                                     distFunc(x, POINT(cellLess)),
                                     distFunc(x, POINT(cellMore)) };
#undef POINT

        //Get the 2 closest distances.
        WorleyResult result;
        int exclude = -1;
        for (int i = 0; i < 2; ++i)
        {
            int smallest = 0;
            for (int j = 1; j < N_CELLS; ++j)
            {
                if (j != exclude && distances[j] < distances[smallest])
                    smallest = j;
            }
            result.Dists[i] = distances[smallest];
            exclude = smallest;
        }
        return result;
    }
    WorleyResult Worley(Vector2f v, Vector2f variance, float(*distFunc)(Vector2f p1, Vector2f p2))
    {
        const int N_CELLS = 3 * 3;

        float Xs[] = { 0.0f, floor(v.x), 0.0f },
              Ys[] = { 0.0f, floor(v.y), 0.0f };
        Xs[0] = Xs[1] - 1.0f;
        Xs[2] = Xs[1] + 1.0f;
        Ys[0] = Ys[1] - 1.0f;
        Ys[2] = Ys[1] + 1.0f;

        Vector2f cellPoses[N_CELLS];
        int cellI = 0;
        for (int x = -1; x < 2; ++x)
            for (int y = -1; y < 2; ++y)
                cellPoses[cellI++] = Vector2f(Xs[x + 1], Ys[y + 1]);

        Vector2f minPos = -variance + 0.5f,
                 maxPos = variance + 0.5f;
        float distances[N_CELLS];
        for (int i = 0; i < N_CELLS; ++i)
        {
            Vector2f hashPoint(Hash(cellPoses[i]), 0.0f);
            hashPoint.y = Hash(hashPoint.x);

            distances[i] = distFunc(cellPoses[i] + Vector2f::Lerp(minPos, maxPos, hashPoint),
                                    v);
        }

        //Get the 2 closest distances.
        WorleyResult result;
        int exclude = -1;
        for (int i = 0; i < 2; ++i)
        {
            int smallest = 0;
            for (int j = 1; j < N_CELLS; ++j)
            {
                if (j != exclude && distances[j] < distances[smallest])
                    smallest = j;
            }
            result.Dists[i] = distances[smallest];
            exclude = smallest;
        }
        return result;
    }
    WorleyResult Worley(Vector3f v, Vector3f variance, float(*distFunc)(Vector3f p1, Vector3f p2))
    {
        const int N_CELLS = 3 * 3 * 3;

        //Get the position of every cell.
        Vector3f cellPoses[N_CELLS];
        float Xs[] = { 0.0f, floorf(v.x), 0.0f },
              Ys[] = { 0.0f, floorf(v.y), 0.0f },
              Zs[] = { 0.0f, floorf(v.z), 0.0f };
        Xs[0] = Xs[1] - 1.0f;
        Xs[2] = Xs[1] + 1.0f;
        Ys[0] = Ys[1] - 1.0f;
        Ys[2] = Ys[1] + 1.0f;
        Zs[0] = Zs[1] - 1.0f;
        Zs[2] = Zs[1] + 1.0f;
        int cellI = 0;
        for (int x = -1; x < 2; ++x)
            for (int y = -1; y < 2; ++y)
                for (int z = -1; z < 2; ++z)
                    cellPoses[cellI++] = Vector3f(Xs[x + 1], Ys[y + 1], Zs[z + 1]);

        //Get the randomized point inside each cell and calculate its distance to the given point.
        Vector3f minPos = -variance + 0.5f,
                 maxPos = variance + 0.5f;
        float distances[N_CELLS];
        for (int i = 0; i < N_CELLS; ++i)
        {
            Vector3f hashPoint(Hash(cellPoses[i]), 0.0f, 0.0f);
            hashPoint.y = Hash(hashPoint.x);
            hashPoint.z = Hash(hashPoint.y);

            distances[i] = distFunc(cellPoses[i] + Vector3f::Lerp(minPos, maxPos, hashPoint),
                                    v);
        }

        //Get the 2 closest distances.
        WorleyResult result;
        int exclude = -1;
        for (int i = 0; i < 2; ++i)
        {
            int smallest = 0;
            for (int j = 1; j < N_CELLS; ++j)
            {
                if (j != exclude && distances[j] < distances[smallest])
                    smallest = j;
            }
            result.Dists[i] = distances[smallest];
            exclude = smallest;
        }
        return result;
    }
    WorleyResult Worley(Vector4f v, Vector4f variance, float(*distFunc)(Vector4f p1, Vector4f p2))
    {
        const int N_CELLS = 3 * 3 * 3 * 3;

        //Get the position of every cell.
        Vector4f cellPoses[N_CELLS];
        float Xs[] = { 0.0f, floorf(v.x), 0.0f },
              Ys[] = { 0.0f, floorf(v.y), 0.0f },
              Zs[] = { 0.0f, floorf(v.z), 0.0f },
              Ws[] = { 0.0f, floorf(v.w), 0.0f };
        Xs[0] = Xs[1] - 1.0f;
        Xs[2] = Xs[1] + 1.0f;
        Ys[0] = Ys[1] - 1.0f;
        Ys[2] = Ys[1] + 1.0f;
        Zs[0] = Zs[1] - 1.0f;
        Zs[2] = Zs[1] + 1.0f;
        Ws[0] = Ws[1] - 1.0f;
        Ws[2] = Ws[1] + 1.0f;
        int cellI = 0;
        for (int x = -1; x < 2; ++x)
            for (int y = -1; y < 2; ++y)
                for (int z = -1; z < 2; ++z)
                    for (int w = -1; w < 2; ++w)
                        cellPoses[cellI++] = Vector4f(Xs[x + 1], Ys[y + 1], Zs[z + 1], Ws[w + 1]);

        //Get the randomized point inside each cell and calculate its distance to the given point.
        Vector4f minPos = -variance + 0.5f,
                 maxPos = variance + 0.5f;
        float distances[N_CELLS];
        for (int i = 0; i < N_CELLS; ++i)
        {
            Vector4f hashPoint(Hash(cellPoses[i]), 0.0f, 0.0f, 0.0f);
            hashPoint.y = Hash(hashPoint.x);
            hashPoint.z = Hash(hashPoint.y);
            hashPoint.w = Hash(hashPoint.z);

            distances[i] = distFunc(cellPoses[i] + Vector4f::Lerp(minPos, maxPos, hashPoint),
                                    v);
        }

        //Get the 2 closest distances.
        WorleyResult result;
        int exclude = -1;
        for (int i = 0; i < 2; ++i)
        {
            int smallest = 0;
            for (int j = 1; j < N_CELLS; ++j)
            {
                if (j != exclude && distances[j] < distances[smallest])
                    smallest = j;
            }
            result.Dists[i] = distances[smallest];
            exclude = smallest;
        }
        return result;
    }

    float WorleyDist_StraightLine1(float p1, float p2) { return std::fabsf(p1 - p2); }
    float WorleyDist_StraightLine2(Vector2f p1, Vector2f p2) { return p1.Distance(p2); }
    float WorleyDist_StraightLine3(Vector3f p1, Vector3f p2) { return p1.Distance(p2); }
    float WorleyDist_StraightLine4(Vector4f p1, Vector4f p2) { return p1.Distance(p2); }
    float WorleyDist_Manhattan1(float p1, float p2) { return std::fabsf(p1 - p2); }
    float WorleyDist_Manhattan2(Vector2f p1, Vector2f p2) { return WorleyDist_Manhattan1(p1.x, p2.x) +
                                                                   WorleyDist_Manhattan1(p1.y, p2.y); }
    float WorleyDist_Manhattan3(Vector3f p1, Vector3f p2) { return WorleyDist_Manhattan1(p1.x, p2.x) +
                                                                   WorleyDist_Manhattan1(p1.y, p2.y) +
                                                                   WorleyDist_Manhattan1(p1.z, p2.z); }
    float WorleyDist_Manhattan4(Vector4f p1, Vector4f p2) { return WorleyDist_Manhattan1(p1.x, p2.x) +
                                                                   WorleyDist_Manhattan1(p1.y, p2.y) +
                                                                   WorleyDist_Manhattan1(p1.z, p2.z) +
                                                                   WorleyDist_Manhattan1(p1.w, p2.w); }
}
Vectorf MV_PerlinNoise::GetValue(const Ray& ray, FastRand& prng,
                                 const Shape* shpe,
                                 const Vertex* surface) const
{
    Vectorf x = X->GetValue(ray, prng, shpe, surface);
    switch (x.NValues)
    {
        case RT::Dimensions::One: return NoiseFuncs::Perlin(x.x);
        case RT::Dimensions::Two: return NoiseFuncs::Perlin((Vector2f)x);
        case RT::Dimensions::Three: return NoiseFuncs::Perlin((Vector3f)x);
        case RT::Dimensions::Four: return NoiseFuncs::Perlin((Vector4f)x);
        default: assert(false); return 0.5f;
    }
}
Vectorf MV_WorleyNoise::GetValue(const Ray& ray, FastRand& prng,
                                 const Shape* shpe,
                                 const Vertex* surface) const
{
    //Get the inputs.
    Vectorf x = X->GetValue(ray, prng, shpe, surface);
    Vectorf variance = Variance->GetValue(ray, prng, shpe, surface);
    Dimensions size = Max(x.NValues, variance.NValues);

    //Get the two closest distances.
    NoiseFuncs::WorleyResult result;
    switch (size)
    {
        case RT::Dimensions::One:
        {
            float(*distFunc)(float, float);
            switch (DistFunc)
            {
                case StraightLine: distFunc = NoiseFuncs::WorleyDist_StraightLine1; break;
                case Manhattan: distFunc = NoiseFuncs::WorleyDist_Manhattan1; break;
                default: assert(false); distFunc = NoiseFuncs::WorleyDist_StraightLine1; break;
            }
            result = NoiseFuncs::Worley(x.x, variance.x, distFunc);
        } break;

        case RT::Dimensions::Two:
        {
            float(*distFunc)(Vector2f, Vector2f);
            switch (DistFunc)
            {
                case StraightLine: distFunc = NoiseFuncs::WorleyDist_StraightLine2; break;
                case Manhattan: distFunc = NoiseFuncs::WorleyDist_Manhattan2; break;
                default: assert(false); distFunc = NoiseFuncs::WorleyDist_StraightLine2; break;
            }
            result = NoiseFuncs::Worley((Vector2f)x, (Vector2f)variance, distFunc);
        } break;

        case RT::Dimensions::Three:
        {
            float(*distFunc)(Vector3f, Vector3f);
            switch (DistFunc)
            {
                case StraightLine: distFunc = NoiseFuncs::WorleyDist_StraightLine3; break;
                case Manhattan: distFunc = NoiseFuncs::WorleyDist_Manhattan3; break;
                default: assert(false); distFunc = NoiseFuncs::WorleyDist_StraightLine3; break;
            }
            result = NoiseFuncs::Worley((Vector3f)x, (Vector3f)variance, distFunc);
        } break;

        case RT::Dimensions::Four:
        {
            float(*distFunc)(Vector4f, Vector4f);
            switch (DistFunc)
            {
                case StraightLine: distFunc = NoiseFuncs::WorleyDist_StraightLine4; break;
                case Manhattan: distFunc = NoiseFuncs::WorleyDist_Manhattan4; break;
                default: assert(false); distFunc = NoiseFuncs::WorleyDist_StraightLine4; break;
            }
            result = NoiseFuncs::Worley((Vector4f)x, (Vector4f)variance, distFunc);
        } break;

        default:
            assert(false);
            result.Dists[0] = 0.0f;
            result.Dists[1] = 0.0f;
    }

    //Modify the distances.
    float d1 = result.Dists[0],
          d2 = result.Dists[1];
    switch (DistParam1)
    {
        case Params::One: break;
        case Params::Zero: d1 = 0.0f; break;
        case Params::D1: d1 *= result.Dists[0]; break;
        case Params::D2: d1 *= result.Dists[1]; break;
        case Params::InvD1: d1 /= result.Dists[0]; break;
        case Params::InvD2: d1 /= result.Dists[1]; break;
        default: assert(false); break;
    }
    switch (DistParam2)
    {
        case Params::One: break;
        case Params::Zero: d2 = 0.0f; break;
        case Params::D1: d2 *= result.Dists[0]; break;
        case Params::D2: d2 *= result.Dists[1]; break;
        case Params::InvD1: d2 /= result.Dists[0]; break;
        case Params::InvD2: d2 /= result.Dists[1]; break;
        default: assert(false); break;
    }

    //Combine the distances.
    switch (DistCombineOp)
    {
        case Ops::Add: return d1 + d2;
        case Ops::Sub: return d1 - d2;
        case Ops::InvSub: return d2 - d1;
        case Ops::Mul: return d1 * d2;
        case Ops::Div: return d1 / d2;
        case Ops::InvDiv: return d2 / d1;
        default: assert(false); return 0.0f;
    }
}

void MV_WorleyNoise::WriteData(DataWriter& data, const String& namePrefix,
                               const ConstMaterialValueToID& idLookup) const
{
    MaterialValue::WriteData(data, namePrefix, idLookup);
    data.WriteByte((unsigned char)DistFunc, namePrefix + "DistFunc");
    data.WriteByte((unsigned char)DistCombineOp, namePrefix + "DistCombine");
    data.WriteByte((unsigned char)DistParam1, namePrefix + "DistParam1");
    data.WriteByte((unsigned char)DistParam2, namePrefix + "DistParam2");
}
void MV_WorleyNoise::ReadData(DataReader& data, const String& namePrefix,
                              NodeToChildIDs& childIDLookup)
{
    MaterialValue::ReadData(data, namePrefix, childIDLookup);

    unsigned char c;

    data.ReadByte(c, namePrefix + "DistFunc");
    DistFunc = (DistFuncs)c;

    data.ReadByte(c, namePrefix + "DistCombine");
    DistCombineOp = (Ops)c;

    data.ReadByte(c, namePrefix + "DistParam1");
    DistParam1 = (Params)c;

    data.ReadByte(c, namePrefix + "DistParam2");
    DistParam2 = (Params)c;
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
                 return Max(COMMA(V->GetNDims(), Normal->GetNDims())); );
IMPL_SIMPLE_FUNC(Refract,
                    auto v = GET_VAL(V);
                    auto normal = GET_VAL(Normal);
                    float index = (float)GET_VAL(IndexOfRefraction);
                    Vectorf result;
                    v.Refract(COMMA3(normal, index, result));
                    return result; ,
                 return Max(COMMA(V->GetNDims(), Normal->GetNDims())); );

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
                 return GET_VAL(Edge).OperateOn(COMMA([](COMMA(float edge, float x)) { return (x < edge ? 0.0f : 1.0f); },
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