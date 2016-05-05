#pragma once

#include "MaterialValue.h"
#include "Texture2D.h"
#include "UniquePtr.h"


class RT_API MaterialValue_Constant : public MaterialValue
{
public:

    Vectorf Value;

    
    MaterialValue_Constant(Vectorf value) : Value(value) { }


    virtual Dimensions GetNDims() const override { return Value.NValues; }

    virtual Vectorf GetValue(const Shape& shpe, const Vertex& surface,
                              const Ray& ray, FastRand& prng) const override { return Value; }

    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


    ADD_MVAL_REFLECTION_DATA_H(MaterialValue_Constant, "Constant", 1.0f);
};


//A 2D texture file lookup.
//TODO: Use a MaterialValue input for UV's. Create MaterialValues that expose information about the inputs to the function.
class RT_API MaterialValue_Tex2D : public MaterialValue
{
public:

    enum SupportedFileTypes
    {
        BMP,
        PNG,
        UNKNOWN,
    };


    UniquePtr<Texture2D> Tex;


    //Loads an image from the given file of the given type.
    //If the given file type is "UNKNOWN", it will be inferred based on the file's extension.
    //Outputs an error message if the given file wasn't loaded successfully.
    MaterialValue_Tex2D(const std::string& filePath, std::string& outErrorMsg,
                        SupportedFileTypes type = UNKNOWN);


    //Reloads the file this texture came from.
    //Returns an error message if the file wasn't loaded successfully,
    //    or the empty string if everything went fine.
    std::string Reload() { return Reload(filePath, fileType); }
    //Changes the file this texture comes from.
    //Returns an error message if the file wasn't loaded successfully,
    //    or the empty string if everything went fine.
    std::string Reload(const std::string& newPath, SupportedFileTypes newFileType = UNKNOWN);

    const std::string& GetFilePath() const { return filePath; }


    virtual Dimensions GetNDims() const override { return Three; }

    virtual Vectorf GetValue(const Shape& shpe, const Vertex& surface,
                              const Ray& ray, FastRand& prng) const override
        { return Tex->GetColor(surface.UV[0], surface.UV[1]); }


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;


private:

    std::string filePath;
    SupportedFileTypes fileType;


    MaterialValue_Tex2D() { }

    ADD_MVAL_REFLECTION_DATA_H(MaterialValue_Tex2D, "Tex2D");
};


/*
    Use a bunch of macros to quickly define the large number of simple MaterialValues.
    For example:
        * MaterialValue_Add
        * MaterialValue_Div
        * MaterialValue_Sin
        * MaterialValue_Clamp
*/

#define MAKE_MULTI_MV(name, paramsListName) \
    class RT_API MaterialValue_##name : public MaterialValue \
    { \
    public: \
         \
        std::vector<Ptr> paramsListName; \
         \
        MaterialValue_##name(std::initializer_list<Ptr> _##paramsListName) : paramsListName(_##paramsListName) { } \
        MaterialValue_##name(const std::vector<Ptr>& _##paramsListName) : paramsListName(_##paramsListName) { } \
         \
        virtual Dimensions GetNDims() const override; \
        virtual Vectorf GetValue(const Shape& shpe, const Vertex& surface, \
                                 const Ray& ray, FastRand& prng) const override; \
        virtual void WriteData(DataWriter& writer) const override; \
        virtual void ReadData(DataReader& reader) override; \
         \
        ADD_MVAL_REFLECTION_DATA_H(MaterialValue_##name, name, std::vector<Ptr>()); \
    };

#define MAKE_SIMPLE_FUNC1(name, paramName) \
    class RT_API MaterialValue_##name : public MaterialValue \
    { \
    public: \
         \
        Ptr paramName; \
         \
        MaterialValue_##name(Ptr _##paramName) : paramName(_##paramName.Release()) { }; \
         \
        virtual Dimensions GetNDims() const override { return paramName->GetNDims(); } \
        virtual Vectorf GetValue(const Shape& shpe, const Vertex& surface, \
                                 const Ray& ray, FastRand& prng) const override; \
        virtual void WriteData(DataWriter& writer) const override { MaterialValue::WriteData(writer); WriteValue(paramName, writer, #paramName); } \
        virtual void ReadData(DataReader& reader) override { MaterialValue::ReadData(reader); ReadValue(paramName, reader, #paramName); } \
         \
    private: \
        MaterialValue_##name() { } \
        ADD_MVAL_REFLECTION_DATA_H(MaterialValue_##name, name) \
    };
#define MAKE_SIMPLE_FUNC2(name, param1Name, param2Name) \
    class RT_API MaterialValue_##name : public MaterialValue \
    { \
    public: \
         \
        Ptr param1Name, param2Name; \
         \
        MaterialValue_##name(Ptr _##param1Name, Ptr _##param2Name) : param1Name(_##param1Name.Release()), param2Name(_##param2Name.Release()) { }; \
         \
        virtual Dimensions GetNDims() const override; \
        virtual Vectorf GetValue(const Shape& shpe, const Vertex& surface, \
                                 const Ray& ray, FastRand& prng) const override; \
        virtual void WriteData(DataWriter& writer) const override { MaterialValue::WriteData(writer); WriteValue(param1Name, writer, #param1Name); WriteValue(param2Name, writer, #param2Name); } \
        virtual void ReadData(DataReader& reader) override { MaterialValue::ReadData(reader); ReadValue(param1Name, reader, #param1Name); ReadValue(param2Name, reader, #param2Name); } \
         \
    private: \
        MaterialValue_##name() { } \
        ADD_MVAL_REFLECTION_DATA_H(MaterialValue_##name, name) \
    };
#define MAKE_SIMPLE_FUNC3(name, param1Name, param2Name, param3Name) \
    class RT_API MaterialValue_##name : public MaterialValue \
    { \
    public: \
         \
        Ptr param1Name, param2Name, param3Name; \
         \
        MaterialValue_##name(Ptr _##param1Name, Ptr _##param2Name, Ptr _##param3Name) : param1Name(_##param1Name.Release()), param2Name(_##param2Name.Release()), param3Name(_##param3Name.Release()) { }; \
         \
        virtual Dimensions GetNDims() const override; \
        virtual Vectorf GetValue(const Shape& shpe, const Vertex& surface, \
                                 const Ray& ray, FastRand& prng) const override; \
        virtual void WriteData(DataWriter& writer) const override { MaterialValue::WriteData(writer); WriteValue(param1Name, writer, #param1Name); WriteValue(param2Name, writer, #param2Name); WriteValue(param3Name, writer, #param3Name); } \
        virtual void ReadData(DataReader& reader) override { MaterialValue::ReadData(reader); ReadValue(param1Name, reader, #param1Name); ReadValue(param2Name, reader, #param2Name); ReadValue(param3Name, reader, #param3Name); } \
         \
    private: \
        MaterialValue_##name() { } \
        ADD_MVAL_REFLECTION_DATA_H(MaterialValue_##name, name) \
    };



MAKE_MULTI_MV(Add,  ToAdd);
MAKE_MULTI_MV(Sub,  ToSub);
MAKE_MULTI_MV(Mult, ToMultiply);
MAKE_MULTI_MV(Div,  ToDivide);

MAKE_SIMPLE_FUNC1(Sin, Input); MAKE_SIMPLE_FUNC1(Cos, Input); MAKE_SIMPLE_FUNC1(Tan, Input);
MAKE_SIMPLE_FUNC1(Asin, Input); MAKE_SIMPLE_FUNC1(Acos, Input); MAKE_SIMPLE_FUNC1(Atan, Input);
MAKE_SIMPLE_FUNC2(Atan2, Y, X);

//Returns 0.0 if "X" is less than "Edge", or 1.0 if it isn't.
MAKE_SIMPLE_FUNC2(Step, Edge, X);
MAKE_SIMPLE_FUNC3(Lerp, A, B, T);
MAKE_SIMPLE_FUNC1(Smoothstep, T);
MAKE_SIMPLE_FUNC1(Smootherstep, T);

MAKE_SIMPLE_FUNC3(Clamp, Min, Max, X);

//TODO: Add various other material values, including noise generation.

#undef MAKE_MULTI_MV
#undef MAKE_SIMPLE_FUNC1
#undef MAKE_SIMPLE_FUNC2
#undef MAKE_SIMPLE_FUNC3
#undef WRITE_MV
#undef READ_MV