#******************************************************************************************************
#  CMakeLists.txt - Gbtc
#
#  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
#
#  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
#  the NOTICE file distributed with this work for additional information regarding copyright ownership.
#  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
#  not use this file except in compliance with the License. You may obtain a copy of the License at:
#
#      http://opensource.org/licenses/MIT
#
#  Unless agreed to in writing, the subject software distributed under the License is distributed on an
#  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
#  License for the specific language governing permissions and limitations.
#
#  Code Modification History:
#  ----------------------------------------------------------------------------------------------------
#  04/09/2012 - Stephen C. Wills
#       Generated original version of source code.
#
#******************************************************************************************************

cmake_minimum_required (VERSION 2.8)
project (TimeSeriesPlatformLibrary)

set (HEADER_OUTPUT_DIRECTORY Include/GSF)
set (CMAKE_RUNTIME_OUTPUT_DIRECTORY Output)
set (CMAKE_ARCHIVE_OUTPUT_DIRECTORY Libraries)
set (CMAKE_LIBRARY_OUTPUT_DIRECTORY Libraries)

set (headerFiles Common/Convert.h Common/EndianConverter.h
                 Common/ThreadSafeQueue.h Common/Types.h
                 Transport/CompactMeasurementParser.h Transport/Constants.h
                 Transport/DataSubscriber.h Transport/SignalIndexCache.h
                 Transport/SubscriberInstance.h Transport/Types.h
                 Transport/Version.h)

# Option to choose whether to build static or shared libraries
option (BUILD_SHARED_LIBS "Build gsf using shared libraries" OFF)

# Copy header files
foreach (headerFile ${headerFiles})
	string (REGEX MATCH "(.*)[/\\]" DIR ${headerFile})
	file (COPY ${headerFile} DESTINATION ${HEADER_OUTPUT_DIRECTORY}/${DIR})
endforeach (headerFile)

# Build gsf library
add_library (gsf Common/Convert.cpp Common/EndianConverter.cpp
                 Common/pugixml.cpp Transport/CompactMeasurementParser.cpp
                 Transport/DataSubscriber.cpp Transport/SignalIndexCache.cpp
                 Transport/SubscriberInstance.cpp Transport/Types.cpp)
target_link_libraries (gsf boost_system boost_thread boost_date_time
                           boost_iostreams pthread m)

# Install headers and library
install (DIRECTORY ${PROJECT_BINARY_DIR}/${HEADER_OUTPUT_DIRECTORY}
         DESTINATION include)
install (TARGETS gsf DESTINATION lib)


######################
# SAMPLE EXECUTABLES #
######################

set (CMAKE_RUNTIME_OUTPUT_DIRECTORY Samples)

# SimpleSubscribe sample
add_executable (SimpleSubscribe EXCLUDE_FROM_ALL Samples/SimpleSubscribe.cpp)
target_link_libraries (SimpleSubscribe gsf)

# AdvancedSubscribe sample
add_executable (AdvancedSubscribe EXCLUDE_FROM_ALL
                Samples/AdvancedSubscribe.cpp)
target_link_libraries (AdvancedSubscribe gsf)

# AverageFrequencyCalculator sample
add_executable (AverageFrequencyCalculator EXCLUDE_FROM_ALL
                Samples/AverageFrequencyCalculator.cpp)
target_link_libraries (AverageFrequencyCalculator gsf)

# InstanceSubscribe sample
add_executable (InstanceSubscribe EXCLUDE_FROM_ALL
                Samples/InstanceSubscribe.cpp Samples/SubscriberHandler.cpp)
target_link_libraries (InstanceSubscribe gsf)

# Build with 'make samples'
add_custom_target (samples DEPENDS SimpleSubscribe AdvancedSubscribe
                   AverageFrequencyCalculator InstanceSubscribe)
