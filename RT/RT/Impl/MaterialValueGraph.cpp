#include "../Headers/MaterialValueGraph.h"

using namespace RT;


void MaterialValueGraph::WriteData(DataWriter& data) const
{
    //Make a unique ID for every node.

    std::vector<const MaterialValue*> allNodes;
    ConstMaterialValueToID mvToID;
    unsigned int nextID = std::numeric_limits<unsigned int>::min();
    unsigned int nNodes = 0;

    List<const MaterialValue*> toInvestigate;
    for (int i = 0; i < OUT_rootVals.GetSize(); ++i)
        toInvestigate.PushBack(OUT_rootVals[i]);

    while (toInvestigate.GetSize() > 0)
    {
        auto mv = toInvestigate.PopBack();
        nNodes += 1;

        if (mvToID.Contains(mv))
            continue;

        allNodes.push_back(mv);

        mvToID[mv] = nextID;
        nextID += 1;

        for (int i = 0; i < mv->GetNChildren(); ++i)
            toInvestigate.PushBack(mv->GetChild(i));
    }

    
    //Write the nodes.
    data.WriteList<const MaterialValue*>(allNodes.data(), allNodes.size(),
                                         [](DataWriter& wr, const MaterialValue*const& val,
                                            const String& name, void* pData)
                                         {
                                             MaterialValue::WriteValue(val,
                                                                       *(ConstMaterialValueToID*)pData,
                                                                       wr, name);
                                         },
                                         "nodes",
                                         &mvToID);

    //Write out the root values as IDs.
    List<unsigned int> rootValIDs;
    for (size_t i = 0; i < OUT_rootVals.GetSize(); ++i)
        rootValIDs.PushBack(mvToID[OUT_rootVals[i]]);
    data.WriteList<unsigned int>(rootValIDs.GetData(), rootValIDs.GetSize(),
                                 [](DataWriter& wr, const unsigned int& val,
                                    const String& name)
                                 {
                                     wr.WriteUInt(val, name);
                                 },
                                 "rootVals");
}

void MaterialValueGraph::ReadData(DataReader& data)
{
    NodeToChildIDs childIDs;
    IDToMaterialValue idLookup;

    //Read each node.
    struct ReaderData
    {
        NodeToChildIDs& childIDs;
        IDToMaterialValue& idLookup;
        ReaderData(NodeToChildIDs& _childIDs, IDToMaterialValue& _idLookup)
            : childIDs(_childIDs), idLookup(_idLookup) { }
    };
    std::vector<MaterialValue::Ptr> allNodes;
    ReaderData rdData(childIDs, idLookup);
    data.ReadList<MaterialValue::Ptr>(&allNodes,
                                      [](void* pList, size_t newSize)
                                      {
                                          ((std::vector<MaterialValue::Ptr>*)pList)->resize(newSize);
                                      },
                                      [](DataReader& rd, void* pList, size_t i, const String& name,
                                         void* pData)
                                      {
                                          auto& list = *(std::vector<MaterialValue::Ptr>*)pList;
                                          ReaderData& rdData = *(ReaderData*)pData;

                                          unsigned int id = MaterialValue::ReadValue(list[i],
                                                                                     rdData.childIDs,
                                                                                     rd, name);
                                          rdData.idLookup[id] = list[i];
                                      },
                                      "nodes",
                                      &rdData);

    //Finalize each node.
    for (auto& mvPtr : allNodes)
        mvPtr->OnDoneReadingData(idLookup, childIDs);

    //Read in the root values as IDs.
    List<unsigned int> rootValIDs;
    data.ReadList<unsigned int>(&rootValIDs,
                                [](void* pList, size_t newSize)
                                {
                                    ((List<unsigned int>*)pList)->Resize(newSize);
                                },
                                [](DataReader& rd, void* pList, size_t i, const String& name)
                                {
                                    rd.ReadUInt((*(List<unsigned int>*)pList)[i], name);
                                },
                                "rootVals");
    //Convert to MaterialValue pointers.
    IN_rootVals.Clear();
    for (size_t i = 0; i < rootValIDs.GetSize(); ++i)
        IN_rootVals.PushBack(idLookup[rootValIDs[i]]);
}