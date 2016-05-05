#pragma once

#include "Main.hpp"
#include "Tracer.h"
#include "DataSerialization.h"


//A way to calculate the color of the sky (i.e. a ray that didn't hit anything).
class RT_API SkyMaterial : public ISerializable
{
public:

    //Allocates a sky material on the heap with the given type-name.
    //Used in the serialization system.
    static SkyMaterial* Create(const std::string& typeName) { return GetFactory(typeName)(); }


    virtual ~SkyMaterial() { }


    //Gets the color of the sky using the given ray.
    virtual Vector3f GetColor(const Ray& ray) const = 0;

    virtual void ReadData(DataReader& data) override { }
    virtual void WriteData(DataWriter& data) const override { }


    //Gets this class's name as a string.
    //Don't override this manually! Use the "ADD_SKYMAT_REFLECTION_DATA" macros instead.
    //TODO: For this and Material and Shape, use const char* instead of std::string.
    virtual std::string GetTypeName() const = 0;


protected:

    typedef SkyMaterial*(*SkyMatFactory)();


    //Sets the factory to use for the given class name.
    //Makes the given class name visible to the serialization system.
    //NOTE: This should never be called manually; use the "ADD_SKYMAT_REFLECTION_DATA" macros.
    static void AddReflectionData(const std::string& typeName, SkyMatFactory factory);
    //Gets the factory to create a basic sky material of the given type name.
    //Used by the serialization system.
    static SkyMatFactory GetFactory(const std::string& typeName);
};



//Put this in a SkyMaterial sub-class's .h file to allow it to work with the serialization system.
//The extra arguments after "typeName" are the arguments to construct an instance of the class.
//The actual value of the constructor arguments isn't important.
#define ADD_SKYMAT_REFLECTION_DATA_H(className, typeName, ...) \
    public: \
        virtual std::string GetTypeName() const override { return #typeName; } \
    private: \
        struct _ReflectionDataInitializer \
        { \
        public: \
            _ReflectionDataInitializer() \
            { \
                AddReflectionData(#className, []() { return (SkyMaterial*)(new className(__VA_ARGS__)); }); \
            } \
        }; \
        static _ReflectionDataInitializer _RefDataInit;

//Put this in a SkyMaterial sub-class's .cpp file to allow it to work with the serialization system.
#define ADD_SKYMAT_REFLECTION_DATA_CPP(className) \
    className::_ReflectionDataInitializer className::_RefDataInit = className::_ReflectionDataInitializer();