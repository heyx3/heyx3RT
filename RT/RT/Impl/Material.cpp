#include "../Headers/Material.h"

#include "../Headers/Matrix4f.h"


Vector3f Material::TangentSpaceToWorldSpace(const Vector3f& tangentSpaceNormal,
                                            const Vector3f& worldNormal,
                                            const Vector3f& worldTangent,
                                            const Vector3f& worldBitangent)
{
    Matrix4f tempMat, transfMat;
    tempMat.SetAsOrientation(worldNormal, worldBitangent, worldTangent);
    tempMat.GetTranspose(transfMat);

    return transfMat.ApplyVector(tangentSpaceNormal);
}