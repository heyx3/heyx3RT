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


//RT_API: Goes before any class/function/global var that should be visible to users of the DLL.
//RT_EXIMP: Defined as "extern" if importing a DLL, or nothing otherwise.
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

#pragma warning(disable: 4251)
#include <string>
RT_EXIMP template class std::allocator<char>;
RT_EXIMP template struct std::char_traits<char>;
RT_EXIMP template class RT_API std::basic_string<char, std::char_traits<char>, std::allocator<char>>;
#pragma warning(default: 4251)


#include <vector>

//Allows a vector of the given type to be used in the public interface of a DLL-exported object/function.
#define EXPORT_STL_VECTOR(type) RT_EXIMP template class RT_API std::vector<type>;