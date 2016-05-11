#pragma once


#include "Material.h"
#include "SkyMaterial.h"

#include "Camera.h"
#include "Texture2D.h"
#include "SmartPtrs.h"
#include "DataSerialization.h"


//A shape and its material.
//Note that deserializing this class will result in an unmanaged, heap-allocated shape and material.
struct RT_API ShapeAndMat : public ISerializable
{
public:

    SharedPtr<Shape> Shpe;
    SharedPtr<Material> Mat;

    ShapeAndMat() : Shpe(nullptr), Mat(nullptr) { }
    ShapeAndMat(SharedPtr<Shape> shpe, SharedPtr<Material> mat) : Shpe(shpe), Mat(mat) { }

    virtual void WriteData(DataWriter& writer) const override;
    virtual void ReadData(DataReader& reader) override;
};


#pragma warning(disable: 4251)
EXPORT_STL_VECTOR(ShapeAndMat);
#pragma warning(default: 4251)


//A ray-tracer.
class RT_API Tracer : public ISerializable
{
public:


    //Used to get the color when a ray doesn't hit anything.
    SharedPtr<SkyMaterial> SkyMat;
    
    //The shapes in the scene, with their corresponding materials.
    std::vector<ShapeAndMat> Objects;


    Tracer() { }
    Tracer(SkyMaterial* skyMat, const std::vector<ShapeAndMat>& objects);


    //Precomputes some data for all the shapes in this tracer.
    //Call this after all scene objects are finalized and before any tracing is done.
    void PrecalcData() { for (auto& sm : Objects) sm.Shpe->PrecalcData(); }

    //Traces the given ray through the scene to see what it hits.
    //Returns whether the ray hit anything.
    //Note that the ray may be redirected as it hits certain kinds of objects.
    bool TraceRay(size_t bounce, size_t maxBounces, Ray& ray, FastRand& prng,
                  Vector3f& outColor, Vertex& outHit, float& outDist) const;

    //Renders this scene into the given horizontal chunk of the given texture.
    void TraceImage(const Camera& cam, Texture2D& outTex, size_t startY, size_t endY, size_t maxBounces,
                    float gamma = 2.0f, size_t samplesPerPixel = 100) const;

    //Renders this scene into the given image,
    //    splitting the work across the given number of threads.
    //Blocks this thread until finished.
    //Note that passing 1 for the number of threads means that no extra threads will be created.
    void TraceFullImage(const Camera& cam, Texture2D& outTex, size_t nThreads, size_t maxBounces,
                        float gamma = 2.0f, size_t samplesPerPixel = 100) const;


    virtual void ReadData(DataReader& data) override;
    virtual void WriteData(DataWriter& data) const override;
};