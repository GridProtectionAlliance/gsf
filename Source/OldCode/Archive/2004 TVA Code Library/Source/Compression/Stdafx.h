// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once
#using <mscorlib.dll>
#include <Windows.h>

// Some things defined in Windows.h can conflict with needed .NET namespace items,
// so we undefine these when they cause problems
#undef CreateDirectory
#undef CreateDirectoryA
#undef CreateDirectoryW
