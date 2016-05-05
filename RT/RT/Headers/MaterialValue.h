#pragma once


#include "Main.hpp"
#include "Shape.h"
#include "Ray.h"
#include "DataSerialization.h"
#include "UniquePtr.h"
#include "FastRand.h"
#include "Vectorf.h"


//A value calculated based on a ray collision with an object.
//For example, a texture lookup based on the collision point's UV.
//Outputs anything from a 1D vector to a 4D vector.
class RT_API MaterialValue : public ISerializable
{
public:

    typedef UniquePtr<MaterialValue> Ptr;


    static Ptr Create(const std::string& typeName)
    {    
        return Ptr(GetFactory(typeName)());
    }

    static void WriteValue(const Ptr& mv, DataWriter& writer, const std::string& name)
    {
        writer.WriteString(mv->GetTypeName(), name + "Type");
        writer.WriteDataStructure(*mv, name + "Value");
    }
    static void ReadValue(Ptr& outMV, DataReader& reader, const std::string name)
    {
        std::string typeName;
        reader.ReadString(typeName, name + "Type");
        outMV = Create(typeName).Release();
        reader.ReadDataStructure(*outMV, name + "Value");
    }


    virtual ~MaterialValue() { }


    virtual Dimensions GetNDims() const = 0;

    //Gets a value with between 1 and 4 dimensions.
    //Note that the shape and vertex may be null if nothing was hit by the ray.
    virtual Vectorf GetValue(const Ray& ray, FastRand& prng,
                             const Shape* shpe = nullptr,
                             const Vertex* surface = nullptr) const = 0;


    //Don't override this manually! Use the "ADD_MVAL_REFLECTION_DATA_H" macros instead.
    virtual std::string GetTypeName() const = 0;

    virtual void ReadData(DataReader& data) override { }
    virtual void WriteData(DataWriter& data) const override { }


protected:
    

    typedef MaterialValue*(*MVFactory)();


    //Sets the factory to use for the given class name.
    //Makes the given class name visible to the serialization system.
    //NOTE: This should never be called manually; use the "ADD_MVAL_REFLECTION_DATA" macros.
    static void AddReflectionData(const std::string& typeName, MVFactory factory);
    //Gets the factory to create a MaterialValue with the given type-name (from "GetTypeName()").
    //Used by the serialization system.
    static MVFactory GetFactory(const std::string& typeName);
};


//TODO: Add support for three-deep (or more) class hierarchies by adding the class name onto the types/variables in all the various reflection data macros.


//Put this in a MaterialValue sub-class's .h file to allow it to work with the serialization system.
//The extra arguments after "className" are the arguments to construct an instance of the class.
//The actual value of the constructor arguments isn't important.
#define ADD_MVAL_REFLECTION_DATA_H(className, typeName, ...) \
    public: \
        virtual std::string GetTypeName() const override { return #typeName; } \
    private: \
        struct _ReflectionDataInitializer \
        { \
        public: \
            _ReflectionDataInitializer() \
            { \
                AddReflectionData(#className, []() { return (MaterialValue*)(new className(__VA_ARGS__)); }); \
            } \
        }; \
        static _ReflectionDataInitializer _RefDataInit;

//Put this in a MaterialValue sub-class's .cpp file to allow it to work with the serialization system.
#define ADD_MVAL_REFLECTION_DATA_CPP(className) \
    className::_ReflectionDataInitializer className::_RefDataInit = className::_ReflectionDataInitializer();