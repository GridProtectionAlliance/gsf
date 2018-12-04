//******************************************************************************************************
//  Convert.cpp - Gbtc
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

#include <ctime>
#include <iomanip>
#include <sstream>
#include <boost/uuid/string_generator.hpp>
#include <boost/uuid/uuid_io.hpp>

#include "Convert.h"
#include "Date.h"

using namespace std;
using namespace std::chrono;
using namespace date;
using namespace boost::uuids;
using namespace GSF::TimeSeries;

void GSF::TimeSeries::GetUnixTime(const int64_t ticks, time_t& unixSOC, uint16_t& milliseconds)
{
    // Unix dates are measured as the number of seconds since 1/1/1970
    const int64_t BaseTimeOffset = 621355968000000000L;

    unixSOC = static_cast<time_t>((ticks - BaseTimeOffset) / 10000000);

    if (unixSOC < 0)
        unixSOC = 0;

    milliseconds = static_cast<uint16_t>(ticks / 10000 % 1000);
}

uint32_t GSF::TimeSeries::TicksToString(char* ptr, uint32_t maxsize, string format, int64_t ticks)
{
    time_t fromSeconds;
    uint16_t milliseconds;

    GetUnixTime(ticks, fromSeconds, milliseconds);

    stringstream formatStream;
    uint32_t formatIndex = 0;

    while (formatIndex < format.size())
    {
        char c = format[formatIndex];
        ++formatIndex;

        if (c != '%')
        {
            // Not a format specifier
            formatStream << c;
            continue;
        }

        // Check for %f and %t format specifiers and handle them
        // accordingly. All other specifiers get forwarded to strftime
        c = format[formatIndex];
        ++formatIndex;

        switch (c)
        {
            case 'f':
            {
                stringstream temp;
                temp << setw(3) << setfill('0') << static_cast<int>(milliseconds);
                formatStream << temp.str();
                break;
            }

            case 't':
                formatStream << ticks;
                break;

            default:
                formatStream << '%' << c;
                break;
        }
    }

    struct tm timeinfo;

#ifdef _WIN32
    gmtime_s(&timeinfo, &fromSeconds);
#else
    gmtime_r(&fromSeconds, &timeinfo);
#endif

    return strftime(ptr, maxsize, formatStream.str().data(), &timeinfo);
}

template <class T>
string GSF::TimeSeries::ToString(const T& obj)
{
    stringstream stream;
    stream << obj;
    return stream.str();
}

std::string GSF::TimeSeries::ToString(Guid value)
{
    return boost::uuids::to_string(value);
}

std::string GSF::TimeSeries::ToString(time_t value, const char* fmt)
{
    stringstream stream;
    stream << format(fmt, system_clock::from_time_t(value));
    return stream.str();
}

Guid GSF::TimeSeries::ToGuid(const uint8_t* data, bool swapBytes)
{
    Guid id;
    uint8_t swappedBytes[16];
    uint8_t* encodedBytes;

    // Check if bytes need to be decoded in reverse order
    if (swapBytes)
    {
        uint8_t copy[8];

        for (uint32_t i = 0; i < 16; i++)
        {
            swappedBytes[i] = data[15 - i];

            if (i < 8)
                copy[i] = swappedBytes[i];
        }

        // Convert Microsoft encoding to RFC
        swappedBytes[3] = copy[0];
        swappedBytes[2] = copy[1];
        swappedBytes[1] = copy[2];
        swappedBytes[0] = copy[3];

        swappedBytes[4] = copy[5];
        swappedBytes[5] = copy[4];

        swappedBytes[6] = copy[7];
        swappedBytes[7] = copy[6];

        encodedBytes = swappedBytes;
    }
    else
    {
        encodedBytes = const_cast<uint8_t*>(data);
    }

    for (Guid::iterator iter = id.begin(); iter != id.end(); ++iter, ++encodedBytes)
        *iter = static_cast<Guid::value_type>(*encodedBytes);

    return id;
}

Guid GSF::TimeSeries::ToGuid(const char* data)
{
    const string_generator generator;
    return generator(data);
}

const char* GSF::TimeSeries::Coalesce(const char* data, const char* nonEmptyValue)
{
    if (data == nullptr)
        return nonEmptyValue;

    if (strlen(data) == 0)
        return nonEmptyValue;

    return data;
}

time_t GSF::TimeSeries::ParseXMLTimestamp(const char* time)
{
    istringstream in { time };
    sys_seconds timestamp;

    // Parse an XML formatted timestamp string, e.g.: 2018-03-14T19:23:11.665-04:00,
    // using the Hinnant date library: https://github.com/HowardHinnant/date
    in >> parse("%Y-%m-%dT%T%z", timestamp);

    return system_clock::to_time_t(timestamp);
}