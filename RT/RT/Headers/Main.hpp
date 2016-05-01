#pragma once


#if defined(_WIN32) || defined(WIN32)

#define OS_WINDOWS

#define WIN32_LEAN_AND_MEAN     // Exclude rarely-used stuff from Windows headers
#define NOMINMAX                // Stop conflicts with "min" and "max" macro names
// Windows Header Files:
#include <windows.h>

#elif defined(__unix__)

#define OS_UNIX

#endif


#ifdef OS_WINDOWS
	#ifdef RT_EXPORTS
		#define RT_API __declspec(dllexport)
        #define RT_EXIMP
	#elif RT_STATIC
		#define RT_API
        #define RT_EXIMP
	#else
		#define RT_API __declspec(dllimport)
        #define RT_EXIMP extern
	#endif
#else
	#define RT_API
    #define RT_EXIMP
#endif


//Force generation of a few important STL types.

#include <string>
RT_EXIMP template class std::allocator<char>;
RT_EXIMP template struct std::char_traits<char>;
RT_EXIMP template class RT_API std::basic_string<char, std::char_traits<char>, std::allocator<char>>;

#include <vector>
#define EXPORT_STL_VECTOR(type) RT_EXIMP template class RT_API std::vector<type>;
//EXPORT_STL_VECTOR(int);

#include <unordered_map>
#define EXPORT_STL_UNORDERED_MAP(keyType, valType) RT_EXIMP template class RT_API std::unordered_map<keyType, valType>;
//EXPORT_STL_UNORDERED_MAP(unsigned char, bool);