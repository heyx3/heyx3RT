#pragma once

#include "Main.hpp"

#include <unordered_map>


namespace RT
{
    template<typename TKey, typename TValue>
    class Dictionary
    {
    public:

        size_t GetSize() const { return dict.size(); }
        void Reserve(size_t n) { dict.reserve(n); }

        bool Contains(const TKey& key) const { return dict.find(key) != dict.end(); }
        void Clear() { dict.clear(); }

        TValue& operator[](const TKey& key) { return dict[key]; }
        const TValue& operator[](const TKey& key) const { return dict.find(key)->second; }

        const TValue* TryGet(const TKey& key) const
        {
            auto found = dict.find(key);
            if (found == dict.end())
                return nullptr;
            else
                return &found->second;
        }
        TValue* TryGet(const TKey& key)
        {
            return (TValue*)((const Dictionary<TKey, TValue>*)this)->TryGet(key);
        }
        
        const TValue& Get(const TKey& key, const TValue& valIfNotFound) const
        {
            auto tried = TryGet(key);
            return (tried == nullptr ?
                        valIfNotFound :
                        *tried);
        }
        TValue& Get(const TKey& key, const TValue& valIfNotFound)
        {
            return (TValue&)((const Dictionary<TKey, TValue>*)this)->Get(key, valIfNotFound);
        }

        //A function that just takes a const reference to a key.
        template<typename Func>
        //Runs the given function for every key in this dictionary.
        void DoToEach(Func func) const
        {
            for (auto keyAndVal : dict)
                func(keyAndVal.first);
        }

        //A function that just takes a const reference to a key.
        template<typename Func>
        //Runs the given function for every key in this dictionary.
        void DoToEach(Func func)
        {
            for (auto keyAndVal : dict)
                func(keyAndVal.first);
        }


    private:

        std::unordered_map<TKey, TValue> dict;
    };

#define EXPORT_RT_DICT(keyType, valType) template class RT_API Dictionary<keyType, valType>;
}