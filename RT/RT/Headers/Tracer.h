#pragma once


#include "List.h"

#include "Material.h"
#include "SkyMaterial.h"

#include "Camera.h"
#include "Texture2D.h"
#include "SmartPtrs.h"
#include "DataSerialization.h"


namespace RT
{
    EXPORT_SHAREDPTR(Shape);
    EXPORT_SHAREDPTR(Material);

    //A shape and its material.
    struct RT_API ShapeAndMat : public ISerializable
    {
    public:

        SharedPtr<Shape> Shpe;
        SharedPtr<Material> Mat;

        ShapeAndMat() : Shpe(nullptr), Mat(nullptr) { }
        ShapeAndMat(SharedPtr<Shape> shpe, SharedPtr<Material> mat) : Shpe(shpe), Mat(mat) { }

        virtual void WriteData(DataWriter& writer) const override;
        virtual void ReadData(DataReader& reader) override;

        bool operator==(const ShapeAndMat& other) const { return Shpe.Get() == other.Shpe.Get() &&
                                                                 Mat.Get() == other.Mat.Get(); }
    };

    EXPORT_RT_LIST(ShapeAndMat);
}

namespace RT
{
    EXPORT_SHAREDPTR(SkyMaterial);

    //A ray-tracer/scene.
    class RT_API Tracer : public ISerializable
    {
    public:


        //Used to get the color when a ray doesn't hit anything.
        SharedPtr<SkyMaterial> SkyMat;
    
        //The shapes in the scene, with their corresponding materials.
        List<ShapeAndMat> Objects;


        Tracer() { }
        Tracer(SkyMaterial* skyMat, const List<ShapeAndMat>& objects);


        //Precomputes some data for all the shapes in this tracer.
        //Call this after all scene objects are finalized and before any tracing is done.
        void PrecalcData() { for (size_t i = 0; i < Objects.GetSize(); ++i) Objects[i].Shpe->PrecalcData(); }

        //Traces the given ray through the scene to see what it hits.
        //Returns the shape that was hit, or null if nothing was hit.
        const ShapeAndMat* TraceRay(const Ray& ray, Vertex& outHit,
                                    FastRand& prng, float& outDist) const;
        //Traces the given ray through the scene to see what it hits.
        //Returns the shape that was hit, or null if nothing was hit.
        ShapeAndMat* TraceRay(const Ray& ray, Vertex& outHit,
                              FastRand& prng, float& outDist)
            { return (ShapeAndMat*)((const Tracer*)this)->TraceRay(ray, outHit, prng, outDist); }

        //Traces the given ray through the scene to see what it hits.
        //Returns whether the ray hit anything.
        //Note that the ray may be redirected as it hits certain kinds of objects.
        bool TraceRay(size_t bounce, size_t maxBounces, Ray& ray, FastRand& prng,
                      Vector3f& outColor, Vertex& outHit, float& outDist) const;

        //Renders this scene into the given horizontal chunk of the given texture.
        void TraceImage(const Camera& cam, Texture2D& outTex,
                        size_t startY, size_t endY, size_t maxBounces,
                        float verticalFOVDegrees, float aperture, float focusDist,
                        size_t samplesPerPixel) const;

        //Renders this scene into the given image,
        //    splitting the work across the given number of threads.
        //Blocks this thread until finished.
        //Note that passing 1 for the number of threads means that no extra threads will be created.
        void TraceFullImage(const Camera& cam, Texture2D& outTex,
                            size_t nThreads, size_t maxBounces,
                            float verticalFOVDegrees, float aperture, float focusDist,
                            size_t samplesPerPixel) const;


        virtual void ReadData(DataReader& data) override;
        virtual void WriteData(DataWriter& data) const override;
    };
}