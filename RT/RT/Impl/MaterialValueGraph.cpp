#include "../Headers/MaterialValueGraph.h"

using namespace RT;


void MaterialValueGraph::WriteData(DataWriter& data) const
{
    //Make a unique ID for every node.

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

        mvToID[mv] = nextID;
        nextID += 1;

        for (int i = 0; i < mv->GetNChildren(); ++i)
            toInvestigate.PushBack(mv->GetChild(i));
    }


    //Write the nodes in order, from highest tree depth to the lowest.

    data.WriteUInt(nNodes, "NumbNodes");

    List<const MaterialValue*> toWrite;
    Dictionary<const MaterialValue*, bool> processedNodeYet;
    for (int i = 0; i < OUT_rootVals.GetSize(); ++i)
    {
        toWrite.PushBack(OUT_rootVals[i]);
        processedNodeYet[OUT_rootVals[i]] = false;
    }

    size_t count = 0;
    while (toWrite.GetSize() > 0)
    {
        auto mv = toWrite.GetBack();

        //If this node has already been processed, write it out and pop it off the stack.
        if (processedNodeYet[mv])
        {
            toWrite.PopBack();
            MaterialValue::WriteValue(mv, mvToID, data, String(count));
            count += 1;
        }
        //Otherwise, queue up its children to be processed first.
        else
        {
            processedNodeYet[mv] = true;
            for (size_t i = 0; i < mv->GetNChildren(); ++i)
            {
                auto childMV = mv->GetChild(i);

                //Add this child to the front of the stack.
                int pos = toWrite.IndexOf([&childMV](const MaterialValue* mv2)
                {
                    return childMV == mv2;
                });
                bool processedAlready = processedNodeYet.Get(childMV, false);

                if (pos == -1)
                {
                    if (processedAlready)
                    {
                        //We already wrote the node out.
                        continue;
                    }
                    else
                    {
                        //We haven't seen this node yet.
                        toWrite.PushBack(childMV);
                        processedNodeYet[childMV] = false;
                    }
                }
                else
                {
                    //The node has already been seen, but not processed,
                    //    so move it to the top of the stack.
                    toWrite.RemoveAt(pos);
                    toWrite.PushBack(childMV);
                }
            }
        }
    }

    //Finally, write out the root values as IDs.
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
    unsigned int nNodes;
    data.ReadUInt(nNodes, "NumbNodes");
    for (unsigned int i = 0; i < nNodes; ++i)
    {
        MaterialValue::Ptr mv;
        unsigned int id = MaterialValue::ReadValue(mv, childIDs, data, String(i));
        idLookup[id] = mv;
    }

    //Finalize each node.
    idLookup.DoToEach([&childIDs, &idLookup](unsigned int key)
    {
        idLookup[key]->OnDoneReadingData(idLookup, childIDs);
    });

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