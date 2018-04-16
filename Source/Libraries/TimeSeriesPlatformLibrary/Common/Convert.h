//******************************************************************************************************
//  Convert.h - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/06/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#ifndef __COMMON_CONVERT_H
#define __COMMON_CONVERT_H

#include <cstddef>
#include <string>

#include "CommonTypes.h"

using namespace std;

namespace GSF {
namespace TimeSeries
{
    // Converts a GEP timestamp, in Ticks, to UNIX second of century and milliseconds
    void GetUnixTime(const int64_t gepTime, time_t& unixSOC, int16_t& milliseconds);

    // Thin wrapper around strftime to provide formats for milliseconds (%f) and full-resolution ticks (%t)
    size_t TicksToString(char* ptr, size_t maxsize, string format, int64_t ticks);

    // Converts an object to a string
    template <class T>
    string ToString(const T& obj);

    // Converts 16 contiguous bytes of character data into a globally unique identifier
    Guid ToGuid(const uint8_t* data, bool swapBytes);
    Guid ToGuid(const char* data);

    // Returns a non-empty nor null value
    const char* Coalesce(const char* data, const char* nonEmptyValue);

    // Converts an XML formatted time string to a common epoch time
    time_t ParseXMLTimestamp(const char* time);
}}

#endif