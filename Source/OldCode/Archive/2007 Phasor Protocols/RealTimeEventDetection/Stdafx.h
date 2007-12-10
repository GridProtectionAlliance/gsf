// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include <conio.h>
#include <string.h>
#include "..\CLAPACKlibs\f2c.h"
#include "..\CLAPACKlibs\clapack.h"
#include "..\CLAPACKlibs\blaswrap.h"
//#include "..\CLAPACKlibs\fftw3.h"
extern "C" struct fftw_plan_s { void *_nouse;};
#include "..\CLAPACKlibs\fftw3.h"