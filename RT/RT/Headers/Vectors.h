#pragma once

#define _USE_MATH_DEFINES
#include <math.h>

#include "Main.hpp"


struct RT_API Vector2f
{
public:

    static Vector2f Lerp(const Vector2f& a, const Vector2f& b, float t)
    {
        return Vector2f(a.x + (t * (b.x - a.x)),
                        a.y + (t * (b.y - a.y)));
    }

    static Vector2f X() { return Vector2f(1.0f, 0.0f); }
    static Vector2f Y() { return Vector2f(0.0f, 1.0f); }


    float x, y;


    Vector2f() : x(0.0f), y(0.0f) { }
    Vector2f(float _x, float _y) : x(_x), y(_y) { }


    float Dot(const Vector2f& rhs) const
    {
        return (x * rhs.x) + (y * rhs.y);
    }

    float LengthSqr() const { return Dot(*this); }
    float Length() const { return sqrt(LengthSqr()); }

    float DistanceSqr(const Vector2f& other) const { return (*this - other).LengthSqr(); }
    float Distance(const Vector2f& other) const { return sqrtf(DistanceSqr(other)); }

    Vector2f Normalize() const
    {
        float invLen = 1.0f / Length();
        return *this * invLen;
    }

    Vector2f Reflect(const Vector2f& normal) const { return (*this) - (normal * 2.0f * (normal).Dot(*this)); }

    Vector2f Reciprocal() const { return Vector2f(1.0f / x, 1.0f / y); }

    Vector2f GetPerp() const { return Vector2f(-y, x); }


    bool operator==(const Vector2f& other) const { return x == other.x && y == other.y; }
    bool operator!=(const Vector2f& other) const { return x != other.x || y != other.y; }
    bool operator==(float f) const { return operator==(Vector2f(f, f)); }
    bool operator!=(float f) const { return operator!=(Vector2f(f, f)); }

    float& operator[](size_t i) { return (i == 0 ? x : y); }
    const float& operator[](size_t i) const { return (i == 0 ? x : y); }

    //Basic component-wise operations on vectors/floats:
#define OP3(symbol) Vector2f operator##symbol(const Vector2f& other) const { return Vector2f(x symbol other.x, y symbol other.y); }
#define OP1(symbol) Vector2f operator##symbol(float f) const { return Vector2f(x symbol f, y symbol f); }
#define OPT3(symbol) Vector2f& operator##symbol(const Vector2f& other) { x symbol other.x; y symbol other.y; return *this; }
#define OPT1(symbol) Vector2f& operator##symbol(float f) { x symbol f; y symbol f; return *this; }
    OP3(+) OP3(-) OP3(*) OP3(/ )
    OP1(+) OP1(-) OP1(*) OP1(/ )
    OPT3(+= ) OPT3(-= ) OPT3(*= ) OPT3(/= ) OPT3(= )
    OPT1(+= ) OPT1(-= ) OPT1(*= ) OPT1(/= ) OPT1(= )
#undef OP3
#undef OP1
#undef OPT3
#undef OPT1
    Vector2f operator-() const { return Vector2f(-x, -y); }
};


struct RT_API Vector3f
{
public:

    static Vector3f Lerp(const Vector3f& a, const Vector3f& b, float t)
    {
        return Vector3f(a.x + (t * (b.x - a.x)),
                        a.y + (t * (b.y - a.y)),
                        a.z + (t * (b.z - a.z)));
    }
    
    static Vector3f X() { return Vector3f(1.0f, 0.0f, 0.0f); }
    static Vector3f Y() { return Vector3f(0.0f, 1.0f, 0.0f); }
    static Vector3f Z() { return Vector3f(0.0f, 0.0f, 1.0f); }

    static Vector3f Forward() { return Z(); }
    static Vector3f Up() { return Y(); }
    static Vector3f Sideways() { return X(); }


	float x, y, z;


	Vector3f()                             : x(0.0f), y(0.0f), z(0.0f) { }
	Vector3f(float _x, float _y, float _z) : x(_x), y(_y), z(_z) { }
    

    float Dot(const Vector3f& rhs) const
    {
        return (x * rhs.x) + (y * rhs.y) + (z * rhs.z);
    }
    Vector3f Cross(const Vector3f& rhs) const
    {
        return Vector3f((y * rhs.z) - (z * rhs.y),
                        (z * rhs.x) - (x * rhs.z),
                        (x * rhs.y) - (y * rhs.x));
    }

    float LengthSqr() const { return Dot(*this); }
    float Length() const { return sqrt(LengthSqr()); }

    float DistanceSqr(const Vector3f& other) const { return (*this - other).LengthSqr(); }
    float Distance(const Vector3f& other) const { return sqrtf(DistanceSqr(other)); }

    Vector3f Normalize() const
    {
        float invLen = 1.0f / Length();
        return *this * invLen;
    }

    Vector3f Reflect(const Vector3f& normal) const { return (*this) - (normal * 2.0f * (normal).Dot(*this)); }
    Vector3f Refract(const Vector3f& normal, float invIndex) const
    {
        Vector3f crossed = normal.Cross(*this);
        return ((normal.Cross(-crossed)) * invIndex) -
               (normal * sqrt(1.0f - (invIndex * invIndex * crossed.Dot(crossed))));
    }

    Vector3f Reciprocal() const { return Vector3f(1.0f / x, 1.0f / y, 1.0f / z); }

    void GetOrthoBasis(Vector3f& outTangent, Vector3f& outBitangent) const
    {
        outTangent = (fabs(x) == 1.0f ? Z() : X());
        outBitangent = Cross(outTangent).Normalize();
        outTangent = Cross(outBitangent);
    }


    bool operator==(const Vector3f& other) const { return x == other.x && y == other.y && z == other.z; }
    bool operator!=(const Vector3f& other) const { return x != other.x || y != other.y || z != other.z; }
    bool operator==(float f) const { return operator==(Vector3f(f, f, f)); }
    bool operator!=(float f) const { return operator!=(Vector3f(f, f, f)); }
    
    float& operator[](int i) { return (i == 0 ? x : (i == 1 ? y : z)); }
    const float& operator[](int i) const { return (i == 0 ? x : (i == 1 ? y : z)); }

    //Basic component-wise operations on vectors/floats:
#define OP3(symbol) Vector3f operator##symbol(const Vector3f& other) const { return Vector3f(x symbol other.x, y symbol other.y, z symbol other.z); }
#define OP1(symbol) Vector3f operator##symbol(float f) const { return Vector3f(x symbol f, y symbol f, z symbol f); }
#define OPT3(symbol) Vector3f& operator##symbol(const Vector3f& other) { x symbol other.x; y symbol other.y; z symbol other.z; return *this; }
#define OPT1(symbol) Vector3f& operator##symbol(float f) { x symbol f; y symbol f; z symbol f; return *this; }
    OP3(+) OP3(-) OP3(*) OP3(/)
    OP1(+) OP1(-) OP1(*) OP1(/)
    OPT3(+=) OPT3(-=) OPT3(*=) OPT3(/=) OPT3(=)
    OPT1(+=) OPT1(-=) OPT1(*=) OPT1(/=) OPT1(=)
#undef OP3
#undef OP1
#undef OPT3
#undef OPT1
    Vector3f operator-() const { return Vector3f(-x, -y, -z); }
};


struct RT_API Vector4f
{
public:

    static Vector4f Lerp(const Vector4f& a, const Vector4f& b, float t)
    {
        return Vector4f(a.x + (t * (b.x - a.x)),
                        a.y + (t * (b.y - a.y)),
                        a.z + (t * (b.z - a.z)),
                        a.w + (t * (b.w - a.w)));
    }
    
    static Vector4f X() { return Vector4f(1.0f, 0.0f, 0.0f, 0.0f); }
    static Vector4f Y() { return Vector4f(0.0f, 1.0f, 0.0f, 0.0f); }
    static Vector4f Z() { return Vector4f(0.0f, 0.0f, 1.0f, 0.0f); }
    static Vector4f W() { return Vector4f(0.0f, 0.0f, 0.0f, 1.0f); }


	float x, y, z, w;


    Vector4f()                                       : x(0.0f), y(0.0f), z(0.0f), w(0.0f) { }
    Vector4f(float _x, float _y, float _z, float _w) : x(_x), y(_y), z(_z), w(_w) { }
    

    float Dot(const Vector4f& rhs) const
    {
        return (x * rhs.x) + (y * rhs.y) + (z * rhs.z) * (w * rhs.w);
    }

    float LengthSqr() const { return Dot(*this); }
    float Length() const { return sqrt(LengthSqr()); }

    float DistanceSqr(const Vector4f& other) const { return (*this - other).LengthSqr(); }
    float Distance(const Vector4f& other) const { return sqrtf(DistanceSqr(other)); }

    Vector4f Normalize() const
    {
        float invLen = 1.0f / Length();
        return *this * invLen;
    }

    Vector4f Reflect(const Vector4f& normal) const { return (*this) - (normal * 2.0f * (normal).Dot(*this)); }

    Vector4f Reciprocal() const { return Vector4f(1.0f / x, 1.0f / y, 1.0f / z, 1.0f / w); }


    bool operator==(const Vector4f& other) const { return x == other.x && y == other.y && z == other.z && w == other.w; }
    bool operator!=(const Vector4f& other) const { return x != other.x || y != other.y || z != other.z || w != other.w; }
    bool operator==(float f) const { return operator==(Vector4f(f, f, f, f)); }
    bool operator!=(float f) const { return operator!=(Vector4f(f, f, f, f)); }
    
    float& operator[](int i) { return (i == 0 ? x : (i == 1 ? y : (i == 2 ? z : w))); }
    const float& operator[](int i) const { return (i == 0 ? x : (i == 1 ? y : (i == 2 ? z : w))); }

    //Basic component-wise operations on vectors/floats:
#define OP3(symbol) Vector4f operator##symbol(const Vector4f& other) const { return Vector4f(x symbol other.x, y symbol other.y, z symbol other.z, w symbol other.w); }
#define OP1(symbol) Vector4f operator##symbol(float f) const { return Vector4f(x symbol f, y symbol f, z symbol f, w symbol f); }
#define OPT3(symbol) Vector4f& operator##symbol(const Vector4f& other) { x symbol other.x; y symbol other.y; z symbol other.z; w symbol other.w; return *this; }
#define OPT1(symbol) Vector4f& operator##symbol(float f) { x symbol f; y symbol f; z symbol f; w symbol f; return *this; }
    OP3(+) OP3(-) OP3(*) OP3(/)
    OP1(+) OP1(-) OP1(*) OP1(/)
    OPT3(+=) OPT3(-=) OPT3(*=) OPT3(/=) OPT3(=)
    OPT1(+=) OPT1(-=) OPT1(*=) OPT1(/=) OPT1(=)
#undef OP3
#undef OP1
#undef OPT3
#undef OPT1
        Vector4f operator-() const { return Vector4f(-x, -y, -z, -w); }
};