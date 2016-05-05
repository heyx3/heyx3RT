#pragma once


#include "Main.hpp"
#include "FastRand.h"
#include "Shape.h"
#include "Camera.h"
#include "Texture2D.h"
#include "DataSerialization.h"


class Material;
class SkyMaterial;


//A shape and its material.
//Note that deserializing this class will result in an unmanaged, heap-allocated shape and material.
struct RT_API ShapeAndMat : public ISerializable
{
public:

    Shape* Shpe;
    Material* Mat;


    ShapeAndMat() : Shpe(nullptr), Mat(nullptr) { }
    ShapeAndMat(Shape* shpe, Material* mat) : Shpe(shpe), Mat(mat) { }


    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;
};

EXPORT_STL_VECTOR(ShapeAndMat);


//A ray-tracer.
class RT_API Tracer : public ISerializable
{
public:


    //Used to get the color when a ray doesn't hit anything.
    const SkyMaterial* SkyMat;
    
    //The shapes in the scene, with their corresponding materials.
    std::vector<ShapeAndMat> Objects;


    Tracer(const SkyMaterial* skyMat, const std::vector<ShapeAndMat>& objects);


    //Traces the given ray through the scene to see what it hits.
    //Returns whether the ray hit anything.
    //Note that the ray may be redirected as it hits certain kinds of objects.
    bool TraceRay(int bounce, int maxBounces, Ray& ray, FastRand& prng,
                  Vector3f& outColor, Vertex& outHit, float& outDist) const;

    //Renders this scene into the given horizontal chunk of the given texture.
    void TraceImage(const Camera& cam, Texture2D& outTex, int startY, int endY, int maxBounces,
                    float gamma = 2.0f, int samplesPerPixel = 100) const;

    //Renders this scene into the given image,
    //    splitting the work across the given number of threads.
    //Blocks this thread until finished.
    //Note that passing 1 for the number of threads means that no extra threads will be created.
    void TraceFullImage(const Camera& cam, Texture2D& outTex, int nThreads, int maxBounces,
                        float gamma = 2.0f, int samplesPerPixel = 100) const;


    virtual void ReadData(DataReader& data) override;
    virtual void WriteData(DataWriter& data) const override;
};