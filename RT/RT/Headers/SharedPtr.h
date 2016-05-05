#pragma once

#include "Main.hpp"

#include <memory>


template<typename T>
class RT_API SharedPtr
{
public:

    SharedPtr(T* _ptr = nullptr) : ptr(_ptr) { }

    SharedPtr(const SharedPtr<T>& cpy) : ptr(cpy.ptr) { }
    SharedPtr<T>& operator=(const SharedPtr<T>& cpy) { ptr = cpy.ptr; return *this; }


    T& operator*()
    {
        return ptr.operator*();
    }
    T* operator->()
    {
        return ptr.operator->();
    }

    const T& operator*() const
    {
        return ptr.operator*();
    }
    const T* operator->() const
    {
        return ptr.operator->();
    }


private:

    std::unique_ptr<T> ptr;
};