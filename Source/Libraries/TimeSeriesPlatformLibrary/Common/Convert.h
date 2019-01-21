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

namespace GSF
{
    // Converts a timestamp, in Ticks, to Unix second of century and milliseconds
    void ToUnixTime(int64_t ticks, time_t& unixSOC, uint16_t& milliseconds);

    // Convert Unix second of century and milliseconds to DateTime
    DateTime FromUnixTime(time_t unixSOC, uint16_t milliseconds);

    // Converts a timestamp, in Ticks, to DateTime
    DateTime FromTicks(int64_t ticks);
    
    // Thin wrapper around strftime to provide formats for milliseconds (%f) and full-resolution ticks (%t)
    uint32_t TicksToString(char* ptr, uint32_t maxsize, std::string format, int64_t ticks);

    // Converts an object to a string
    template <class T>
    std::string ToString(const T& obj)
    {
        std::stringstream stream;
        stream << obj;
        return stream.str();
    }

    std::string ToString(Guid value);

    std::string ToString(DateTime value, const char* format = "%Y-%m-%d %H:%M:%S%F");

    // Converts 16 contiguous bytes of character data into a globally unique identifier
    Guid ParseGuid(const uint8_t* data, bool swapBytes);
    Guid ParseGuid(const char* data);

    // Returns a non-empty nor null value
    const char* Coalesce(const char* data, const char* nonEmptyValue);

    // Attempts to parse an time string in several common formats
    bool TryParseTimestamp(const char* time, DateTime& timestamp, bool parseAsUTC = true);

    // Converts a string to a date-time that may be in several common formats
    DateTime ParseTimestamp(const char* time, bool parseAsUTC = true);
}

#endif