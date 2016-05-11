#pragma once

#include "Vectors.h"


struct RT_API Ray
{
public:

    Ray() { }
    Ray(const Vector3f& _pos, const Vector3f& _dir) : pos(_pos), dir(_dir) { }


    const Vector3f& GetPos() const { return pos; }
    const Vector3f& GetDir() const { return dir; }

    Vector3f GetPos(float t) const { return pos + (dir * t); }


    void SetPos(const Vector3f& newPos) { pos = newPos; }
    void SetDir(const Vector3f& newDir) { dir = newDir; }


private:

    Vector3f pos, dir;
};