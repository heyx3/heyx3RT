#pragma once

#include <assert.h>

#include "Vectors.h"


namespace RT
{
    //A fast (but not cryptographically-strong) PRNG.
    //Always generates non-negative numbers.
    class RT_API FastRand
    {
    public:
        static int Hash(int x, int y) { return (x << 5) + x ^ y; } //http://stackoverflow.com/questions/13812335/how-to-hash-an-int-in-c-sharp
        static int Hash(float f) { return *(int*)(&f); }


	    int Seed;


        FastRand(int seed = 1234567) : Seed(seed) { }
        FastRand(int x, int y) : FastRand(Hash(x, y)) { }
        FastRand(int x, int y, int z) : FastRand(Hash(x, y), z) { }
        FastRand(int x, int y, int z, int w) : FastRand(Hash(x, y), Hash(z, w)) { }

        FastRand(float seed) : FastRand(Hash(seed)) { }
        FastRand(float x, float y) : FastRand(Hash(x), Hash(y)) { }
        FastRand(float x, float y, float z) : FastRand(Hash(x), Hash(y), Hash(z)) { }
        FastRand(float x, float y, float z, float w) : FastRand(Hash(x), Hash(y), Hash(z), Hash(w)) { }


        //Gets a random non-negative integer.
	    inline int NextInt()
	    {
            //I forget where this particular XORshift generator came from.
		    Seed = (Seed ^ 61) ^ (Seed >> 16);
		    Seed += (Seed << 3);
		    Seed ^= (Seed >> 4);
		    Seed *= 0x27d4eb2d;
		    Seed ^= (Seed >> 15);
            assert(Seed >= 0);
		    return Seed;
	    }
        //Gets a random float between 0 and 1.
	    inline float NextFloat()
        {
            //TODO: Check this out: https://www.reddit.com/r/askscience/comments/3kxj7u/many_prngs_use_bitwise_operations_for_their_speed/cv6ao96/

            const int b = 9999999;
		    return (float)(NextInt() % b) / (float)b;
	    }

        //Gets a random vector of length 1.
        inline Vector3f GetRandUnitVector()
        {
            float theta = 2.0f * (float)M_PI * NextFloat(),
                  z = -1.0f + (2.0f * NextFloat()),
                  temp = sqrt(1.0f - (z * z));
            return Vector3f(cosf(theta) * temp, sinf(theta) * temp, z);
        }
    };
}