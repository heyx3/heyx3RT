#pragma once

#include "Main.hpp"

#include <cmath>


//This namespace defines a Bounding Volume Hierarchy -- a spacial data structure.
namespace BVH
{
    //A sample "Pos<>" template struct is defined here,
    //    but it isn't necessary since RT already offers vector types that conform to this standard.

#if 0
    #pragma region Definition of Pos<> struct

    //"ComponentType" is the type of number along each axis (e.x. "float").
    //"NComponents" is the dimensionality of this kind of position.
    template<typename ComponentType, unsigned int NComponents>
    //A position. An instance of this template should be used for the below "Bounds" template.
    struct Pos
    {
    public:

        typedef ComponentType MyComponent;

        static const unsigned int NumbComponents = NComponents;


        ComponentType Values[NComponents];


        Pos() { }
        Pos(ComponentType values[NComponents])
        {
            memcpy(Values, values, sizeof(ComponentType) * NComponents);
        }


        ComponentType& operator[](unsigned int i) { return Values[i]; }
        const ComponentType& operator[](unsigned int i) const { return Values[i]; }

        Pos operator+(const Pos<ComponentType, NComponents>& other) const
        {
            Pos p;
            for (unsigned int i = 0; i < NComponents; ++i)
                p[i] = Values[i] + other[i];
            return p;
        }
        Pos operator-(const Pos<ComponentType, NComponents>& other) const
        {
            Pos p;
            for (unsigned int i = 0; i < NComponents; ++i)
                p[i] = Values[i] - other[i];
            return p;
        }
        Pos operator*(const ComponentType& scale) const
        {
            Pos p;
            for (unsigned int i = 0; i < NComponents; ++i)
                p[i] = Values[i] * scale;
            return p;
        }
        Pos operator/(const ComponentType& denominator) const
        {
            Pos p;
            for (unsigned int i = 0; i < NComponents; ++i)
                p[i] = Values[i] / denominator;
            return p;
        }
    };

    typedef Pos<float, 1> Pos1f;
    typedef Pos<float, 2> Pos2f;
    typedef Pos<float, 3> Pos3f;
    typedef Pos<int, 1> Pos1i;
    typedef Pos<int, 2> Pos2i;
    typedef Pos<int, 3> Pos3i;

    #pragma endregion
#endif



    //"PosType" is the type of data structure representing position.
    //Should be a version of the "BVH::Pos<>" template.
    template<typename PosType>
    //A shape representing the bounding space around an item/items in the BVH.
    //This class is merely a reference and can't actually be used;
    //    any data structure with the same interface as this can be used in a BVH.
    struct Bounds
    {
    public:

        typedef PosType MyPosType;
        typedef Bounds<T, PosType> ThisType;


        Bounds() = delete;
        Bounds(const Bounds& cpy) = delete;

        PosType GetCenter() const = delete;
        ThisType Union(const ThisType& other) const = delete;
        bool Contains(const PosType& p) const = delete;
        bool Overlaps(const ThisType& other) const = delete;
    };



    //Below is a definition for the most commonly-used types of bounds for a BVH.


    //"PosType" should be a version of the "BVH::Pos<>" template,
    //    or at least a data structure with the same interface and behaviour.
    template<typename PosType>
    //An axis-aligned bounding space for some object.
    struct AABounds
    {
    public:

        typedef PosType MyPosType;
        typedef AABounds<PosType> ThisType;


        PosType Min, Max;

        AABounds() : Min(PosType()), Max(PosType()) { }
        AABounds(const PosType& min, const PosType& max) : Min(min), Max(max) { }
        AABounds(const ThisType& cpy) : Min(cpy.Min), Max(cpy.Max) { }


        PosType GetCenter() const { return (Min + Max) / 2; }
        ThisType Union(const ThisType& other) const
        {
            PosType min, max;
            for (unsigned int i = 0; i < PosType::NumbComponents; ++i)
            {
                min[i] = (Min[i] < other.Min[i]) ? Min[i] : other.Min[i];
                max[i] = (Max[i] > other.Max[i]) ? Max[i] : other.Max[i];
            }
            return ThisType(min, max);
        }
        bool Contains(const PosType& p) const
        {
            for (unsigned int i = 0; i < PosType::NumbComponents; ++i)
                if (p.Values[i] < Min.Values[i] || p.Values[i] > Max.Values[i])
                    return false;
            return true;
        }
        bool Overlaps(const ThisType& other) const
        {
            for (unsigned int i = 0; i < PosType::NumbComponents; ++i)
                if (Min.Values[i] > other.Max.Values[i] || Max.Values[i] < other.Min.Values[i])
                    return false;
            return true;
        }
    };


    //"PosType" should be a version of the "BVH::Pos<>" template,
    //    or at least a data structure with the same interface and behaviour.
    //Note that this type of bounds will be inaccurate if not using floating-point positions.
    template<typename PosType>
    //A bounding sphere for some object.
    struct SphereBounds
    {
    public:

        typedef PosType MyPosType;
        typedef SphereBounds<PosType> ThisType;


        PosType Center;
        PosType::MyComponent Radius;

        SphereBounds() : Center(), Radius(0) { }
        SphereBounds(const PosType& center, const PosType::MyComponent& radius)
            : Center(center), Radius(radius)
        { }
        SphereBounds(const ThisType& cpy) : Center(cpy.Center), Radius(cpy.Radius); { }


        PosType GetCenter() const { return Center; }
        ThisType Union(const ThisType& other) const
        {
            PosType tempCenter = (Center + other.Center) / 2;
            PosType
                PosType::MyComponent distSqr1 = DistSqr(newCenter,
                                                        AddLength(other.Center - newCenter, other.Radius)),
                distSqr2 = DistSqr(newCenter,
                                   AddLength(Center - newCenter, Radius));
            if (distSqr1 > distSqr2)
            {
                return SphereBounds(newCenter, std::sqrt(distSqr1));
            }
            else
            {
                return SphereBounds(newCenter, std::sqrt(distSqr2));
            }
        }
        bool Contains(const PosType& p) const
        {
            return LengthSqr(Center - p) < (Radius * Radius);
        }
        bool Overlaps(const ThisType& other) const
        {
            return SphereBounds(Center, Radius + other.Radius).Contains(other.Center);
        }

    private:

        static PosType::MyComponent LengthSqr(const PosType& vector)
        {
            PosType::MyComponent result = 0;
            for (unsigned int i = 0; i < PosType::NumbComponents; ++i)
                result += p[i] * p[i];
            return result;
        }
        static PosType::MyComponent DistSqr(const PosType& p1, const PosType& p2)
        {
            return LengthSqr(p1 - p2);
        }
        static PosType AddLength(const PosType& vector, const PosType::MyComponent& deltaLength)
        {
            PosType::MyComponent length = std::sqrt(LengthSqr(vector));
            PosType::MyComponent lengthScale = (length + deltaLength) / length;
            return vector * lengthScale;
        }
    };
}