#include "../Headers/MaterialValue.h"

#include <vector>
#include <thread>
#include <mutex>
#include <assert.h>
#include <iostream>

using namespace RT;


namespace
{
    struct MVFact
    {
    public:
        String TypeName;
        MaterialValue*(*Factory)();

        MVFact(const String& typeName, MaterialValue*(*factory)())
            : TypeName(typeName), Factory(factory) { }
    };

    std::mutex& GetVectorMutex()
    {
        static std::mutex mutx;
        return mutx;
    }
    std::vector<MVFact>& GetFactoryVector()
    {
        static std::vector<MVFact> factories;
        return factories;
    }
}

void MaterialValue::AddReflectionData(const String& typeName, MVFactory factory)
{
    std::lock_guard<std::mutex> lock(GetVectorMutex());

    std::vector<MVFact>& factories = GetFactoryVector();
    for (size_t i = 0; i < factories.size(); ++i)
    {
        if (factories[i].TypeName == typeName)
        {
            assert(false);
            return;
        }
    }

    factories.push_back(MVFact(typeName, factory));
}
MaterialValue::MVFactory MaterialValue::GetFactory(const String& typeName)
{
    std::lock_guard<std::mutex> lock(GetVectorMutex());

    MVFactory foundFactory = nullptr;

    std::vector<MVFact>& factories = GetFactoryVector();
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


void MaterialValue::AssertExists(const Shape* shpe) const
{
    if (shpe == nullptr)
    {
        std::cout << "\nERROR: MaterialValue \"" << GetTypeName().CStr() <<
                     "\" requires a Shape to compute value, but it doesn't exist in this context!\n\n";
        char dummy;
        std::cin >> dummy;
    }
}
void MaterialValue::AssertExists(const Vertex* surface) const
{
    if (surface == nullptr)
    {
        std::cout << "\nERROR: MaterialValue \"" << GetTypeName().CStr() <<
                     "\" requires a Vertex to compute value, but it doesn't exist in this context!\n\n";
        char dummy;
        std::cin >> dummy;
    }
}