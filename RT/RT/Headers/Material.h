#pragma once

#include "FastRand.h"
#include "Tracer.h"
#include "DataSerialization.h"


//A way to calculate the color of a surface.
class RT_API Material : public ISerializable
{
public:

    //Converts a tangent-space normal to world space.
    //Used for normal-mapping.
    static Vector3f TangentSpaceToWorldSpace(const Vector3f& tangentSpaceNormal,
                                             const Vector3f& worldNormal,
                                             const Vector3f& worldTangent,
                                             const Vector3f& worldBitangent);


    virtual ~Material() { }


    //Scatters the given incoming ray after it hits the given surface point of this material.
    //Also potentially attenuates the ray.
    //Returns "true" if the ray scattered, or "false" if the ray was absorbed.
    virtual bool Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                         FastRand& prng, Vector3f& attenuation, Ray& rOut) const = 0;

    virtual void ReadData(DataReader& data) override { }
    virtual void WriteData(DataWriter& data) const override { }


    //Put this in a Material class's .h file to allow it to work correctly with serializers/deserializers.
    #define ADD_MATERIAL_REFLECTION_DATA_H(className) \
        public: \
            virtual std::string GetTypeName() const override { return #className; } \
        private: \
            struct _ReflectionDataInitializer \
            { \
            public: \
                _ReflectionDataInitializer(); \
            }; \
            static _ReflectionDataInitializer _RefDataInit;
    //Put this in a Material class's .h file to allow it to work correctly with serializers/deserializers.
    //The extra arguments after "className" are the arguments for the class's constructor.
    //They don't have to be sane arguments; the instance won't be used for anything except
    #define ADD_MATERIAL_REFLECTION_DATA_CPP(className, ...) \
        className::_ReflectionDataInitializer::_ReflectionDataInitializer() \
        { \
            \
        } \
        className::_ReflectionDataInitializer _RefDataInit = className::_ReflectionDataInitializer;

    //Gets this class's name as a string.
    //Don't override this manually! Use ADD_MATERIAL_REFLECTION_DATA_H instead.
    virtual std::string GetTypeName() const = 0;


protected:


};