#include "../Headers/Tracer.h"

#include <assert.h>

#include "../Headers/FastRand.h"
#include "../Headers/Material.h"
#include "../Headers/SkyMaterial.h"


#ifdef OS_WINDOWS

#else
#include <pthread.h>
#endif


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
        float gamma;
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
        d.tracer->TraceImage(*d.cam, *d.tex, d.startY, d.endY, d.bounces, d.gamma, d.samples);
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


Tracer::Tracer(SkyMaterial* skyMat, const std::vector<ShapeAndMat>& objects)
    : Objects(objects), SkyMat(skyMat)
{
}

bool Tracer::TraceRay(size_t bounce, size_t maxBounces,
                      Ray& ray, FastRand& prng,
                      Vector3f& outColor, Vertex& outHit, float& outDist) const
{
    if (bounce >= maxBounces)
    {
        outColor = SkyMat->GetColor(ray, prng);
        return false;
    }

    outDist = std::numeric_limits<float>().infinity();
    int closestShape = -1;

    //Get the closest intersection with a shape.
    for (size_t i = 0; i < Objects.size(); ++i)
    {
        float tempDist;
        Vertex tempHit;
        if (Objects[i].Shpe->CastRay(ray, tempHit))
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

    //Get the color of the shape's surface.
    if (closestShape != -1)
    {
        Ray newR;
        Vector3f atten;
        bool scattered = Objects[closestShape].Mat->Scatter(ray, outHit, *Objects[closestShape].Shpe,
                                                            prng, atten, newR);
        if (scattered)
        {
            Vector3f bounceCol;
            Vertex bounceHit;
            float bounceDist;
            TraceRay(bounce + 1, maxBounces, newR, prng, bounceCol, bounceHit, bounceDist);

            outColor = atten * bounceCol;
        }
        else
        {
            outColor = Vector3f();
        }

        return true;
    }

    //No shape was hit, so get the color of the sky.
    outColor = SkyMat->GetColor(ray, prng);
    return false;
}

void Tracer::TraceImage(const Camera& cam, Texture2D& tex,
                        size_t startY, size_t endY, size_t maxBounces,
                        float gamma, size_t nSamples) const
{
    float invSamples = 1.0f / (float)nSamples,
          invWidth = 1.0f / (float)tex.GetWidth(),
          invHeight = 1.0f / (float)tex.GetHeight();
    float gammaPow = 1.0f / gamma;

    for (size_t y = startY; y <= endY; ++y)
    {
        float fY = -0.5f + ((float)y  * invHeight);

        for (size_t x = 0; x < tex.GetWidth(); ++x)
        {
            float fX = (-0.5f + ((float)x * invWidth)) * cam.WidthOverHeight;

            //Average the result of a bunch of random samples inside the pixel.

            Vector3f color(0.0f, 0.0f, 0.0f);

            FastRand fr(x, y);

            for (size_t i = 0; i < nSamples; ++i)
            {
                float cX = fX + (fr.NextFloat() * invWidth),
                      cY = fY + (fr.NextFloat() * invHeight);
                Vector3f pixelPos = cam.Pos +
                                    (cam.GetForward() * 1.0f) +
                                    (cam.GetSideways() * cX) +
                                    (cam.GetUpward() * cY);
                Ray r(pixelPos, (pixelPos - cam.Pos).Normalize());

                Vector3f tempCol;
                Vertex outHit;
                float outDist;
                TraceRay(0, maxBounces, r, fr, tempCol, outHit, outDist);

                color += tempCol;
            }
            color *= invSamples;

            //Gamma-correction.
            color.x = pow(color.x, gammaPow);
            color.y = pow(color.y, gammaPow);
            color.z = pow(color.z, gammaPow);

            tex.SetColor(x, y, color);
        }
    }
}

void Tracer::TraceFullImage(const Camera& cam, Texture2D& tex,
                            size_t nThreads, size_t maxBounces,
                            float gamma, size_t nSamples) const
{
    nThreads = (nThreads > 1 ? nThreads : 1);
    int span = tex.GetHeight() / nThreads;

    ThreadDat dat;
    dat.cam = &cam;
    dat.tex = &tex;
    dat.tracer = this;
    dat.gamma = gamma;
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
        size_t endY = (i * span) + span - 1;
        dats[i].endY = (tex.GetHeight() - 1 < endY ? (tex.GetHeight() - 1) : endY);

#ifdef OS_WINDOWS
        threads.push_back(CreateThread(nullptr, 0, &RunTrace, &dats[i], 0, nullptr));
        assert(threads[threads.size() - 1]);
#else
        pthread_t threadID;
        pthread_create(&threadID, nullptr, &RunTrace, &dats[i]);
        threads.push_back(threadID);
#endif
    }
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
    writer.WriteList<ShapeAndMat>(Objects.data(), Objects.size(),
                                  [](DataWriter& writer, const ShapeAndMat& o, const std::string& name)
                                    { writer.WriteDataStructure(o, name); },
                                  "Objects");
}
void Tracer::ReadData(DataReader& reader)
{
    SkyMaterial* smPtr;
    SkyMaterial::ReadValue(smPtr, reader, "SkyMaterial");
    SkyMat.Reset(smPtr);

    reader.ReadList<ShapeAndMat>(&Objects,
                                 [](void* pList, size_t nElements)
                                    { ((std::vector<ShapeAndMat>*)pList)->resize(nElements); },
                                 [](DataReader& rd, void* pList, size_t i, const std::string& name)
                                    { rd.ReadDataStructure((*((std::vector<ShapeAndMat>*)pList))[i], name); },
                                 "Objects");
}