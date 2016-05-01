#pragma once


#include "Main.hpp"
#include "Vertex.h"
#include "Ray.h"
#include "DataSerialization.h"


//A way of getting a value (often a color) of a surface hit by a ray.
class RT_API MaterialValue : public ISerializable
{
public:

    virtual ~MaterialValue() { }


    virtual Vector3f GetValue(const Vertex& surface, const Ray& ray) const = 0;

    virtual void ReadData(DataReader& data) override { }
    virtual void WriteData(DataWriter& data) const override { }
};