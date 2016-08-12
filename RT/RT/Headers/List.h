#pragma once

#include "Main.hpp"

#include <vector>


namespace RT
{
    template<typename T>
    class List
    {
    public:

        size_t GetSize() const { return vec.size(); }

        T* GetData() { return vec.data(); }
        const T* GetData() const { return vec.data(); }

        //Sets this vector to increase/decrease its size to the given value.
        void Resize(size_t n) { vec.resize(n); }
        //Reserves at least the given amount of space in this vector.
        void Reserve(size_t n) { vec.reserve(n); }

        //Adds the given value to the end of this vector.
        void PushBack(const T& value) { vec.push_back(value); }
        //Adds the given value to the beginning of this vector.
        void PushFront(const T& value) { Insert(0, value); }

        void Insert(size_t i, const T& value) { vec.insert(vec.begin() + i, value); }
        void Clear() { vec.clear(); }

        template<typename Predicate>
        int IndexOf(Predicate p) const
        {
            for (int i = 0; i < GetSize(); ++i)
                if (p(vec[i]))
                    return i;
            return -1;
        }

        T& operator[](size_t i) { return vec[i]; }
        const T& operator[](size_t i) const { return vec[i]; }

        T& GetBack() { return vec[vec.size() - 1]; }
        const T& GetBack() const { return vec[vec.size() - 1]; }


    private:

        std::vector<T> vec;
    };

#define EXPORT_RT_LIST(elementType) template class RT_API List<elementType>;
}