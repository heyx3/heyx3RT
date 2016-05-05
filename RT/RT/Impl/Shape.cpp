#include "../Headers/Shape.h"


#include <assert.h>
#include <vector>
#include <thread>
#include <mutex>


namespace
{
    struct ShapeFact
    {
    public:
        std::string TypeName;
        Shape*(*Factory)();

        ShapeFact(const std::string& typeName, Shape*(*factory)())
            : TypeName(typeName), Factory(factory)
        { }
    };

    std::mutex useVectorMutex;

    std::vector<ShapeFact>& GetFactoryVector()
    {
        static std::vector<ShapeFact> factories;
        return factories;
    }
}

void Shape::AddReflectionData(const std::string& typeName, ShapeFactory factory)
{
    std::lock_guard<std::mutex> lock(useVectorMutex);

    std::vector<ShapeFact>& factories = GetFactoryVector();
    for (int i = 0; i < factories.size(); ++i)
    {
        if (factories[i].TypeName == typeName)
        {
            assert(false);
            return;
        }
    }

    factories.push_back(ShapeFact(typeName, factory));
}
Shape::ShapeFactory Shape::GetFactory(const std::string& typeName)
{
    std::lock_guard<std::mutex> lock(useVectorMutex);

    ShapeFactory foundFactory = nullptr;

    std::vector<ShapeFact>& factories = GetFactoryVector();
    for (int i = 0; i < factories.size(); ++i)
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