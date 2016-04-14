#pragma once

#include "Vector3f.h"


//A single point in a mesh triangle.
struct RT_API Vertex
{
public:

    Vector3f Pos, Normal, Tangent, Bitangent;
    float UV[2];
};