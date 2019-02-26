This is the C++ implementation of the Gateway Exchange Protocol.

This code includes functionality for both "subscriber" and "publisher" in GEP.

------------------------------------------------------------------------------
Compiling in Visual Studio
------------------------------------------------------------------------------

To properly compile in Visual Studio, you will need to download Boost:
	http://www.boost.org/users/download/

By default, the GSF Time Series Platform Library project configuration adds an
additional include directory for the Boost libraries in a parallel location to
the Grid Solutions Framework project in a folder called "boost" regardless of
version, for example:

GSF Library files:
	C:\Projects\gsf
				   \Build
				   \Source
				   etc...

Boost Library files:
	C:\Projects\boost
				     \boost
				     \libs
				     etc...

If you have an existing Boost installation you can simply create a symbolic
link to the folder, e.g., mklink /D C:\Projects\boost C:\boost_1_66_0

Alternately you can adjust the additional include directories to your own
Boost installation location for each of the build configurations. The code
has been tested with version v1.66 of Boost.

Note that you will need to compile Boost in order to execute the sample
applications found in:

	...\gsf\Source\Applications\TimeSeries Platform Library\Samples

The GSF Time Series Platform Library uses zlib features of Boost, as a result
compiling boost requires zlib source code that can be downloaded separately:
	https://zlib.net/

After unzipping the zlib source code, set the following Boost compile script
environmental variables to the root of the zlib source code path, e.g.:

	set ZLIB_SOURCE="C:\zlib-1.2.11"
	set ZLIB_INCLUDE="C:\zlib-1.2.11"

Once environmental variables are set for zlib paths, compile Boost as normal.

------------------------------------------------------------------------------
Compiling in Linux
------------------------------------------------------------------------------

The following information is intended to help developers build the GSF
Time Series Platform Library on Linux platforms. Similar instructions may
apply to other platforms.

1. Dependencies
---------------
The Time Series Platform Library depends on the following libraries in order
to build. Earlier versions of the libraries listed may not work properly.

CMake v2.8 (http://www.cmake.org/)

GNU Make (http://www.gnu.org/software/make/)

zlib Library, e.g.: sudo apt install zlib1g-dev

bzip2 Library, e.g.: sudo apt install libbz2-dev

Boost C++ Libraries v1.66.0 (http://www.boost.org/)
	- Boost.Asio
	- Boost.Bind
	- Boost.Iostreams
	- Boost.System
	- Boost.Thread
	- Boost.Uuid

Boost will need to be compiled:
https://www.boost.org/doc/libs/1_66_0/more/getting_started/unix-variants.html

2. Configuration
----------------
From the command terminal, enter the source directory containing this
README.txt file and type the following command.

	cmake .

Alternatively, you can create a build directory separate from the
source code you downloaded. Enter the build directory you created
and type the following command.

	cmake path/to/source

Using the CMake GUI, you can modify configuration options, such as
building as a shared library or changing the installation directory.

To make a debug build, use the following:

    cmake -DCMAKE_BUILD_TYPE=Debug

3. Build
--------
At the top level of the build directory, type the following command.

	make

In addition to the library itself, there are sample applications which
demonstrate the proper use of the Time Series Platform Library API.
To build these samples, type the following command.

	make samples


4. Installation
---------------
At the top level of the build directory, type the following command.

	make install

This will move the header files and the library file to the location
specified during configuration. Header files go under the 'include/'
subdirectory, and the library file goes under the 'lib/' subdirectory.
