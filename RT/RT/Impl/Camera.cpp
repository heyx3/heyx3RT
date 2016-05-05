#include "../Headers/Camera.h"


Camera::Camera(const Vector3f& pos, const Vector3f& _forward, const Vector3f& _upward,
               float widthOverHeight, bool lockUp, float closestDotVariance)
    : Pos(pos), forward(_forward), up(_upward),
      LockUp(lockUp), ClosestDotVariance(closestDotVariance),
      WidthOverHeight(widthOverHeight)
{
    UpdateSideways();
}


void Camera::SetRotation(const Vector3f& newForward, const Vector3f& newUp)
{
    forward = newForward;
    up = newUp;
    UpdateSideways();
}
void Camera::Rotate(const Quaternion& quat)
{
    forward = quat.Rotate(forward).Normalize();
    up = quat.Rotate(up).Normalize();
    UpdateSideways();
}
void Camera::AddPitch(float radians)
{
    Quaternion rot(sideways, radians);
    Vector3f newForward = rot.Rotate(forward).Normalize();

    if (!LockUp)
    {
        forward = newForward;
        up = rot.Rotate(up).Normalize();
        UpdateSideways();
    }
    else
    {
        float dot = newForward.Dot(up);
        float variance = fabs(fabs(dot) - 1.0f);

        if (variance >= ClosestDotVariance)
        {
            forward = newForward;
            UpdateSideways();
        }
    }
}
void Camera::AddYaw(float radians)
{
    Quaternion rot(up, radians);
    forward = rot.Rotate(forward).Normalize();
}
void Camera::AddRoll(float radians)
{
    up = forward.Cross(sideways);
    
    Quaternion rot(forward, radians);
    up = rot.Rotate(up).Normalize();
    UpdateSideways();
}

void Camera::WriteData(DataWriter& writer) const
{
    writer.WriteVec3f(Pos, "Pos");
    writer.WriteBool(LockUp, "LockUp");
    if (LockUp)
    {
        writer.WriteFloat(ClosestDotVariance, "ClosestDotVariance");
    }
    writer.WriteFloat(WidthOverHeight, "WidthOverHeight");
    writer.WriteVec3f(forward, "Forward");
    writer.WriteVec3f(up, "Up");
    writer.WriteVec3f(sideways, "Sideways");
}
void Camera::ReadData(DataReader& reader)
{
    reader.ReadVec3f(Pos, "Pos");
    reader.ReadBool(LockUp, "LockUp");
    if (LockUp)
    {
        reader.ReadFloat(ClosestDotVariance, "ClosestDotVariance");
    }
    reader.ReadFloat(WidthOverHeight, "WidthOverHeight");
    reader.ReadVec3f(forward, "Forward");
    reader.ReadVec3f(up, "Up");
    reader.ReadVec3f(sideways, "Sideways");

    forward = forward.Normalize();
    up = up.Normalize();
    sideways = sideways.Normalize();
}