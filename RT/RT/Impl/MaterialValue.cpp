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


void MaterialValue::WriteValue(const MaterialValue* mv,
                               const ConstMaterialValueToID& idLookup,
                               DataWriter& writer, const String& name)
{
    writer.WriteString(mv->GetTypeName(), name + "Type");
    writer.WriteUInt(idLookup[mv], name + "ID");
    mv->WriteData(writer, name, idLookup);
}

unsigned int MaterialValue::ReadValue(Ptr& outMV, NodeToChildIDs& childIDLookup,
                                      DataReader& reader, const String name)
{
    String typeName;
    reader.ReadString(typeName, name + "Type");

    outMV = Create(typeName);
    outMV->ReadData(reader, name, childIDLookup);

    unsigned int u;
    reader.ReadUInt(u, name + "ID");
    return u;
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

void MaterialValue::WriteData(DataWriter& data, const String& namePrefix,
                              const ConstMaterialValueToID& idLookup) const
{
    //Get child IDs.
    IDList childIDs;
    childIDs.Resize(GetNChildren());
    for (size_t i = 0; i < childIDs.GetSize(); ++i)
        childIDs[i] = idLookup[GetChild(i)];

    //Write child IDs.
    data.WriteList<unsigned int>(childIDs.GetData(), childIDs.GetSize(),
                                 [](DataWriter& wr, const unsigned int& val, const String& name)
                                 {
                                     wr.WriteUInt(val, name);
                                 },
                                 namePrefix + "childrenIDs");
}

void MaterialValue::ReadData(DataReader& data, const String& namePrefix,
                             NodeToChildIDs& childIDLookup)
{
    //Get child IDs.
    IDList childIDs;
    data.ReadList<unsigned int>(&childIDs,
                                [](void* pList, size_t nElements)
                                {
                                    ((IDList*)pList)->Resize(nElements);
                                },
                                [](DataReader& rd, void* pList, size_t listIndex, const String& name)
                                {
                                    rd.ReadUInt((*(IDList*)pList)[listIndex], name);
                                },
                                namePrefix + "childrenIDs");
    childIDLookup[this] = childIDs;
}
void MaterialValue::OnDoneReadingData(const IDToMaterialValue& mvLookup,
                                      const NodeToChildIDs& childIDLookup)
{
    //Add the children.
    const IDList& childIDs = *childIDLookup.TryGet(this);
    for (size_t i = 0; i < childIDs.GetSize(); ++i)
        SetChild(i, mvLookup[childIDs[i]]);
}