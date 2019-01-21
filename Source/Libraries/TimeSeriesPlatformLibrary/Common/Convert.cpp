//******************************************************************************************************
//  Convert.cpp - Gbtc
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

#include <iomanip>
#include <sstream>
#include <boost/uuid/string_generator.hpp>
#include <boost/uuid/uuid_io.hpp>
#include <boost/date_time/posix_time/posix_time.hpp>

#include "Convert.h"

using namespace std;
using namespace std::chrono;
using namespace boost::uuids;
using namespace boost::posix_time;
using namespace GSF;

string PreparseTimestamp(const string& timestamp, time_duration& utcOffset)
{
    // 2018-03-14T19:23:11.665-04:00
    vector<string> dateTimeParts = Split(Replace(timestamp, "T", " "), " ", false);

    // Failed to understand timestamp format, just return input
    if (dateTimeParts.empty() || dateTimeParts.size() > 2)
        return timestamp;

    string updatedTimestamp {};
    string& datePart = dateTimeParts[0];
    string part {};
    vector<string> dateParts = Split(Replace(datePart, "/", "-", false), "-", false);

    if (dateParts.size() != 3)
        return timestamp;

    string year, month, day;

    for (int32_t i = 0; i < 3; i++)
    {
        part = dateParts[i];

        if (part.size() == 1)
            part.insert(0, "0");

        if (part.size() == 4)
            year = part;
        else if (month.empty())
            month = part;
        else
            day = part;
    }

    updatedTimestamp.append(year);
    updatedTimestamp.append("-");
    updatedTimestamp.append(month);
    updatedTimestamp.append("-");
    updatedTimestamp.append(day);

    if (dateTimeParts.size() == 1)
    {
        updatedTimestamp.append(" 00:00:00");
        return updatedTimestamp;
    }

    string& timePart = dateTimeParts[1];

    // Remove any time zone offset and hold on to it for later
    const bool containsMinus = Contains(timePart, "-", false);
    vector<string> timeParts = Split(timePart, containsMinus ? "-" : "+", false);
    string timeZoneOffset {};

    if (timeParts.size() == 2)
    {
        timePart = timeParts[0];

        // Swap timezone sign for conversion to UTC
        timeZoneOffset.append(containsMinus ? "+" : "-");
        timeZoneOffset.append(Replace(timeParts[1], ":", "", false));
    }

    timeParts = Split(timePart, ":", false);

    if (timeParts.size() == 2)
        timeParts.push_back("00");

    if (timeParts.size() != 3)
        return timestamp;

    updatedTimestamp.append(" ");

    for (int32_t i = 0; i < 3; i++)
    {
        string fractionalSeconds {};
        part = timeParts[i];

        if (i == 2 && Contains(part, ".", false))
        {
            vector<string> secondParts = Split(part, ".", false);

            if (secondParts.size() == 2)
            {
                part = secondParts[0];
                fractionalSeconds.append(".");
                fractionalSeconds.append(secondParts[1]);
            }
        }

        if (i > 0)
            updatedTimestamp.append(":");

        if (part.size() == 1)
            updatedTimestamp.append("0");

        updatedTimestamp.append(part);

        if (!fractionalSeconds.empty())
            updatedTimestamp.append(fractionalSeconds);
    }

    if (timeZoneOffset.size() == 5)
        utcOffset = time_duration(stoi(timeZoneOffset.substr(0, 3)), stoi(timeZoneOffset.substr(3)), 0);

    return updatedTimestamp;
}

void GSF::ToUnixTime(const int64_t ticks, time_t& unixSOC, uint16_t& milliseconds)
{
    // Unix dates are measured as the number of seconds since 1/1/1970
    const int64_t BaseTimeOffset = 621355968000000000L;

    unixSOC = (ticks - BaseTimeOffset) / 10000000;

    if (unixSOC < 0)
        unixSOC = 0;

    milliseconds = static_cast<uint16_t>(ticks / 10000 % 1000);
}

DateTime GSF::FromUnixTime(time_t unixSOC, uint16_t milliseconds)
{
    return from_time_t(unixSOC) + boost::posix_time::milliseconds(milliseconds);;
}

DateTime GSF::FromTicks(const int64_t ticks)
{
    time_t unixSOC;
    uint16_t milliseconds;

    ToUnixTime(ticks, unixSOC, milliseconds);
    return FromUnixTime(unixSOC, milliseconds);
}

uint32_t GSF::TicksToString(char* ptr, uint32_t maxsize, string format, int64_t ticks)
{
    time_t fromSeconds;
    uint16_t milliseconds;

    ToUnixTime(ticks, fromSeconds, milliseconds);

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

    struct tm timeinfo{};

#ifdef _WIN32
    gmtime_s(&timeinfo, &fromSeconds);
#else
    gmtime_r(&fromSeconds, &timeinfo);
#endif

    return strftime(ptr, maxsize, formatStream.str().data(), &timeinfo);
}

std::string GSF::ToString(Guid value)
{
    return boost::uuids::to_string(value);
}

std::string GSF::ToString(DateTime value, const char* format)
{
    using namespace boost::gregorian;

    stringstream stream;

    date_facet* facet = new date_facet();
    facet->format(format);
    stream.imbue(locale(locale::classic(), facet));
    stream << value;

    return stream.str();
}

Guid GSF::ParseGuid(const uint8_t* data, bool swapBytes)
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

Guid GSF::ParseGuid(const char* data)
{
    const string_generator generator;
    return generator(data);
}

const char* GSF::Coalesce(const char* data, const char* nonEmptyValue)
{
    if (data == nullptr)
        return nonEmptyValue;

    if (strlen(data) == 0)
        return nonEmptyValue;

    return data;
}

// Parse a timestamp string, e.g.: 2018-03-14T19:23:11.665-04:00
bool GSF::TryParseTimestamp(const char* time, DateTime& timestamp, bool parseAsUTC)
{
    static const locale formats[] = {
        locale(locale::classic(), new time_input_facet("%Y-%m-%d %H:%M:%S%F")),
        locale(locale::classic(), new time_input_facet("%Y%m%dT%H%M%S%F"))
    };

    static const int32_t formatsCount = sizeof(formats) / sizeof(formats[0]);

    for (int32_t i = 0; i < formatsCount; i++)
    {
        time_duration utcOffset {};
        istringstream stream(PreparseTimestamp(time, utcOffset));

        stream.imbue(formats[i]);
        stream >> timestamp;

        if (bool(stream))
        {
            if (parseAsUTC)
                timestamp += utcOffset;

            return true;
        }
    }

    return false;
}

DateTime GSF::ParseTimestamp(const char* time, bool parseAsUTC)
{
    DateTime timestamp;

    if (TryParseTimestamp(time, timestamp, parseAsUTC))
        return timestamp;

    throw runtime_error("Failed to parse timestamp \"" + string(time) + "\"");
}