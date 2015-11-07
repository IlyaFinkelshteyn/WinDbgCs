// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the CSSCRIPTS_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// CSSCRIPTS_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef CSSCRIPTS_EXPORTS
#define CSSCRIPTS_API __declspec(dllexport)
#else
#define CSSCRIPTS_API __declspec(dllimport)
#endif

extern "C" {

CSSCRIPTS_API HRESULT DebugExtensionInitialize(
	_Out_ PULONG Version,
	_Out_ PULONG Flags);

};
