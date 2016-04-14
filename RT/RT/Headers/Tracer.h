#pragma once

#include <vector>
#include <unordered_map>

#include "Main.hpp"
#include "FastRand.h"
#include "Shape.h"
#include "Camera.h"
#include "Texture2D.h"


class Material;
class SkyMaterial;


//A thread-safe ray-tracer.
class RT_API Tracer
{
public:

    int MaxBounces;

    //Used to get the color when a ray doesn't hit anything.
    const SkyMaterial* SkyMat;


    Tracer(int nShapes, const SkyMaterial* skyMat,
           const Shape** shapePtrArray, const Material** materialPtrArray,
           int maxBounces = 10);

    ~Tracer();


    //Traces the given ray through the scene to see what it hits.
    //Returns whether the ray hit anything.
    //Note that the ray may be redirected as it hits certain kinds of objects.
    bool TraceRay(int bounce, Ray& ray, FastRand& prng,
                  Vector3f& outColor, Vertex& outHit, float& outDist) const;

    //Renders this scene into the given horizontal chunk of the given texture.
    void TraceImage(const Camera& cam, Texture2D& outTex, int startY, int endY,
                    float gamma = 2.0f, int samplesPerPixel = 100) const;

    //Renders this scene into the given image,
    //    splitting the work across the given number of threads.
    //Blocks this thread until finished.
    //Note that passing 1 for the number of threads means that no extra threads will be created.
    void TraceFullImage(const Camera& cam, Texture2D& outTex, int nThreads,
                        float gamma = 2.0f, int samplesPerPixel = 100) const;
  

private:
 
    int nShapes;
    const Shape** shapes;
    const Material** shapeMats;
};