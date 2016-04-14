#pragma once


#if defined(_WIN32) || defined(WIN32)

#define OS_WINDOWS

#define WIN32_LEAN_AND_MEAN     // Exclude rarely-used stuff from Windows headers
// Windows Header Files:
#include <windows.h>

#elif defined(__unix__)

#define OS_UNIX

#endif


#ifdef OS_WINDOWS
	#ifdef RT_EXPORTS
		#define RT_API __declspec(dllexport)
	#elif RT_STATIC
		#define RT_API
	#else
		#define RT_API __declspec(dllimport)
	#endif
#else
	#define RT_API
#endif