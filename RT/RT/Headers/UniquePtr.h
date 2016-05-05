#pragma once

#include "Main.hpp"

#include <memory>


template<typename T>
class RT_API UniquePtr
{
public:

    UniquePtr(T* _ptr = nullptr) : ptr(_ptr) { }

    UniquePtr(UniquePtr<T>&& moveFrom) { *this = std::move(moveFroM); }
    UniquePtr& operator=(UniquePtr&& moveFrom)
    {
        ptr = std::move(moveFrom);
        return *this;
    }

    UniquePtr& operator=(T* newPtr) { delete ptr.release(); ptr = newPtr; }


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


    T* Release() { return ptr.release(); }


private:

    std::unique_ptr<T> ptr;


    UniquePtr(const UniquePtr<T>& cpy) = delete;
    UniquePtr<T>& operator=(const UniquePtr<T>& cpy) = delete;
};