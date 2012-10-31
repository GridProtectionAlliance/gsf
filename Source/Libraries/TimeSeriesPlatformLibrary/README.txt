This file is intended to help developers build the Time Series Platform Library
on Linux platforms. Similar instructions may apply to other platforms.


1. Dependencies
---------------
The Time Series Platform Library depends on the following libraries in order
to build. Earlier versions of the libraries listed may not work properly.

CMake v2.8 (http://www.cmake.org/)

GNU Make (http://www.gnu.org/software/make/)
	- Not required on a Windows platform

Boost C++ Libraries v1.49.0 (http://www.boost.org/)
	- Boost.Asio
	- Boost.Bind
	- Boost.System
	- Boost.Thread
	- Boost.Uuid


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