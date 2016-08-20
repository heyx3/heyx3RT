#pragma once

#include <assert.h>
#include "Vectors.h"
#include "DataSerialization.h"



namespace RT
{
    enum RT_API Dimensions : unsigned char
    {
        One = 1,
        Two = 2,
        Three = 3,
        Four = 4,
    };

    inline Dimensions Min(Dimensions a, Dimensions b) { return (a < b ? a : b); }
    inline Dimensions MinIgnoring1D(Dimensions a, Dimensions b) { return (a == One ? b : (b == One ? a : Min(a, b))); }
    inline Dimensions MinIgnoring1D(Dimensions a, Dimensions b, Dimensions c) { return MinIgnoring1D(a, MinIgnoring1D(b, c)); }

    inline Dimensions Max(Dimensions a, Dimensions b) { return (a > b ? a : b); }


    //A vector with between 1 and 4 values.
    struct RT_API Vectorf
    {
    public:

        float x, y, z, w;
        Dimensions NValues;


        Vectorf() { *this = 0.0f; }
        Vectorf(float f) { *this = f; }
        Vectorf(Vector2f v) { *this = v; }
        Vectorf(Vector3f v) { *this = v; }
        Vectorf(Vector4f v) { *this = v; }


        Vectorf Reciprocal() const { return Vectorf(1.0f / x, 1.0f / y, 1.0f / z, 1.0f / w, NValues); }

        float LengthSqr() const;
        float Length() const { return sqrtf(LengthSqr()); }

        float DistanceSqr(const Vectorf& other) const { return (other - *this).LengthSqr(); }
        float Distance(const Vectorf& other) const { return (other - *this).Length(); }

        float Dot(const Vectorf& other) const;

        Vectorf Normalized() const { return *this / Length(); }


        //"Func" is of the type "float f(float myF, float otherF)".
        template<typename Func>
        //Does the given function to each component in this vector.
        Vectorf OperateOn(Func f) const
        {
            Vectorf newV;
            newV.NValues = NValues;
            for (size_t i = 0; i < (size_t)NValues; ++i)
                newV[i] = f(operator[](i));
            return newV;
        }
        //"Func" is of the type "float f(float myF, float otherF)".
        template<typename Func>
        //Does a component-wise operation between this and the given Vectorf.
        //When two vectors don't have the same number of dimensions, one of the following will happen:
        //    * If one of the vectors is 1D, its single component will be applied to each component of the other vector.
        //    * Otherwise, the output vector will be the same size as the smallest vector,
        //      and the extra components in the other vector are ignored.
        Vectorf OperateOn(Func f, const Vectorf& other) const
        {
            Vectorf outF;

            outF.NValues = MinIgnoring1D(NValues, other.NValues);

            for (size_t i = 0; i < outF.NValues; ++i)
                outF[i] = f(NValues == 1 ? x : (*this)[i],
                            other.NValues == 1 ? other.x : other[i]);

            return outF;
        }
        //"Func" is of the type "float f(float myF, float otherF)".
        template<typename Func>
        //Does a component-wise operation between this and the given Vectorf.
        //When two vectors don't have the same number of dimensions, one of the following will happen:
        //    * If one of the vectors is 1D, its single component will be applied to each component of the other vector.
        //    * Otherwise, the output vector will be the same size as the largest vector,
        //      and the "identity" parameters fill in any missing components in the smaller one.
        Vectorf OperateOn(Func f, const Vectorf& other, float myIdentity, float otherIdentity) const
        {
            //If the two vectors have the same size, this is trivial.
            if (NValues == other.NValues)
                return OperateOn(f, other);

            //Create two new Vectorf's of equal size and do the operation on them.
            Dimensions outDims = Max(NValues, other.NValues);
            Vectorf me2, other2;
            me2.NValues = outDims;
            other2.NValues = outDims;
            for (size_t i = 0; i < outDims; ++i)
            {
                if (NValues == One)
                    me2[i] = x;
                else if (i < NValues)
                    me2[i] = operator[](i);
                else
                    me2[i] = myIdentity;

                if (other.NValues == One)
                    other2[i] = other.x;
                else if (i < (size_t)other.NValues)
                    other2[i] = other[i];
                else
                    other2[i] = otherIdentity;
            }

            return me2.OperateOn(f, other2);
        }
        //"Func" is of the type "float f(float myF, float otherF)".
        template<typename Func>
        //Does a component-wise operation between this and the given Vectorf.
        //When two vectors don't have the same number of dimensions, one of the following will happen:
        //    * If one of the vectors is 1D, its single component will be applied to each component of the other vector.
        //    * Otherwise, the output vector will be the same size as the largest vector,
        //      and the "identity" parameter fills in any missing components in the smaller one.
        Vectorf OperateOn(Func f, const Vectorf& other, float identity) const
        {
            return OperateOn(f, other, identity, identity);
        }
        //"Func" is of the type "float f(float myF, float other1F, float other2F)".
        template<typename Func>
        //Does a component-wise operation between three vectors.
        //When the vectors don't have the same number of dimensions, one of the following will happen:
        //    * If one of the vectors is 1D, its single component will be applied to each component of the other vectors.
        //    * The output vector will be the same size as the smallest vector (ignoring 1D vectors here),
        //      and the extra components in the other vectors are ignored.
        Vectorf OperateOn(Func f, const Vectorf& other1, const Vectorf& other2) const
        {
            Vectorf outF;

            outF.NValues = MinIgnoring1D(NValues, MinIgnoring1D(other1.NValues, other2.NValues));

            for (size_t i = 0; i < outF.NValues; ++i)
                outF[i] = f(NValues == 1 ? x : (*this)[i],
                            other1.NValues == 1 ? other1.x : other1[i],
                            other2.NValues == 1 ? other2.x : other2[i]);

            return outF;
        }
        //"Func" is of the type "float f(float myF, float other1F, float other2F)".
        template<typename Func>
        //Does a component-wise operation between three vectors.
        //When the vectors don't have the same number of dimensions, one of the following will happen:
        //    * If one of the vectors is 1D, its single component will be applied to each component of the other vectors.
        //    * The output vector will be the same size as the largest vector (ignoring 1D vectors here),
        //      and the extra components in the other vectors are filled in with the given "identity" params.
        Vectorf OperateOn(Func f, const Vectorf& other1, const Vectorf& other2,
                          float myIdentity, float other1Identity, float other2Identity) const
        {
            //If the vectors have the same size, this is trivial.
            if (NValues == other1.NValues && NValues == other2.NValues)
                return OperateOn(f, other1, other2);

            //Create new Vectorf's of equal size and do the operation on them.
            Dimensions outDims = MaxIgnoring1D(NValues, MaxIgnoring1D(other1.NValues, other2.NValues));
            Vectorf me2, other1_2, other2_2;
            me2.NValues = outDims;
            other1_2.NValues = outDims;
            other2_2.NValues = outDims;
            for (size_t i = 0; i < outDims; ++i)
            {
                if (NValues == One)
                    me2[i] = x;
                else if (i < NValues)
                    me2[i] = operator[](i);
                else
                    me2[i] = myIdentity;

                if (other1_2.NValues == One)
                    other1_2[i] = other1.x;
                else if (i < other1.NValues)
                    other1_2[i] = other1[i];
                else
                    other1_2[i] = other1Identity;

                if (other2_2.NValues == One)
                    other2_2[i] = other2.x;
                else if (i < other2.NValues)
                    other2_2[i] = other2[i];
                else
                    other2_2[i] = other2Identity;
            }

            return me2.OperateOn(f, other1, other2);
        }


        //Casting to float must be explicit to prevent subtle bugs.
        explicit operator float() const { return x; }
        operator Vector2f() const;
        operator Vector3f() const;
        operator Vector4f() const;


        const float& operator[](size_t i) const { switch (i) { case 1: return x; case 2: return y; case 3: return z; case 4: return w; default: assert(false); return x; } }
        float& operator[](size_t i) { switch (i) { case 1: return x; case 2: return y; case 3: return z; case 4: return w; default: assert(false); return x; } }

        Vectorf& operator=(float f)    { NValues = One; x = f; return *this; }
        Vectorf& operator=(Vector2f v) { NValues = Two; x = v.x; y = v.y; return *this; }
        Vectorf& operator=(Vector3f v) { NValues = Three; x = v.x; y = v.y; z = v.z; return *this; }
        Vectorf& operator=(Vector4f v) { NValues = Four; x = v.x; y = v.y; z = v.z; w = v.w; return *this; }

        Vectorf operator+(const Vectorf& other) const;
        Vectorf operator*(const Vectorf& other) const;
        Vectorf operator-(const Vectorf& other) const;
        Vectorf operator/(const Vectorf& other) const;

        Vectorf operator-() const { return OperateOn([](float f) { return -f; }); }


    private:

        Vectorf(float _x, float _y, float _z, float _w, Dimensions nValues)
            : x(_x), y(_y), z(_z), w(_w), NValues(nValues) { }
    };


    struct RT_API Vectorf_Writable : public IWritable
    {
    public:
        const Vectorf& V;
        Vectorf_Writable(const Vectorf& v) : V(v) { }
        virtual void WriteData(DataWriter& writer) const override;
    };
    struct RT_API Vectorf_Readable: public IReadable
    {
    public:
        Vectorf& V;
        Vectorf_Readable(Vectorf& v) : V(v) { }
        virtual void ReadData(DataReader& reader) override;
    };
}