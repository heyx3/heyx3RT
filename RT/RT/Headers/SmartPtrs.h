#pragma once

#include "Main.hpp"

#include <utility>
#include <memory>


#pragma warning(disable: 4251)

namespace RT
{
    template<typename T>
    class RT_API UniquePtr
    {
    public:

        UniquePtr(T* _ptr = nullptr) { ptr.reset(_ptr); }

        UniquePtr(UniquePtr<T>&& moveFrom) { *this = std::move(moveFrom); }
        UniquePtr& operator=(UniquePtr&& moveFrom)
        {
            ptr.reset(moveFrom.Release());
            return *this;
        }

        UniquePtr(const UniquePtr<T>& cpy) = delete;
        UniquePtr<T>& operator=(const UniquePtr<T>& cpy) = delete;


        T& operator*() { return *ptr; }
        T* operator->() { return ptr.get(); }
        const T& operator*() const { return *ptr; }
        const T* operator->() const { return ptr.get(); }


        T* Get() { return ptr.get(); }
        const T* Get() const { return ptr.get(); }

        T* Release() { return ptr.release(); }
        void Reset(T* newPtr = nullptr) { ptr.reset(newPtr); }


    private:

        std::unique_ptr<T> ptr;
    };


    template<typename T>
    class RT_API SharedPtr
    {
    public:

        SharedPtr(T* _ptr = nullptr) { ptr.reset(_ptr); }

        T& operator*() { return *ptr; }
        T* operator->() { return ptr.get(); }
        const T& operator*() const { return *ptr; }
        const T* operator->() const { return ptr.get(); }

        T* Get() { return ptr.get(); }
        const T* Get() const { return ptr.get(); }

        void Reset(T* newPtr = nullptr) { ptr.reset(newPtr); }


    private:

        std::shared_ptr<T> ptr;
    };


    #define EXPORT_UNIQUEPTR(ptrType) template class RT_API UniquePtr<ptrType>;
    #define EXPORT_SHAREDPTR(ptrType) template class RT_API SharedPtr<ptrType>;
}

#pragma warning(default: 4251)