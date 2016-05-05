#pragma once

#include "BoundingBox.h"
#include "Vertex.h"
#include "Transform.h"
#include "DataSerialization.h"



//An abstract class that represents some geometry.
class RT_API Shape : public ISerializable
{
public:

    //Allocates a shape on the heap with the given type-name.
    //Used in the serialization system.
    static Shape* Create(const std::string& typeName) { return GetFactory(typeName)(); }


    Transform Tr;


    Shape() { }
    virtual ~Shape() { }


    virtual void PrecalcData() { }

    virtual void GetBoundingBox(BoundingBox& outBox) const = 0;
    virtual bool CastRay(const Ray& ray, Vertex& outHit) const = 0;


    virtual void WriteData(DataWriter& writer) const override { writer.WriteDataStructure(Tr, "Transform"); }
    virtual void ReadData(DataReader& reader) override { reader.ReadDataStructure(Tr, "Transform"); }

    //This should not be overridden manually! Use the "ADD_SHAPE_REFLECTION_DATA" macros instead.
    virtual std::string GetTypeName() const = 0;


protected:

    typedef Shape*(*ShapeFactory)();

    //Sets the factory to use for the given class name.
    //Makes the given class name visible to the serialization system.
    //NOTE: This should never be called manually; use the "ADD_SHAPE_REFLECTION_DATA" macros.
    static void AddReflectionData(const std::string& typeName, ShapeFactory factory);
    //Gets the factory to create a basic sky material of the given type name.
    //Used by the serialization system.
    static ShapeFactory GetFactory(const std::string& typeName);
};



//Put this in a Shape sub-class's .h file to allow it to work with the serialization system.
//The extra arguments after "className" are the arguments to construct an instance of the class.
//The actual value of the constructor arguments isn't important.
#define ADD_SHAPE_REFLECTION_DATA_H(className, ...) \
    public: \
        virtual std::string GetTypeName() const override { return #className; } \
    private: \
        struct _ReflectionDataInitializer \
        { \
        public: \
            _ReflectionDataInitializer() \
            { \
                AddReflectionData(#className, []() { return (Shape*)(new className(__VA_ARGS__)); }); \
            } \
        }; \
        static _ReflectionDataInitializer _RefDataInit;

//Put this in a Shape sub-class's .cpp file to allow it to work with the serialization system.
#define ADD_SHAPE_REFLECTION_DATA_CPP(className) \
    className::_ReflectionDataInitializer className::_RefDataInit = className::_ReflectionDataInitializer();