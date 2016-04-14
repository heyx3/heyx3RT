#pragma once

#include <assert.h>

#include "Vector3f.h"


//A fast (but not cryptographically-strong) PRNG.
//Always generates non-negative numbers.
class RT_API FastRand
{
public:

	int Seed;

	
    FastRand(int seed = 1234567) : Seed(seed) { }
    FastRand(int x, int y) : Seed((x << 5) + x ^ y) { } //http://stackoverflow.com/questions/13812335/how-to-hash-an-int-in-c-sharp


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
