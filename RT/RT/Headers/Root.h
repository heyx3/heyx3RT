#pragma once

#include "Node.h"

#include <unordered_map>
#include <tuple>


//This namespace defines a Bounding Volume Hierarchy -- a spacial data structure.
namespace BVH
{
    //TOOD: Template parameter for number of child nodes (default is 2 for binary tree). Use template parameter so that child references are stored on the stack with the Node instance.

    //"T" is the type of element being stored in this BVH.
    //"BoundsType" is the type of bounding space. Should have the same interface as "BVH::Bounds<>".
    //"BoundsFactory" is a function that creates a bounding space for a given element.
    //    It should have the signature "BoundsType f(const T& element)".
    template<typename T, typename BoundsType, typename BoundsFactory>
    //The root of a BVH.
    //Changing it is not thread-safe, but querying it can be after calling "PrepareThreadSafeQuerying()".
    class Root
    {
    public:

        typedef Root<T, BoundsType, BoundsFactory> ThisType;
        typedef Node<T, ThisType, BoundsType, BoundsFactory> NodeType;

        friend NodeType::Ref;


        //Which axis to sort items along: 0 = X, 1 = Y, etc.
        unsigned int SortingAxis = 0;

        //How many elements a leaf can hold before it splits into two child leaves.
        unsigned int Threshold = 10;


        unsigned int GetNumbNodes() const { return count; }

        void Add(const T& t)
        {
            count += 1;

            //Search down through the tree to find a leaf to add the element to.

            unsigned int nodeRefIndex = GetRootIndex();
            BoundsType bnds = BoundsFactory(t);
            BoundsType::MyPosType::MyComponent centerPos = bnds.GetCenter()[SortingAxis];

            while (!nodes[nodeRefIndex].IsLeaf())
            {
                //Update the bounds of this node while we're here.
                nodes[nodeRefIndex].AddBounds(bnds);

                //Choose the child whose bounds are closer to this element's center position.

                ThisType &c1 = nodes[nodeRefIndex].GetChild1().GetNode(),
                    &c2 = nodes[nodeRefIndex].GetChild2().GetNode();
                float dist1 = abs(centerPos - c1.bnds.GetCenter()[SortingAxis]),
                    dist2 = abs(centerPos - c2.bnds.GetCenter()[SortingAxis]);

                if (dist1 < dist2)
                    c1.AddElement(element, sortingAxis);
                else
                    c2.AddElement(element, sortingAxis);
            }

            //Add the element to the leaf, splitting it into smaller leaves if necessary.
            if (nodes[nodeRefIndex].GetNElements() > Threshold)
            {
                int id1, id2;
                unsigned int index1, index2;
                std::tie(id1, index1) = MakeNode();
                std::tie(id2, index2) = MakeNode();

                nodes[nodeRefIndex].Split(NodeType::Ref(*this, id1),
                                          NodeType::Ref(*this, id2, index2),
                                          SortingAxis);
            }
            else
            {
                nodes[nodeRefIndex].AddElement(t);
            }
        }

        //Returns whether the item was actually found and removed.
        bool Remove(const T& t)
        {
            return Remove_Helper(t, BoundsFactory(t), GetRootIndex());
        }

        void Clear()
        {
            nodes.clear();
            nextID = 0;
            count = 0;
            versionNum += 1;
        }


        //TODO: Implement Contains() and the various iterators.
        //TODO: Provide another Remove() that takes the iterator class.



        //After calling this function, all queries to this tree are thread-safe.
        //This thread-safety only extends to queries and not modifications to the tree --
        //    modifying it removes that thread-safety.
        //Note that "Contains()" is not made thread-safe by this call.
        void PrepareThreadSafeQuerying()
        {
            //Make sure all NodeRefs are up-to-date.
            for (int i = 0; i < nodes.size(); ++i)
            {
                if (!nodes[i].IsLeaf())
                {
                    nodes[i].GetChild1().GetIndex();
                    nodes[i].GetChild2().GetIndex();
                }
            }
        }


    private:

        unsigned int GetRootIndex() const { return 0; }

        const NodeType& GetRoot() const { return nodes[GetRootIndex()]; }
        NodeType& GetRoot() { return nodes[GetRootIndex()]; }

        //Returns the id and index of the new node, respectively.
        std::tuple<int, unsigned int> MakeNode()
        {
            nodes.push_back(NodeType(*this, nextID, Threshold));

            //Make sure we don't overflow.
            assert(nextID < (nextID + 1));
            nextID += 1;

            return std::tuple<int, unsigned int>(nodes[nodes.size() - 1].GetID(),
                                                 nodes.size() - 1);
        }
        //Removes the node at the given index.
        void DestroyNode(unsigned int index)
        {
            nodes.erase(nodes.begin() + index);
            versionNum += 1;
        }

        //Returns the index of the given node,
        //    as well as a bool indicating whether the given node actually exists.
        std::tuple<bool, unsigned int> GetIndex(int nodeID) const
        {
            if (nodes.size() == 0)
                return std::tuple<bool, unsigned int>(false, 0);

            //Use binary search, since all the nodes are in order by ID.
            int start = 0,
                end = nodes.size() - 1;
            while (start <= end)
            {
                int i = (start + end) / 2;

                if (nodes[i] == nodeID)
                {
                    assert(i >= 0);
                    return std::tuple<bool, unsigned int>(true, i);
                }
                else if (nodes[i].GetID() < nodeID)
                    start = i + 1;
                else
                    end = i - 1;
            }

            return std::tuple<bool, unsigned int>(false, 0);
        }

        //Searches and removes the given item from the tree starting at the given node.
        //Returns whether the item was found and removed.
        bool Remove_Helper(const T& t, const BoundsType& tBnds, int nodeIndex)
        {
            NodeType& n = nodes[nodeIndex];
            if (n.IsLeaf())
            {
                return n.RemoveElement(t);
            }
            else
            {
                const NodeType::Ref &c1 = n.GetChild1(),
                    &c2 = n.GetChild2();
                if ((c1.GetNode().GetBounds().Overlaps(tBnds) && Remove_Helper(t, tBnds, c1.GetIndex())) ||
                    (c2.GetNode().GetBounds().Overlaps(tBnds) && Remove_Helper(t, tBnds, c2.GetIndex())))
                {
                    n.ComputeBounds();
                    return true;
                }

                return false;
            }
        }


        std::vector<NodeType> nodes;

        unsigned int count = 0;
        int nextID = 0;
        unsigned int versionNum = 0;
    };
}