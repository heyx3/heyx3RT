#pragma once

#include "Vectors.h"


struct RT_API Quaternion
{
public:

    float x, y, z, w;


    Quaternion() : x(0.0f), y(0.0f), z(0.0f), w(1.0f) { }
    Quaternion(float _x, float _y, float _z, float _w) : x(_x), y(_y), z(_z), w(_w) { }

    Quaternion(const Vector3f& axis, float angleRadians);
    Quaternion(const Quaternion& firstRot, const Quaternion& secondRot);
    Quaternion(const Vector3f& from, const Vector3f& to);


    Quaternion& operator=(const Quaternion& other) { x = other.x; y = other.y; z = other.z; w = other.w; return *this; }

    Quaternion operator-() const { return Quaternion(-x, -y, -z, w); }
    
    Quaternion operator+(const Quaternion& q) const { return Quaternion(x + q.x, y + q.y, z + q.z, w + q.w); }
    Quaternion operator-(const Quaternion& q) const { return Quaternion(x - q.x, y - q.y, z - q.z, w - q.w); }
    Quaternion operator*(float scale) const { return Quaternion(x * scale, y * scale, z * scale, w * scale); }
    Quaternion operator/(float invScale) const { return Quaternion(x / invScale, y / invScale, z / invScale, w / invScale); }


    Vector3f Rotate(const Vector3f& v) const;

    float Dot(const Quaternion& other) const { return (x * other.x) + (y * other.y) + (z * other.z) + (w * other.w); }
    Quaternion Normalize() const { float invLen = 1.0f / sqrt(Dot(*this)); return Quaternion(x * invLen, y * invLen, z * invLen, w * invLen); }

    void GetAxisAngle(Vector3f& outAxis, float& outAngleRadians) const;
    //Gets the axis/angle rotation represented by this Quaternion as a 4D vector.
    //The X/Y/Z is the axis of rotation, and the W is the angle in radians.
    inline Vector4f GetAxisAngle() const { Vector3f ax; float ang; GetAxisAngle(ax, ang); return Vector4f(ax.x, ax.y, ax.z, ang); }
};