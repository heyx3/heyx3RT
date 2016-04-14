#include "../Headers/Camera.h"


Camera::Camera(const Vector3f& pos, const Vector3f& _forward, const Vector3f& _upward,
               float fovRadians, float widthOverHeight, float _zNear, float _zFar,
               bool lockUp, float closestDotVariance)
    : Pos(pos), forward(_forward), up(_upward),
      LockUp(lockUp), ClosestDotVariance(closestDotVariance), FOVRadians(fovRadians),
      WidthOverHeight(widthOverHeight), zNear(_zNear), zFar(_zFar)
{
    UpdateSideways();
}
Camera::Camera(const Vector3f& pos, const Vector3f& _forward, const Vector3f& _upward,
               const Vector3f& minOrthoBounds, const Vector3f& maxOrthoBounds,
               bool lockUp, float closestDotVariance)
    : Pos(pos), forward(_forward), up(_upward),
      LockUp(lockUp), ClosestDotVariance(closestDotVariance),
      MinOrthoBounds(minOrthoBounds), MaxOrthoBounds(maxOrthoBounds)
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
void Camera::GetViewTransform(Matrix4f& outM) const
{
    outM.SetAsView(Pos, forward, up, sideways);
}
void Camera::GetPerspectiveProjection(Matrix4f& outM) const
{
    outM.SetAsPerspectiveProjection(FOVRadians, WidthOverHeight, zNear, zFar);
}
void Camera::GetOrthoProjection(Matrix4f& outM) const
{
    outM.SetAsOrthoProjection(MinOrthoBounds + Pos, MaxOrthoBounds + Pos);
}