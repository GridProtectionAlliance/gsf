//========================================================================
//
// Error.cc
//
// Copyright 1996-2003 Glyph & Cog, LLC
//
//========================================================================

#include <aconf.h>

#ifdef USE_GCC_PRAGMAS
#pragma implementation
#endif

#include <stdio.h>
#include <stddef.h>
#include <stdarg.h>
#include "GlobalParams.h"
#include "Error.h"
#include "gmem.h"

// .NET error capture hook...
extern void CaptureError(char* errMsg);

void CDECL error(int pos, char *msg, ...)
{
	va_list args;
	int len;
	char* buffer;

	// NB: this can be called before the globalParams object is created
	if (globalParams && globalParams->getErrQuiet()) return;

	if (pos >= 0)
	{
		//fprintf(stderr, "Error (%d): ", pos);
		char error[25];
		sprintf(error, "Error (%d): ", pos);
		CaptureError(error);
	}
	else
	{
		//fprintf(stderr, "Error: ");
		CaptureError("Error: ");
	}

	//va_start(args, msg);
	//vfprintf(stderr, msg, args);
	//va_end(args);
	//fprintf(stderr, "\n");
	//fflush(stderr);

	va_start(args, msg);
	len = _vscprintf(msg, args) + 1;
	buffer = (char*)gmalloc(len * sizeof(char));
	if (buffer)
	{
		vsprintf(buffer, msg, args);
		CaptureError(buffer);
		CaptureError("\n");
		gfree(buffer);
	}
	va_end(args);
}
