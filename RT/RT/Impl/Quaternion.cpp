#include "../Headers/Quaternion.h"

using namespace RT;



Quaternion::Quaternion(const Vector3f& axis, float angleRadians)
{
    float halfAngle = angleRadians * 0.5f,
            sinHalf = sin(halfAngle),
            cosHalf = cos(halfAngle);

    x = axis.x * sinHalf;
    y = axis.y * sinHalf;
    z = axis.z * sinHalf;
    w = cosHalf;
}
Quaternion::Quaternion(const Quaternion& firstRot, const Quaternion& secondRot)
{
    x = (secondRot.x * firstRot.w) + (secondRot.w * firstRot.x) + (secondRot.y * firstRot.z) - (secondRot.z * firstRot.y);
    y = (secondRot.y * firstRot.w) + (secondRot.w * firstRot.y) + (secondRot.z * firstRot.x) - (secondRot.x * firstRot.z);
    z = (secondRot.z * firstRot.w) + (secondRot.w * firstRot.z) + (secondRot.x * firstRot.y) - (secondRot.y * firstRot.x);
    w = (secondRot.w * firstRot.w) - (secondRot.x * firstRot.x) - (secondRot.y * firstRot.y) - (secondRot.z * firstRot.z);
}
Quaternion::Quaternion(const Vector3f& from, const Vector3f& to)
{
    float dotted = from.Dot(to);
    if (1.0f - dotted < 0.0001f)
    {
        *this = Quaternion();
    }
    else if (1.0f + dotted < 0.0001f)
    {
        //Get an arbitrary perpendicular axis to rotate around.
        Vector3f axis = (fabs(from.x) < 1.0f) ?
                            Vector3f(1.0f, 0.0f, 0.0f) :
                            Vector3f(0.0f, 1.0f, 0.0f);
        axis = axis.Cross(from).Normalize();
        *this = Quaternion(axis, (float)M_PI);
    }
    else
    {
        Vector3f norm = from.Cross(to);
        *this = Quaternion(norm.x, norm.y, norm.z, (1.0f + dotted)).Normalize();
    }
}

Vector3f Quaternion::Rotate(const Vector3f& v) const
{
    Quaternion conjugate = -(*this);
        
    Quaternion tempW((w * v.x) + (y * v.z) - (z * v.y),
                        (w * v.y) + (z * v.x) - (x * v.z),
                        (w * v.z) + (x * v.y) - (y * v.x),
                        -(x * v.x) - (y * v.y) - (z * v.z));
    Quaternion finalW(conjugate, tempW);

    return Vector3f(finalW.x, finalW.y, finalW.z);
}

void Quaternion::GetAxisAngle(Vector3f& outAxis, float& outAngle) const
{
    float halfAngle = acosf(w);
    float denom = 1.0f / sinf(halfAngle);
    outAxis = Vector3f(x * denom, y * denom, z * denom);
    outAngle = 2.0f * halfAngle;
}