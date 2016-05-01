#pragma once

#include "Matrix4f.h"
#include "DataSerialization.h"


struct RT_API Transform : public ISerializable
{
public:
    
    Transform(Vector3f pos = Vector3f(), Quaternion rot = Quaternion(),
              Vector3f scale = Vector3f(1.0f, 1.0f, 1.0f));

    
    const Vector3f& GetPos() const { return pos; }
    const Vector3f& GetScale() const { return scale; }
    const Quaternion& GetRot() const { return rot; }

    const Matrix4f& GetMatToWorld() const { return toWorld; }
    const Matrix4f& GetMatToLocal() const { return toLocal; }


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

    void UpdateMats();
};