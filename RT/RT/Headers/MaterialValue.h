#pragma once


#include "Main.hpp"
#include "Shape.h"
#include "Ray.h"
#include "DataSerialization.h"
#include "SmartPtrs.h"
#include "FastRand.h"
#include "Vectorf.h"


namespace RT
{
    class MaterialValue;
    EXPORT_UNIQUEPTR(MaterialValue);


    //A value calculated based on a ray collision with an object.
    //For example, a texture lookup based on the collision point's UV.
    //Outputs anything from a 1D vector to a 4D vector.
    class RT_API MaterialValue : public ISerializable
    {
    public:

        typedef UniquePtr<MaterialValue> Ptr;


        static Ptr Create(const String& typeName)
        {    
            return Ptr(GetFactory(typeName)());
        }

        static void WriteValue(const Ptr& mv, DataWriter& writer, const String& name)
        {
            writer.WriteString(mv->GetTypeName(), name + "Type");
            writer.WriteDataStructure(*mv, name + "Value");
        }
        static void ReadValue(Ptr& outMV, DataReader& reader, const String name)
        {
            String typeName;
            reader.ReadString(typeName, name + "Type");
            outMV.Reset(Create(typeName).Release());
            reader.ReadDataStructure(*outMV, name + "Value");
        }


        MaterialValue() { }
        virtual ~MaterialValue() { }


        virtual Dimensions GetNDims() const = 0;

        //Gets a value with between 1 and 4 dimensions.
        //Note that the shape and vertex may be null if nothing was hit by the ray.
        virtual Vectorf GetValue(const Ray& ray, FastRand& prng,
                                 const Shape* shpe = nullptr,
                                 const Vertex* surface = nullptr) const = 0;


        virtual size_t GetNChildren() const = 0;
        virtual const MaterialValue* GetChild(size_t index) const = 0;
        MaterialValue* GetChild(size_t index) { return const_cast<MaterialValue*>(((const MaterialValue*)this)->GetChild(index)); }

        //Don't override this manually! Use the "ADD_MVAL_REFLECTION_DATA_H" macros instead.
        virtual String GetTypeName() const = 0;

        virtual void ReadData(DataReader& data) override { }
        virtual void WriteData(DataWriter& data) const override { }


    protected:
    

        typedef MaterialValue*(*MVFactory)();


        MaterialValue(const MaterialValue& cpy) = delete;
        virtual MaterialValue& operator=(const MaterialValue& cpy) = delete;


        void AssertExists(const Shape* shpe) const;
        void AssertExists(const Vertex* surface) const;


        //Sets the factory to use for the given class name.
        //Makes the given class name visible to the serialization system.
        //NOTE: This should never be called manually; use the "ADD_MVAL_REFLECTION_DATA" macros.
        static void AddReflectionData(const String& typeName, MVFactory factory);
        //Gets the factory to create a MaterialValue with the given type-name (from "GetTypeName()").
        //Used by the serialization system.
        static MVFactory GetFactory(const String& typeName);
    };
}


//Put this in a MaterialValue sub-class's .h file to allow it to work with the serialization system.
//The extra arguments after "className" are the arguments to construct an instance of the class.
//The actual value of the constructor arguments isn't important.
#define ADD_MVAL_REFLECTION_DATA_H(className, typeName, ...) \
    public: \
        virtual String GetTypeName() const override { return #typeName; } \
    private: \
        struct RT_API _ReflectionDataInitializer \
        { \
        public: \
            _ReflectionDataInitializer() \
            { \
                AddReflectionData(#typeName, []() { return (MaterialValue*)(new className(__VA_ARGS__)); }); \
            } \
        }; \
        static _ReflectionDataInitializer _RefDataInit;

//Put this in a MaterialValue sub-class's .cpp file to allow it to work with the serialization system.
#define ADD_MVAL_REFLECTION_DATA_CPP(className) \
    className::_ReflectionDataInitializer className::_RefDataInit = className::_ReflectionDataInitializer();