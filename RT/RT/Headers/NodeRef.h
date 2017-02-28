#pragma once

#include "Main.hpp"


//This namespace defines a Bounding Volume Hierarchy -- a spacial data structure.
namespace BVH
{
    //"T" is the type being stored in the BVH.
    //"BVHRoot" is of type "Root<T, BoundsType, BoundsFactory>".
    //"BVHNode" is of type "Node<T, BVHRoot, BoundsType>".
    template<typename T, typename BVHRoot, typename BVHNode>
    //A reference to a node in the BVH.
    //Note that if the BVH has significantly changed since this reference was last used,
    //    then this reference has to do some work finding the new location of the node.
    class NodeRef
    {
    public:

        BVHRoot& Owner;


        NodeRef(BVHRoot& owner, int _id = -1)
            : Owner(owner), id(_id)
        {
            versionNum = Owner.versionNum + 1;
            UpdateIndex();
        }
        NodeRef(BVHRoot& owner, int _id, int _index)
            : Owner(owner), id(_id), index(_index)
        {
            versionNum = Owner.versionNum;
        }


        int GetID() const { return id; }
        int GetIndex() const { UpdateIndex(); return index; }

        bool IsValid() const { return GetIndex() >= 0; }
        Node<T>& GetNode() const { return Owner.nodes[GetIndex()]; }


    private:

        int id;

        mutable unsigned int versionNum;
        mutable int index;


        void UpdateIndex() const
        {
            if (versionNum != Owner.versionNum)
            {
                versionNum = Owner.versionNum;
                if (id < 0)
                    index = -1;
                else
                {
                    bool succeeded;
                    std::tie(succeeded, index) = Owner.GetIndex(id);
                }
            }
        }
    };
}