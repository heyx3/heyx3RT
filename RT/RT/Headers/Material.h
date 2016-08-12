#pragma once

#include "FastRand.h"
#include "Shape.h"
#include "DataSerialization.h"


namespace RT
{
    //A way to calculate the color of a surface.
    class RT_API Material : public ISerializable
    {
    public:

        //Allocates a material on the heap with the given type-name. Used in the serialization system.
        static Material* Create(const String& typeName) { return GetFactory(typeName)(); }

        //Writes out the data for the given Material.
        static void WriteValue(const Material& mat, DataWriter& writer, const String& name)
        {
            writer.WriteString(mat.GetTypeName(), name + "Type");
            writer.WriteDataStructure(mat, name + "Value");
        }
        //Reads in the given Material.
        //Note that the code calling this function is responsible for "delete"-ing the new material.
        static void ReadValue(Material*& outMat, DataReader& reader, const String& name)
        {
            String typeName;
            reader.ReadString(typeName, name + "Type");
            outMat = Create(typeName);
            reader.ReadDataStructure(*outMat, name + "Value");
        }

        //Converts a tangent-space normal to world space.
        //Used for normal-mapping.
        static Vector3f TangentSpaceToWorldSpace(const Vector3f& tangentSpaceNormal,
                                                 const Vector3f& worldNormal,
                                                 const Vector3f& worldTangent,
                                                 const Vector3f& worldBitangent);


        //Scatters the given incoming ray after it hits the given surface point of this material.
        //Also potentially attenuates the ray.
        //Returns "true" if the ray scattered, or "false" if the ray was absorbed.
        virtual bool Scatter(const Ray& rIn, const Vertex& surface, const Shape& shpe,
                             FastRand& prng, Vector3f& attenuation, Ray& rOut) const = 0;

        virtual void ReadData(DataReader& data) override { }
        virtual void WriteData(DataWriter& data) const override { }

        //Gets this class's name as a string.
        //Don't override this manually! Use the "ADD_MATERIAL_REFLECTION_DATA" macros instead.
        virtual String GetTypeName() const = 0;


    protected:

        typedef Material*(*MaterialFactory)();


        //Sets the factory to use for the given class name.
        //Makes the given class name visible to the serialization system.
        //NOTE: This should never be called manually; use the "ADD_MATERIAL_REFLECTION_DATA" macros.
        static void AddReflectionData(const String& typeName, MaterialFactory factory);
        //Gets the factory to create a basic material of the given type name.
        //Used by the serialization system.
        static MaterialFactory GetFactory(const String& typeName);
    };
}


//Put this in a Material sub-class's .h file to allow it to work with the serialization system.
//The extra arguments after "className" are the arguments to construct an instance of the class.
//The actual value of the constructor arguments isn't important.
#define ADD_MATERIAL_REFLECTION_DATA_H(className, typeName, ...) \
    public: \
        virtual String GetTypeName() const override { return #typeName; } \
    private: \
        struct RT_API _ReflectionDataInitializer \
        { \
        public: \
            _ReflectionDataInitializer() \
            { \
                AddReflectionData(#typeName, []() { return (Material*)(new className(__VA_ARGS__)); }); \
            } \
        }; \
        static _ReflectionDataInitializer _RefDataInit;

//Put this in a Material sub-class's .cpp file to allow it to work with the serialization system.
#define ADD_MATERIAL_REFLECTION_DATA_CPP(className) \
    className::_ReflectionDataInitializer className::_RefDataInit = className::_ReflectionDataInitializer();