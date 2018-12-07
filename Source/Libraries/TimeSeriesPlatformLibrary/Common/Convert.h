//******************************************************************************************************
//  Convert.h - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
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

#include "CommonTypes.h"

namespace GSF {
namespace TimeSeries
{
    // Converts a GEP timestamp, in Ticks, to UNIX second of century and milliseconds
    void GetUnixTime(int64_t gepTime, time_t& unixSOC, uint16_t& milliseconds);

    // Thin wrapper around strftime to provide formats for milliseconds (%f) and full-resolution ticks (%t)
    uint32_t TicksToString(char* ptr, uint32_t maxsize, std::string format, int64_t ticks);

    // Converts an object to a string
    template <class T>
    std::string ToString(const T& obj);

    std::string ToString(Guid value);

    std::string ToString(time_t value, const char* format = "%F %T");

    // Converts 16 contiguous bytes of character data into a globally unique identifier
    Guid ToGuid(const uint8_t* data, bool swapBytes);
    Guid ToGuid(const char* data);

    // Returns a non-empty nor null value
    const char* Coalesce(const char* data, const char* nonEmptyValue);

    // Converts an XML formatted time string to a common epoch time
    time_t ParseXMLTimestamp(const char* time);
}}

#endif