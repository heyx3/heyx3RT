#pragma once

#include "Matrix4f.h"
#include "DataSerialization.h"


namespace RT
{
    struct RT_API Transform : public ISerializable
    {
    public:
    
        Transform(Vector3f pos = Vector3f(), Quaternion rot = Quaternion(),
                  Vector3f scale = Vector3f(1.0f, 1.0f, 1.0f));

    
        const Vector3f& GetPos() const { return pos; }
        const Vector3f& GetScale() const { return scale; }
        const Quaternion& GetRot() const { return rot; }

        Vector3f Point_WorldToLocal(Vector3f v) const { return toLocal.ApplyPoint(v); }
        Vector3f Dir_WorldToLocal(Vector3f d) const { return toLocal.ApplyVector(d); }
        Vector3f Normal_WorldToLocal(Vector3f n) const { return toLocal_InverseTranspose.ApplyVector(n); }

        Vector3f Point_LocalToWorld(Vector3f v) const { return toWorld.ApplyPoint(v); }
        Vector3f Dir_LocalToWorld(Vector3f d) const { return toWorld.ApplyVector(d); }
        Vector3f Normal_LocalToWorld(Vector3f n) const { return toWorld_InverseTranspose.ApplyVector(n); }

        const Matrix4f& GetMatToWorld() const { return toWorld; }
        const Matrix4f& GetMatToWorld_InverseTranspose() const { return toWorld_InverseTranspose; }
        const Matrix4f& GetMatToLocal() const { return toLocal; }
        const Matrix4f& GetWorldToLocal_InverseTranspose() const { return toLocal_InverseTranspose; }


        void SetPos(const Vector3f& pos);
        void MovePos(const Vector3f& amount) { SetPos(pos + amount); }

        void SetScale(const Vector3f& newScale);
        void ScaleBy(const Vector3f& amount) { SetScale(scale * amount); }
        void ScaleBy(float amount) { ScaleBy(Vector3f(amount, amount, amount)); }

        void SetRot(const Quaternion& newRot);
        void Rotate(const Quaternion& amount) { SetRot(Quaternion(rot, amount)); }


        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;


    private:

        Vector3f pos, scale;
        Quaternion rot;

        Matrix4f toWorld, toLocal;
        Matrix4f toWorld_InverseTranspose,
                 toLocal_InverseTranspose;

        void UpdateMats();
    };
}