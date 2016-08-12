#pragma once

#include <math.h>
#include "Main.hpp"


namespace RT
{
    class RT_API Mathf
    {
    public:

        static const float NaN;

        static float IsNaN(float f) { return f != f; }

        static float RadToDeg(float rad) { return rad * 57.2957795f; }
        static float DegToRad(float deg) { return deg * 0.0174532f; }

        template<typename T>
        static T Clamp(T val, T min, T max) { return (val < min ? min : (val > max ? max : val)); }
        template<typename T>
        static T Min(T t1, T t2, T t3) { return min(t1, min(t2, t3)); }
        template<typename T>
        static T Max(T t1, T t2, T t3) { return max(t1, max(t2, t3)); }


        static float Lerp(float a, float b, float t) { return a + (t * (b - a)); }
        static float SmoothLerp(float t) { return t * t * (3.0f + (-2.0f * t)); }
        static float SmootherLerp(float t) { return t * t * t * (10.0f + (t * (-15.0f + (t * 6.0f)))); }

        static float InvLerp(float a, float b, float value) { return (value - a) / (b - a); }
        static float Remap(float srcA, float srcB, float destA, float destB, float srcVal) { return Lerp(destA, destB, InvLerp(srcA, srcB, srcVal)); }
    };
}