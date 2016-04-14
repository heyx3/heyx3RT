#include "../Headers/Transform.h"


Transform::Transform(Vector3f _pos, Quaternion _rot, Vector3f _scale)
    : pos(_pos), scale(_scale), rot(_rot)
{
    UpdateMats();
}

void Transform::SetPos(const Vector3f& _pos)
{
    pos = _pos;
    UpdateMats();
}
void Transform::SetScale(const Vector3f& _scale)
{
    scale = _scale;
    UpdateMats();
}
void Transform::SetRot(const Quaternion& newRot)
{
    rot = newRot;
    UpdateMats();
}

void Transform::UpdateMats()
{
    Matrix4f transM, rotM, scaleM;
    transM.SetAsTranslate(pos);
    rotM.SetAsRotate(rot);
    scaleM.SetAsScale(scale);

    toWorld = Matrix4f(Matrix4f(scaleM, rotM), transM);
    toWorld.GetInverse(toLocal);
}