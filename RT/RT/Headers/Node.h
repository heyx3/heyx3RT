#pragma once

#include "Main.hpp"
#include "NodeRef.h"

#include <assert.h>
#include <vector>
#include <algorithm>


//This namespace defines a Bounding Volume Hierarchy -- a spacial data structure.
namespace BVH
{
    //"T" is the type of item being stored in the BVH.
    //"BVHRoot" is of type "Root<T, BoundsType>".
    //"BoundType" should have the same interface as "BVH::Bounds<>".
    //"BoundsFactory" is a function that gets the bounds of a given T.
    //    It should have the signature "BoundsType f(const T& value)".
    template<typename T, typename BVHRoot, typename BoundsType, typename BoundsFactory>
    //A node in the BVH hierarchy.
    //Is either a leaf (which means it has a list of elements)
    //    or not a leaf (which means it has two child nodes).
    class Node
    {
    public:

        typedef BoundsType Bounds;
        typedef Node<T, BVHRoot> ThisType;
        typedef NodeRef<T, BVHRoot, ThisType> Ref;


        //Creates a leaf node with no elements.
        Node(BVHRoot& owner, int _id, unsigned int threshold)
            : id(_id), child1(owner), child2(owner), nTotalItems(0)
        {
            leafData.reserve(threshold);
        }


        bool IsValid() const { return id >= 0; }

        int GetID() const { return id; }
        BoundsType GetBounds() const { return bnds; }

        bool IsLeaf() const { return child1.GetID() == -1; }

        const Ref& GetChild1() const { assert(!IsLeaf()); return child1; }
        const Ref& GetChild2() const { assert(!IsLeaf()); return child2; }

        //Gets the number of elements contained in this node/its children.
        unsigned int GetNElements() const { return nTotalItems; }

        //Gets the element at the given index in this leaf node.
        const T& Get(unsigned int elementIndex) const { assert(IsLeaf()); return leafData[elementIndex]; }
        //Gets the element at the given index in this leaf node.
        T& Get(unsigned int elementIndex) { assert(IsLeaf()); return leafData[elementIndex]; }

        //Returns the index of the given item in this leaf node, or -1 if it wasn't found.
        int IndexOf(const T& toFind) const
        {
            assert(IsLeaf());
            for (int i = 0; i < leafData.size(); ++i)
                if (leafData[i] == toFind)
                    return i;
            return -1;
        }

        //Adds the given element to this leaf node.
        void AddElement(const T& element)
        {
            assert(IsLeaf());

            nTotalItems += 1;
            leafData.push_back(element);
        }
        //Tries to remove the given element from this leaf node.
        //Returns whether the given element was found and removed.
        bool RemoveElement(const T& element)
        {
            assert(IsLeaf());
            for (unsigned int i = 0; i < leafData.size(); ++i)
            {
                if (leafData[i] == element)
                {
                    leafData.erase(leafData.begin() + i);

                    assert(nTotalItems > 0);
                    nTotalItems -= 1;

                    ComputeBounds();
                    return true;
                }
            }
            return false;
        }


        //Assuming this node is a leaf, splits it into the given two child nodes.
        //The objects are sorted into either child based on the given axis of their position.
        //The child nodes' bounds are then computed.
        void Split(Ref _child1, Ref _child2, unsigned int sortAxis)
        {
            assert(IsLeaf());

            child1 = _child1;
            child2 = _child2;

            //Sort the elements by position along the given axis.
            unsigned int axis = sortAxis;
            std::sort(leafData.begin(), leafData.end(),
                      [axis](const T& t1, const T& t2)
            {
                return BoundsFactory(t1).GetCenter()[axis] <
                    BoundsFactory(t2).GetCenter()[axis];
            });

            //Distribute the elements among the children.
            for (unsigned int i = 0; i < leafData.size(); ++i)
                if (i < leafData.size() / 2)
                    child1.GetNode().leafData.push_back(leafData[i]);
                else
                    child2.GetNode().leafData.push_back(leafData[i]);
            leafData.clear();

            child1.GetNode().ComputeBounds();
            child2.GetNode().ComputeBounds();
        }
        //Computes the bounds of this node based on the elements it contains.
        //Works for any kind of node (leaf and non-leaf).
        void ComputeBounds()
        {
            if (IsLeaf())
            {
                if (!leafData.empty())
                {
                    bnds = BoundsFactory(leafData[0]);
                    for (unsigned int i = 1; i < leafData.size(); ++i)
                        bnds = bnds.Union(BoundsFactory(leafData[i]));
                }
            }
            else
            {
                bnds = child1.GetNode().bnds.Union(child2.GetNode().bnds);
            }
        }
        //Adds the given bounds into this node's bounds.
        void AddBounds(const Bounds& _bnds) { bnds = bnds.Union(_bnds); }


    private:

        int id;
        BoundsType bnds;
        unsigned int nTotalItems;

        Ref child1, child2;

        std::vector<T> leafData;


        Node(const ThisType& cpy) = delete;
        ThisType& operator=(const ThisType& cpy) = delete;
    };
}