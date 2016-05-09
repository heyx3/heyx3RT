#include "../Headers/Material.h"

#include "../Headers/Matrix4f.h"

#include <vector>
#include <thread>
#include <mutex>


namespace
{
    struct MatFact
    {
    public:
        std::string TypeName;
        Material*(*Factory)();

        MatFact(const std::string& typeName, Material*(*factory)())
            : TypeName(typeName), Factory(factory) { }
    };

    std::mutex useVectorMutex;

    std::vector<MatFact>& GetFactoryVector()
    {
        static std::vector<MatFact> factories;
        return factories;
    }
}

void Material::AddReflectionData(const std::string& typeName, MaterialFactory factory)
{
    std::lock_guard<std::mutex> lock(useVectorMutex);

    std::vector<MatFact>& factories = GetFactoryVector();
    for (size_t i = 0; i < factories.size(); ++i)
    {
        if (factories[i].TypeName == typeName)
        {
            assert(false);
            return;
        }
    }

    factories.push_back(MatFact(typeName, factory));
}
Material::MaterialFactory Material::GetFactory(const std::string& typeName)
{
    std::lock_guard<std::mutex> lock(useVectorMutex);

    MaterialFactory foundFactory = nullptr;

    std::vector<MatFact>& factories = GetFactoryVector();
    for (size_t i = 0; i < factories.size(); ++i)
    {
        if (factories[i].TypeName == typeName)
        {
            foundFactory = factories[i].Factory;
            break;
        }
    }

    assert(foundFactory != nullptr);
    return foundFactory;
}


Vector3f Material::TangentSpaceToWorldSpace(const Vector3f& tangentSpaceNormal,
                                            const Vector3f& worldNormal,
                                            const Vector3f& worldTangent,
                                            const Vector3f& worldBitangent)
{
    Matrix4f tempMat, transfMat;
    tempMat.SetAsOrientation(worldNormal, worldBitangent, worldTangent);
    tempMat.GetTranspose(transfMat);

    return transfMat.ApplyVector(tangentSpaceNormal);
}