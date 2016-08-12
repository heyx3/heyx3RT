#include "../Headers/Shape.h"


#include <assert.h>
#include <vector>
#include <thread>
#include <mutex>

using namespace RT;


namespace
{
    struct ShapeFact
    {
    public:
        String TypeName;
        Shape*(*Factory)();

        ShapeFact(const String& typeName, Shape*(*factory)())
            : TypeName(typeName), Factory(factory)
        { }
    };


    std::mutex& GetVectorMutex()
    {
        static std::mutex mutx;
        return mutx;
    }
    std::vector<ShapeFact>& GetFactoryVector()
    {
        static std::vector<ShapeFact> factories;
        return factories;
    }
}

void Shape::AddReflectionData(const String& typeName, ShapeFactory factory)
{
    std::lock_guard<std::mutex> lock(GetVectorMutex());

    std::vector<ShapeFact>& factories = GetFactoryVector();
    for (size_t i = 0; i < factories.size(); ++i)
    {
        if (factories[i].TypeName == typeName)
        {
            assert(false);
            return;
        }
    }

    factories.push_back(ShapeFact(typeName, factory));
}
Shape::ShapeFactory Shape::GetFactory(const String& typeName)
{
    std::lock_guard<std::mutex> lock(GetVectorMutex());

    ShapeFactory foundFactory = nullptr;

    std::vector<ShapeFact>& factories = GetFactoryVector();
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