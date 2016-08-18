#pragma once

#include "MaterialValue.h"
#include "Texture2D.h"
#include "SmartPtrs.h"


namespace RT
{
    //All MaterialValues in this file have the prefix "MV_" in their name.
    //For example: MV_Constant, MV_Tex2D, MV_SurfPos.


    //Below is a forward declaration of every MaterialValue defined in this file,
    //    so that you don't have to go hunting through it.

    //All trig functions use radians.

    class MV_Constant;
    class MV_Tex2D;

    class MV_Add;
    class MV_Subtract;
    class MV_Multiply;
    class MV_Divide;

    class MV_Normalize;
    class MV_Length;
    class MV_Distance;

    class MV_Sqrt;
    class MV_Sin;
    class MV_Cos;
    class MV_Tan;
    class MV_Asin;
    class MV_Acos;
    class MV_Atan;
    class MV_Atan2;

    class MV_Step;
    class MV_Lerp;
    class MV_Smoothstep;
    class MV_Smootherstep;
    class MV_Clamp;

    class MV_Floor;
    class MV_Ceil;
    class MV_Abs;
    class MV_Min;
    class MV_Max;

    class MV_SurfUV;
    class MV_SurfPos;
    class MV_SurfNormal;
    class MV_SurfTangent;
    class MV_SurfBitangent;
    class MV_RayStartPos;
    class MV_RayPos;
    class MV_RayDir;
    class MV_ShapePos;
    class MV_ShapeScale;
    class MV_ShapeRot;
    class MV_PureNoise;



    //Just outputs a constant value.
    class RT_API MV_Constant : public MaterialValue
    {
    public:

        static Ptr Create(float x) { return new MV_Constant(x); }
        static Ptr Create(float x, float y) { return new MV_Constant(Vector2f(x, y)); }
        static Ptr Create(float x, float y, float z) { return new MV_Constant(Vector3f(x, y, z)); }
        static Ptr Create(float x, float y, float z, float w) { return new MV_Constant(Vector4f(x, y, z, w)); }


        Vectorf Value;

    
        MV_Constant(Vectorf value) : Value(value) { }


        virtual Dimensions GetNDims() const override { return Value.NValues; }

        virtual Vectorf GetValue(const Ray& ray, FastRand& prng,
                                 const Shape* shpe = nullptr,
                                 const Vertex* surface = nullptr) const override
            { return Value; }

        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;

        virtual size_t GetNChildren() const override { return 0; }
        virtual const MaterialValue* GetChild(size_t i) const override { return nullptr; }


        ADD_MVAL_REFLECTION_DATA_H(MV_Constant, Constant, 1.0f);
    };


//Defines a class that outputs a basic property of the ray, shape, or surface.
#define DEF_BASIC_MVAL(name, nDims, getValueBody) \
    class RT_API MV_##name : public MaterialValue \
    { \
    public: \
        virtual Dimensions GetNDims() const override { return nDims; } \
        virtual Vectorf GetValue(const Ray& ray, FastRand& prng, \
                                    const Shape* shpe = nullptr, \
                                    const Vertex* surface = nullptr) const override \
            { getValueBody } \
        virtual size_t GetNChildren() const override { return 0; } \
        virtual const MaterialValue* GetChild(size_t i) const override { return nullptr; } \
        ADD_MVAL_REFLECTION_DATA_H(MV_##name, name); \
    };

    //Gets the UV coords of the surface.
    DEF_BASIC_MVAL(SurfUV, Two, AssertExists(surface); return surface->UV; );
    //Gets the normal of the surface.
    DEF_BASIC_MVAL(SurfNormal, Three, AssertExists(surface); return surface->Normal; );
    //Gets the tangent of the surface.
    DEF_BASIC_MVAL(SurfTangent, Three, AssertExists(surface); return surface->Tangent; );
    //Gets the bitangent of the surface.
    DEF_BASIC_MVAL(SurfBitangent, Three, AssertExists(surface); return surface->Bitangent; );
    //Gets the position of the surface.
    DEF_BASIC_MVAL(SurfPos, Three, AssertExists(surface); return surface->Pos; );
    //Gets the starting position of the ray.
    DEF_BASIC_MVAL(RayStartPos, Three, AssertExists(surface); return ray.GetPos(); );
    //Gets the direction of the ray.
    DEF_BASIC_MVAL(RayDir, Three, return ray.GetDir(); );
    //Gets the position of the shape that was hit.
    DEF_BASIC_MVAL(ShapePos, Three, AssertExists(shpe); return shpe->Tr.GetPos(); );
    //Gets the scale of the shape that was hit.
    DEF_BASIC_MVAL(ShapeScale, Three, AssertExists(shpe); return shpe->Tr.GetScale(); );
    //Gets the axis/angle rotation of the shape that was hit.
    //The X/Y/Z are the axis, and the W is the angle in radians.
    DEF_BASIC_MVAL(ShapeRot, Four, AssertExists(shpe); return shpe->Tr.GetRot().GetAxisAngle(); );

    #undef DEF_BASIC_MVAL


    //Gets a position along the ray given a time T.
    class RT_API MV_RayPos : public MaterialValue
    {
    public:
        Ptr T;
        MV_RayPos(Ptr t) : T(t.Release()) { }
        virtual Dimensions GetNDims() const override { return Three; }
        virtual Vectorf GetValue(const Ray& ray, FastRand& prng,
                                 const Shape* shpe = nullptr,
                                 const Vertex* surface = nullptr) const override
            { return ray.GetPos((float)T->GetValue(ray, prng, shpe, surface)); }
        virtual size_t GetNChildren() const override { return 1; }
        virtual const MaterialValue* GetChild(size_t i) const override { return T.Get(); }
        virtual void WriteData(DataWriter& writer) const override { WriteValue(T, writer, "T"); }
        virtual void ReadData(DataReader& reader) override { ReadValue(T, reader, "T"); }
    private:
        MV_RayPos() { }
        ADD_MVAL_REFLECTION_DATA_H(MV_RayPos, RayPos);
    };

    //Gets a totally random value between 0 and 1.
    //The value may have 1-4 dimensions.
    class RT_API MV_PureNoise : public MaterialValue
    {
    public:
        Dimensions NDims;
        MV_PureNoise(Dimensions nDims = One) : NDims(nDims) { }
        virtual Dimensions GetNDims() const override { return NDims; }
        virtual Vectorf GetValue(const Ray& ray, FastRand& prng,
                                 const Shape* shpe = nullptr,
                                 const Vertex* surface = nullptr) const override;
        virtual size_t GetNChildren() const override { return 0; }
        virtual const MaterialValue* GetChild(size_t i) const override { return nullptr; }
        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;
    private:
        ADD_MVAL_REFLECTION_DATA_H(MV_PureNoise, PureNoise);
    };


    EXPORT_UNIQUEPTR(Texture2D);

    //A 2D texture file lookup.
    class RT_API MV_Tex2D : public MaterialValue
    {
    public:

        Ptr UV;
        UniquePtr<Texture2D> Tex;


        //Loads an image from the given file of the given type.
        //If the given file type is "UNKNOWN", it will be inferred based on the file's extension.
        //Outputs an error message if the given file wasn't loaded successfully.
        MV_Tex2D(const String& filePath, String& outErrorMsg,
                 Ptr uv = new MV_SurfUV,
                 Texture2D::SupportedFileTypes type = Texture2D::UNKNOWN);


        //Reloads the file this texture came from.
        //Returns an error message if the file wasn't loaded successfully,
        //    or the empty string if everything went fine.
        String Reload() { return Reload(filePath, fileType); }
        //Changes the file this texture comes from.
        //Returns an error message if the file wasn't loaded successfully,
        //    or the empty string if everything went fine.
        String Reload(const String& newPath,
                      Texture2D::SupportedFileTypes newFileType = Texture2D::UNKNOWN);

        const String& GetFilePath() const { return filePath; }


        virtual Dimensions GetNDims() const override { return Three; }

        virtual Vectorf GetValue(const Ray& ray, FastRand& prng,
                                 const Shape* shpe = nullptr,
                                 const Vertex* surface = nullptr) const override
            { return Tex->GetColor(UV->GetValue(ray, prng, shpe, surface)); }

        virtual size_t GetNChildren() const override { return 1; }
        virtual const MaterialValue* GetChild(size_t i) const override { return UV.Get(); }

        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;


    private:

        String filePath;
        Texture2D::SupportedFileTypes fileType;


        MV_Tex2D() { }

        ADD_MVAL_REFLECTION_DATA_H(MV_Tex2D, Tex2D);
    };


/*
    Use a bunch of macros to quickly define the large number of simple MaterialValues.
    For example:
        * MV_Add
        * MV_Div
        * MV_Sin
        * MV_Clamp
*/

#pragma region Helper macro definitions

#define MAKE_MULTI_MV(name, paramsListName) \
    class RT_API MV_##name : public MaterialValue \
    { \
    public: \
            \
        MV_##name() { } \
        MV_##name(Ptr& val1, Ptr& val2) { paramsListName.push_back(Ptr(val1.Release())); paramsListName.push_back(Ptr(val2.Release())); } \
        MV_##name(Ptr& val1, Ptr& val2, Ptr& val3) { paramsListName.push_back(Ptr(val1.Release())); paramsListName.push_back(Ptr(val2.Release())); paramsListName.push_back(Ptr(val3.Release())); } \
        MV_##name(Ptr& val1, Ptr& val2, Ptr& val3, Ptr& val4) { paramsListName.push_back(Ptr(val1.Release())); paramsListName.push_back(Ptr(val2.Release())); paramsListName.push_back(Ptr(val3.Release())); paramsListName.push_back(Ptr(val4.Release())); } \
        MV_##name(Ptr& val1, Ptr& val2, Ptr& val3, Ptr& val4, Ptr& val5) { paramsListName.push_back(Ptr(val1.Release())); paramsListName.push_back(Ptr(val2.Release())); paramsListName.push_back(Ptr(val3.Release())); paramsListName.push_back(Ptr(val4.Release())); paramsListName.push_back(Ptr(val5.Release())); } \
            \
        virtual Dimensions GetNDims() const override; \
        virtual Vectorf GetValue(const Ray& ray, FastRand& prng, \
                                    const Shape* shpe = nullptr, \
                                    const Vertex* surface = nullptr) const override; \
            \
        void AddElement(Ptr& p) { paramsListName.push_back(Ptr(p.Release())); } \
        void RemoveElement(MaterialValue* ptr); \
            \
        virtual void WriteData(DataWriter& writer) const override; \
        virtual void ReadData(DataReader& reader) override; \
            \
        virtual size_t GetNChildren() const override { return paramsListName.size(); } \
        virtual const MaterialValue* GetChild(size_t i) const override { return paramsListName[i].Get(); } \
            \
        private: \
            \
        std::vector<Ptr> paramsListName; \
            \
        MV_##name(const MV_##name& cpy) = delete; \
        MV_##name& operator=(const MV_##name& cpy) = delete; \
            \
        ADD_MVAL_REFLECTION_DATA_H(MV_##name, name); \
    };

#define MAKE_COMPLEX_FUNC1(name, paramName, dimsCalc) \
    class RT_API MV_##name : public MaterialValue \
    { \
    public: \
            \
        Ptr paramName; \
            \
        MV_##name(Ptr _##paramName) : paramName(_##paramName.Release()) { }; \
            \
        virtual Dimensions GetNDims() const override { dimsCalc } \
        virtual Vectorf GetValue(const Ray& ray, FastRand& prng, \
                                    const Shape* shpe = nullptr, \
                                    const Vertex* surface = nullptr) const override; \
        virtual size_t GetNChildren() const override { return 1; } \
        virtual const MaterialValue* GetChild(size_t i) const override { return paramName.Get(); } \
        virtual void WriteData(DataWriter& writer) const override { MaterialValue::WriteData(writer); WriteValue(paramName, writer, #paramName); } \
        virtual void ReadData(DataReader& reader) override { MaterialValue::ReadData(reader); ReadValue(paramName, reader, #paramName); } \
            \
    private: \
        MV_##name() { } \
        ADD_MVAL_REFLECTION_DATA_H(MV_##name, name) \
    };
#define MAKE_SIMPLE_FUNC1(name, paramName) MAKE_COMPLEX_FUNC1(name, paramName, return paramName->GetNDims(); )

#define MAKE_SIMPLE_FUNC2(name, param1Name, param2Name) \
    class RT_API MV_##name : public MaterialValue \
    { \
    public: \
            \
        Ptr param1Name, param2Name; \
            \
        MV_##name(Ptr _##param1Name, Ptr _##param2Name) : param1Name(_##param1Name.Release()), param2Name(_##param2Name.Release()) { }; \
            \
        virtual Dimensions GetNDims() const override; \
        virtual Vectorf GetValue(const Ray& ray, FastRand& prng, \
                                    const Shape* shpe = nullptr, \
                                    const Vertex* surface = nullptr) const override; \
        virtual size_t GetNChildren() const override { return 2; } \
        virtual const MaterialValue* GetChild(size_t i) const override { return (i == 0 ? param1Name : param2Name).Get(); } \
        virtual void WriteData(DataWriter& writer) const override { MaterialValue::WriteData(writer); WriteValue(param1Name, writer, #param1Name); WriteValue(param2Name, writer, #param2Name); } \
        virtual void ReadData(DataReader& reader) override { MaterialValue::ReadData(reader); ReadValue(param1Name, reader, #param1Name); ReadValue(param2Name, reader, #param2Name); } \
            \
    private: \
        MV_##name() { } \
        ADD_MVAL_REFLECTION_DATA_H(MV_##name, name) \
    };
#define MAKE_SIMPLE_FUNC3(name, param1Name, param2Name, param3Name) \
    class RT_API MV_##name : public MaterialValue \
    { \
    public: \
            \
        Ptr param1Name, param2Name, param3Name; \
            \
        MV_##name(Ptr _##param1Name, Ptr _##param2Name, Ptr _##param3Name) : param1Name(_##param1Name.Release()), param2Name(_##param2Name.Release()), param3Name(_##param3Name.Release()) { }; \
            \
        virtual Dimensions GetNDims() const override; \
        virtual Vectorf GetValue(const Ray& ray, FastRand& prng, \
                                    const Shape* shpe = nullptr, \
                                    const Vertex* surface = nullptr) const override; \
        virtual size_t GetNChildren() const override { return 3; } \
        virtual const MaterialValue* GetChild(size_t i) const override { return (i == 0 ? param1Name : (i == 1 ? param2Name : param3Name)).Get(); } \
        virtual void WriteData(DataWriter& writer) const override { MaterialValue::WriteData(writer); WriteValue(param1Name, writer, #param1Name); WriteValue(param2Name, writer, #param2Name); WriteValue(param3Name, writer, #param3Name); } \
        virtual void ReadData(DataReader& reader) override { MaterialValue::ReadData(reader); ReadValue(param1Name, reader, #param1Name); ReadValue(param2Name, reader, #param2Name); ReadValue(param3Name, reader, #param3Name); } \
            \
    private: \
        MV_##name() { } \
        ADD_MVAL_REFLECTION_DATA_H(MV_##name, name) \
    };

#pragma endregion

    #pragma warning (disable: 4251)
    //Adds an arbitrary number of inputs together.
    MAKE_MULTI_MV(Add,  ToAdd);
    //Subtracts an arbitrary number of inputs from the first input.
    MAKE_MULTI_MV(Subtract,  ToSub);
    //Multiplies an arbitrary number of inputs together.
    MAKE_MULTI_MV(Multiply, ToMultiply);
    //Divides an arbitrary number of inputs from the first input.
    MAKE_MULTI_MV(Divide,  ToDivide);
    //Gets the smallest of an arbitrary number of inputs for each component.
    MAKE_MULTI_MV(Min, ToUse);
    //Gets the largest of an arbitrary number of inputs for each component.
    MAKE_MULTI_MV(Max, ToUse);
    #pragma warning (default: 4251)

    MAKE_SIMPLE_FUNC1(Normalize, X);
    MAKE_COMPLEX_FUNC1(Length, X, return One; );
    MAKE_SIMPLE_FUNC2(Distance, A, B);

    MAKE_SIMPLE_FUNC1(Sqrt, X);
    MAKE_SIMPLE_FUNC1(Sin, X); MAKE_SIMPLE_FUNC1(Cos, X); MAKE_SIMPLE_FUNC1(Tan, X);
    MAKE_SIMPLE_FUNC1(Asin, X); MAKE_SIMPLE_FUNC1(Acos, X); MAKE_SIMPLE_FUNC1(Atan, X);
    MAKE_SIMPLE_FUNC2(Atan2, Y, X);

    //Returns 0.0 if "X" is less than "Edge", or 1.0 if it isn't.
    MAKE_SIMPLE_FUNC2(Step, Edge, X);
    MAKE_SIMPLE_FUNC3(Lerp, A, B, T);
    MAKE_SIMPLE_FUNC1(Smoothstep, T);
    MAKE_SIMPLE_FUNC1(Smootherstep, T);

    MAKE_SIMPLE_FUNC1(Floor, X);
    MAKE_SIMPLE_FUNC1(Ceil, X);
    MAKE_SIMPLE_FUNC1(Abs, X);

    MAKE_SIMPLE_FUNC3(Clamp, Min, Max, X);

    //TODO: Reflection/refraction MV's.
    //TODO: "Average" MV.
    //TODO: Noise generation MV's.
    //TODO: Append/Component MV's.
    //TODO: Pow MV.

#undef MAKE_MULTI_MV
#undef MAKE_COMPLEX_FUNC1
#undef MAKE_SIMPLE_FUNC1
#undef MAKE_SIMPLE_FUNC2
#undef MAKE_SIMPLE_FUNC3
#undef WRITE_MV
#undef READ_MV
}