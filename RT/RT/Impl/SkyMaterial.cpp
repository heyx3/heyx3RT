#include "../Headers/SkyMaterial.h"


#include <vector>
#include <thread>
#include <mutex>

using namespace RT;


namespace
{
    struct SMatFact
    {
    public:
        String TypeName;
        SkyMaterial*(*Factory)();

        SMatFact(const String& typeName, SkyMaterial*(*factory)())
            : TypeName(typeName), Factory(factory) { }
    };

    std::mutex& GetVectorMutex()
    {
        static std::mutex mutx;
        return mutx;
    }
    std::vector<SMatFact>& GetFactoryVector()
    {
        static std::vector<SMatFact> factories;
        return factories;
    }
}

void SkyMaterial::AddReflectionData(const String& typeName, SkyMatFactory factory)
{
    std::lock_guard<std::mutex> lock(GetVectorMutex());

    std::vector<SMatFact>& factories = GetFactoryVector();
    for (size_t i = 0; i < factories.size(); ++i)
    {
        if (factories[i].TypeName == typeName)
        {
            assert(false);
            return;
        }
    }

    factories.push_back(SMatFact(typeName, factory));
}
SkyMaterial::SkyMatFactory SkyMaterial::GetFactory(const String& typeName)
{
    std::lock_guard<std::mutex> lock(GetVectorMutex());

    SkyMatFactory foundFactory = nullptr;

    std::vector<SMatFact>& factories = GetFactoryVector();
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