#include "../Headers/Vectorf.h"

using namespace RT;


float Vectorf::Dot(const Vectorf& other) const
{
    Vectorf a = *this,
            b = other;
    float f = 0.0f;

    size_t endI = (a.NValues > b.NValues ? b.NValues : a.NValues);
    for (size_t i = 0; i < endI; ++i)
        f += a[i] * b[i];

    return f;
}


Vectorf::operator Vector2f() const
{
    switch (NValues)
    {
        case One: return Vector2f(x, x);
        default: return Vector2f(x, y);
    }
}
Vectorf::operator Vector3f() const
{
    switch (NValues)
    {
        case One: return Vector3f(x, x, x);
        case Two: return Vector3f(x, y, 0.0f);
        default: return Vector3f(x, y, z);
    }
}
Vectorf::operator Vector4f() const
{
    switch (NValues)
    {
        case One: return Vector4f(x, x, x, x);
        case Two: return Vector4f(x, y, 0.0f, 1.0f);
        case Three: return Vector4f(x, y, z, 1.0f);
        case Four: return Vector4f(x, y, z, w);
        default: assert(false); return Vector4f();
    }
}

Vectorf Vectorf::operator+(const Vectorf& other) const
{
    return OperateOn([](float f1, float f2) { return f1 + f2; },
                     other, 0.0f, 0.0f);
}
Vectorf Vectorf::operator*(const Vectorf& other) const
{
    return OperateOn([](float f1, float f2) { return f1 * f2; },
                     other, 1.0f, 1.0f);
}
Vectorf Vectorf::operator-(const Vectorf& other) const
{
    return OperateOn([](float f1, float f2) { return f1 - f2; },
                     other, 0.0f, 0.0f);
}
Vectorf Vectorf::operator/(const Vectorf& other) const
{
    return operator*(OperateOn([](float f) { return 1.0f / f; }));
}

float Vectorf::LengthSqr() const
{
    float f = 0.0f;
    for (size_t i = 0; i < NValues; ++i)
        f += (*this)[i];
    return f;
}

void Vectorf_Writable::WriteData(DataWriter& writer) const
{
    writer.WriteUInt((Dimensions)V.NValues, "Dimensions");
    writer.WriteFloat(V.x, "x");
    if ((int)V.NValues > 1)
    {
        writer.WriteFloat(V.y, "y");
        if ((int)V.NValues > 2)
        {
            writer.WriteFloat(V.z, "z");
            if ((int)V.NValues > 3)
            {
                writer.WriteFloat(V.w, "w");
            }
        }
    }
}
void Vectorf_Readable::ReadData(DataReader& reader)
{
    unsigned int nVals;
    reader.ReadUInt(nVals, "Dimensions");
    V.NValues = (Dimensions)nVals;

    reader.ReadFloat(V.x, "x");
    if ((int)V.NValues > 1)
    {
        reader.ReadFloat(V.y, "y");
        if ((int)V.NValues > 2)
        {
            reader.ReadFloat(V.z, "z");
            if ((int)V.NValues > 3)
            {
                reader.ReadFloat(V.w, "w");
            }
        }
    }
}