#pragma once


#include "Main.hpp"
#include "Shape.h"
#include "Ray.h"
#include "Dictionary.h"
#include "DataSerialization.h"
#include "SmartPtrs.h"
#include "FastRand.h"
#include "Vectorf.h"


namespace RT
{
    /*
        NOTE: Serializing MaterialValues is complicated because multiple nodes can share a child.
        Instead of implementing ISerializable themselves,
            a special helper class called "MaterialValueGraph" implements it
            and helps them serialize properly.
        The basic idea is that every MaterialValue is assigned a unique ID,
            and any list of MaterialValue::Ptr should be serialized as a list of ID's.
    */


    class MaterialValue;
    EXPORT_SHAREDPTR(MaterialValue);

    EXPORT_RT_DICT(MaterialValue*, unsigned int);
    EXPORT_RT_DICT(const MaterialValue*, unsigned int);
    using IDToMaterialValue = Dictionary<unsigned int, SharedPtr<MaterialValue>>;
    using ConstMaterialValueToID = Dictionary<const MaterialValue*, unsigned int>;

    EXPORT_RT_LIST(unsigned int);
    EXPORT_RT_DICT(MaterialValue*, List<unsigned int>);
    using IDList = List<unsigned int>;
    using NodeToChildIDs = Dictionary<MaterialValue*, IDList>;


    //A value calculated based on a ray collision with an object.
    //For example: a texture lookup based on the collision point's UV.
    //Outputs a 1D, 2D, 3D, or 4D float value.
    class RT_API MaterialValue
    {
    public:

        typedef SharedPtr<MaterialValue> Ptr;


        static Ptr Create(const String& typeName) { return Ptr(GetFactory(typeName)()); }

        static void WriteValue(const MaterialValue* mv, const ConstMaterialValueToID& idLookup,
                               DataWriter& writer, const String& name);

        //Returns the node's ID.
        static unsigned int ReadValue(Ptr& outMV, NodeToChildIDs& childIDLookup,
                                      DataReader& reader, const String name);


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
        MaterialValue* GetChild(size_t index) { return (MaterialValue*)((const MaterialValue*)this)->GetChild(index); }
        virtual void SetChild(size_t index, const Ptr& newChild) = 0;

        //Don't override this manually! Use the "ADD_MVAL_REFLECTION_DATA_H" macros instead.
        virtual String GetTypeName() const = 0;


        virtual void WriteData(DataWriter& data, const String& namePrefix,
                               const ConstMaterialValueToID& idLookup) const;

        virtual void ReadData(DataReader& data, const String& namePrefix,
                              NodeToChildIDs& childIDLookup);
        virtual void OnDoneReadingData(const IDToMaterialValue& mvLookup,
                                       const NodeToChildIDs& childIDLookup);


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