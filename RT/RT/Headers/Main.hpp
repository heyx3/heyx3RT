#pragma once


#if defined(_WIN32) || defined(WIN32)

#define OS_WINDOWS

#define WIN32_LEAN_AND_MEAN     // Exclude rarely-used stuff from Windows headers
#define NOMINMAX                // Stop conflicts with "min" and "max" macro names
// Windows Header Files:
#include <windows.h>
//#undef NOMINMAX
//#undef WIN32_LEAN_AND_MEAN

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