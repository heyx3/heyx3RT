#include "../Headers/Tracer.h"

#include <assert.h>

#include "../Headers/FastRand.h"
#include "../Headers/Material.h"
#include "../Headers/SkyMaterial.h"


#ifdef OS_WINDOWS

#else
#include <pthread.h>
#endif

//TODO: Use C++11 threading.

using namespace RT;

namespace
{
    float max(float f1, float f2) { return (f1 > f2) ? f1 : f2; }
    float min(float f1, float f2) { return (f1 > f2) ? f2 : f1; }
}


namespace
{
    struct ThreadDat
    {
        const Tracer* tracer;
        const Camera* cam;
        Texture2D* tex;
        float verticalFOVDegrees, aperture, focusDist;
        size_t samples, bounces;
        
        size_t startY, endY;
    };

#ifdef OS_WINDOWS
    DWORD WINAPI RunTrace(void* pDat)
    {
#else
    void* RunTrace(void* pDat)
    {
#endif
        ThreadDat& d = *(ThreadDat*)pDat;
        d.tracer->TraceImage(*d.cam, *d.tex, d.startY, d.endY, d.bounces,
                             d.verticalFOVDegrees, d.aperture, d.focusDist,
                             d.samples);
        return 0;
    }
}


void ShapeAndMat::WriteData(DataWriter& writer) const
{
    Shape::WriteValue(*Shpe, writer, "Shape");
    Material::WriteValue(*Mat, writer, "Material");
}
void ShapeAndMat::ReadData(DataReader& reader)
{
    Shape* sPtr;
    Shape::ReadValue(sPtr, reader, "Shape");
    Shpe.Reset(sPtr);

    Material* mPtr;
    Material::ReadValue(mPtr, reader, "Material");
    Mat.Reset(mPtr);
}


Tracer::Tracer(SkyMaterial* skyMat, const List<ShapeAndMat>& objects)
    : Objects(objects), SkyMat(skyMat)
{
}

const ShapeAndMat* Tracer::TraceRay(const Ray& ray, Vertex& outHit, FastRand& prng, float& outDist) const
{
    outDist = std::numeric_limits<float>().infinity();
    int closestShape = -1;

    //Get the closest intersection with a shape.
    for (size_t i = 0; i < Objects.GetSize(); ++i)
    {
        float tempDist;
        Vertex tempHit;
        if (Objects[i].Shpe->CastRay(ray, tempHit, prng))
        {
            tempDist = tempHit.Pos.Distance(ray.GetPos());
            if (tempDist < outDist)
            {
                outDist = tempDist;
                outHit = tempHit;
                closestShape = i;
            }
        }
    }

    if (closestShape < 0)
        return nullptr;
    else
        return &Objects[closestShape];
}
bool Tracer::TraceRay(size_t bounce, size_t maxBounces,
                      Ray& ray, FastRand& prng,
                      Vector3f& outColor, Vertex& outHit, float& outDist) const
{
    if (bounce >= maxBounces)
    {
        //The ray went too far; assume it's fully attenuated.
        outColor = Vector3f();
        return false;
    }

    const ShapeAndMat* hit = TraceRay(ray, outHit, prng, outDist);

    //Get the color of the shape's surface.
    if (hit != nullptr)
    {
        Ray newR;
        Vector3f atten, emissive;
        bool scattered = hit->Mat->Scatter(ray, outHit, *hit->Shpe, prng, atten, emissive, newR);
        if (scattered)
        {
            Vector3f bounceCol;
            Vertex bounceHit;
            float bounceDist;
            TraceRay(bounce + 1, maxBounces, newR, prng, bounceCol, bounceHit, bounceDist);

            outColor = emissive + (atten * bounceCol);
        }
        else
        {
            outColor = emissive;
        }

        return true;
    }

    //No shape was hit, so get the color of the sky.
    outColor = SkyMat->GetColor(ray, prng);
    return false;
}

void Tracer::TraceImage(const Camera& cam, Texture2D& tex,
                        size_t startY, size_t endY, size_t maxBounces,
                        float verticalFOVDegrees, float aperture, float focusDist,
                        size_t nSamples) const
{
    float invSamples = 1.0f / (float)nSamples,
          invWidth = 1.0f / (float)(tex.GetWidth() - 1),
          invHeight = 1.0f / (float)(tex.GetHeight() - 1),
          lensRadius = aperture / 2.0f;
    float aspectRatio = invHeight / invWidth,
          aspectRatioSqr = aspectRatio * aspectRatio;

    //Generate the ray's start on a disc of diameter "aperture" surrounding the circle.
    //Generate the target pixel's world position using a pixel grid projected forward to "focusDist".
    //To do this, we need some trig.
    float theta = verticalFOVDegrees * (float)M_PI / 180.0f;
    //Get the half-width/height when focus distance is 1.
    float halfHeightBase = tanf(theta / 2.0f),
          halfWidthBase = halfHeightBase * cam.WidthOverHeight;

    for (size_t y = startY; y <= endY; ++y)
    {
        //A measure from -1.0 to +1.0 of the camera-space Y position of the pixel.
        float fY = -1.0 + (2.0f * (float)y * invHeight);

        for (size_t x = 0; x < tex.GetWidth(); ++x)
        {
            //A measure from -1.0 to +1.0 of the camera-space X position of the pixel.
            float fX = -1.0f + (2.0f * (float)x * invWidth);
            fX *= aspectRatioSqr;

            //Average the result of a bunch of random samples inside the pixel.

            Vector3f color(0.0f, 0.0f, 0.0f);

            FastRand fr((int)x, (int)y);

            for (size_t i = 0; i < nSamples; ++i)
            {
                Vector2f lensOffset = fr.NextUnitVector2() * lensRadius;
                Vector3f worldLensOffset = (cam.GetSideways() * lensOffset.x) +
                                           (cam.GetUpward() * lensOffset.y);

                Vector2f pixelOffset(fr.NextFloat() * invWidth,
                                     fr.NextFloat() * invHeight);

                //Get the pixel position when the focus distance is 1,
                //    then scale that up based on the actual focus distance.
                //The trig works out so that it's a linear scale.
                Vector3f pixelPos = cam.Pos +
                                    (cam.GetForward() * focusDist) +
                                    (cam.GetSideways() * (fX + pixelOffset.x) * halfHeightBase * focusDist) +
                                    (cam.GetUpward() * (fY + pixelOffset.y) * halfWidthBase * focusDist);

                Vector3f rayStart = cam.Pos + worldLensOffset;
                Ray r(rayStart, (pixelPos - rayStart).Normalize());

                Vector3f tempCol;
                Vertex outHit;
                float outDist;
                TraceRay(0, maxBounces, r, fr, tempCol, outHit, outDist);

                color += tempCol;
            }
            color *= invSamples;

            tex.SetColor(x, y, color);
        }
    }
}

void Tracer::TraceFullImage(const Camera& cam, Texture2D& tex,
                            size_t nThreads, size_t maxBounces,
                            float verticalFOVDegrees, float aperture, float focusDist,
                            size_t nSamples) const
{
    nThreads = (nThreads > 1 ? nThreads : 1);
    size_t span = tex.GetHeight() / nThreads;

    ThreadDat dat;
    dat.cam = &cam;
    dat.tex = &tex;
    dat.tracer = this;
    dat.verticalFOVDegrees = verticalFOVDegrees;
    dat.aperture = aperture;
    dat.focusDist = focusDist;
    dat.bounces = maxBounces;
    dat.samples = nSamples;
    
    std::vector<ThreadDat> dats;
    dats.resize(nThreads);

#ifdef OS_WINDOWS
    std::vector<HANDLE> threads;
#else
    std::vector<pthread_t> threads;
#endif

    //The 0th thread is this one we're currently in.
    for (size_t i = 1; i < nThreads; ++i)
    {
        dats[i] = dat;
        dats[i].startY = i * span;

        assert(tex.GetHeight() > 0);
        if (i == nThreads - 1)
        {
            dats[i].endY = tex.GetHeight() - 1;
        }
        else
        {
            size_t endY = (i * span) + span - 1;
            dats[i].endY = (endY >= tex.GetHeight() ? (tex.GetHeight() - 1) : endY);
        }


#ifdef OS_WINDOWS
        threads.push_back(CreateThread(nullptr, 0, &RunTrace, &dats[i], 0, nullptr));
        assert(threads[threads.size() - 1]);
#else
        pthread_t threadID;
        pthread_create(&threadID, nullptr, &RunTrace, &dats[i]);
        threads.push_back(threadID);
#endif
    }
    //Run the last bit of the task in this thread.
    assert(span > 0);
    dats[0] = dat;
    dats[0].startY = 0;
    dats[0].endY = span - 1;
    RunTrace(&dats[0]);

    //Now wait for all the other threads to complete.
    for (size_t i = 0; i < threads.size(); ++i)
    {
#ifdef OS_WINDOWS
        WaitForSingleObject(threads[i], INFINITE);
#else
        void* dummy;
        pthread_join(threads[i], &dummy);
#endif
    }
}

void Tracer::WriteData(DataWriter& writer) const
{
    SkyMaterial::WriteValue(*SkyMat, writer, "SkyMaterial");
    writer.WriteList<ShapeAndMat>(Objects.GetData(), Objects.GetSize(),
                                  [](DataWriter& writer, const ShapeAndMat& o, const String& name)
                                    { writer.WriteDataStructure(o, name); },
                                  "Objects");
}
void Tracer::ReadData(DataReader& reader)
{
    SkyMaterial::ReadValue(SkyMat, reader, "SkyMaterial");

    Objects.Clear();
    reader.ReadList<ShapeAndMat>(&Objects,
                                 [](void* pList, size_t nElements)
                                    { ((std::vector<ShapeAndMat>*)pList)->resize(nElements); },
                                 [](DataReader& rd, void* pList, size_t i, const String& name)
                                    { rd.ReadDataStructure((*((std::vector<ShapeAndMat>*)pList))[i], name); },
                                 "Objects");
}