//******************************************************************************************************
//  CommonTypes.cpp - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/23/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

#include "CommonTypes.h"
#include "Convert.h"
#include <boost/algorithm/string.hpp>
#include <boost/algorithm/string/case_conv.hpp>
#include <boost/algorithm/string/trim.hpp>
#include <boost/uuid/uuid_generators.hpp>

using namespace std;
using namespace boost;
using namespace boost::algorithm;
using namespace boost::posix_time;
using namespace boost::gregorian;
using namespace GSF;

boost::uuids::random_generator RandomGuidGen;
boost::uuids::nil_generator NilGuidGen;

const decimal_t Decimal::MaxValue = numeric_limits<decimal_t>::max();

const decimal_t Decimal::MinValue = numeric_limits<decimal_t>::min();

const decimal_t Decimal::DotNetMaxValue = decimal_t("79228162514264337593543950335");

const decimal_t Decimal::DotNetMinValue = decimal_t("-79228162514264337593543950335");

const string Empty::String {};

const DateTime Empty::DateTime {};

const Guid Empty::Guid = NilGuidGen();

const Object Empty::Object(nullptr);

const IPAddress Empty::IPAddress {};

const uint8_t* Empty::ZeroLengthBytes = new uint8_t[4] { 0, 0, 0, 0 };

size_t StringHash::operator()(const string& value) const
{
    size_t seed = 0;
    const locale locale;

    for (string::const_iterator it = value.begin(); it != value.end(); ++it)
        hash_combine(seed, toupper(*it, locale));

    return seed;
}

bool StringEqual::operator()(const string& left, const string& right) const
{
    return IsEqual(left, right);
}

bool StringComparer::operator()(const std::string& left, const std::string& right) const
{
    return Compare(left, right) < 0;
}

Guid GSF::NewGuid()
{
    return RandomGuidGen();
}

bool GSF::IsEqual(const string& left, const string& right, bool ignoreCase)
{
    if (ignoreCase)
        return iequals(left, right);

    return equals(left, right);
}

bool GSF::StartsWith(const string& value, const string& findValue, bool ignoreCase)
{
    if (ignoreCase)
        return istarts_with(value, findValue);

    return starts_with(value, findValue);
}

bool GSF::EndsWith(const string& value, const string& findValue, bool ignoreCase)
{
    if (ignoreCase)
        return iends_with(value, findValue);

    return ends_with(value, findValue);
}

bool GSF::Contains(const string& value, const string& findValue, bool ignoreCase)
{
    if (ignoreCase)
        return icontains(value, findValue);

    return contains(value, findValue);
}

int32_t GSF::Count(const string& value, const string& findValue, bool ignoreCase)
{
    find_iterator<string::const_iterator> it = ignoreCase ?
        make_find_iterator(value, first_finder(findValue, is_iequal())) :
        make_find_iterator(value, first_finder(findValue, is_equal()));

    const find_iterator<string::const_iterator> end {};
    int32_t count = 0;

    for (; it != end; ++it, ++count)
    {
    }

    return count;
}

int32_t GSF::Compare(const string& leftValue, const string& rightValue, bool ignoreCase)
{
    if (ignoreCase)
    {
        if (ilexicographical_compare(leftValue, rightValue))
            return -1;

        if (ilexicographical_compare(rightValue, leftValue))
            return 1;

        return 0;
    }

    if (lexicographical_compare(leftValue, rightValue))
        return -1;

    if (lexicographical_compare(rightValue, leftValue))
        return 1;

    return 0;
}

int32_t GSF::IndexOf(const string& value, const string& findValue, bool ignoreCase)
{
    iterator_range<string::const_iterator> it = ignoreCase ? ifind_first(value, findValue) : find_first(value, findValue);

    if (it.empty())
        return -1;

    return distance(value.begin(), it.begin());
}

int32_t GSF::IndexOf(const string& value, const string& findValue, int32_t index, bool ignoreCase)
{
    iterator_range<string::const_iterator> it = ignoreCase ? ifind_nth(value, findValue, index) : find_nth(value, findValue, index);

    if (it.empty())
        return -1;

    return distance(value.begin(), it.begin());
}

int32_t GSF::LastIndexOf(const string& value, const string& findValue, bool ignoreCase)
{
    iterator_range<string::const_iterator> it = ignoreCase ? ifind_last(value, findValue) : find_last(value, findValue);

    if (it.empty())
        return -1;

    return distance(value.begin(), it.begin());
}

vector<string> GSF::Split(const string& value, const string& delimiterValue, bool ignoreCase)
{
    split_iterator<string::const_iterator> it = ignoreCase ?
        make_split_iterator(value, first_finder(delimiterValue, is_iequal())) :
        make_split_iterator(value, first_finder(delimiterValue, is_equal()));

    const split_iterator<string::const_iterator> end {};
    vector<string> values;

    for (; it != end; ++it)
    {
        values.push_back(copy_range<string>(*it));
    }

    return values;
}

string GSF::Split(const string& value, const string& delimiterValue, int32_t index, bool ignoreCase)
{
    split_iterator<string::const_iterator> it = ignoreCase ?
        make_split_iterator(value, first_finder(delimiterValue, is_iequal())) :
        make_split_iterator(value, first_finder(delimiterValue, is_equal()));

    const split_iterator<string::const_iterator> end {};
    int32_t count = 0;

    for (; it != end; ++it, ++count)
    {
        if (count == index)
            return copy_range<string>(*it);
    }

    return string {};
}

string GSF::Replace(const string& value, const string& findValue, const string& replaceValue, bool ignoreCase)
{
    if (ignoreCase)
        return ireplace_all_copy(value, findValue, replaceValue);

    return replace_all_copy(value, findValue, replaceValue);
}

string GSF::ToUpper(const string& value)
{
    return to_upper_copy(value);
}

string GSF::ToLower(const string& value)
{
    return to_lower_copy(value);
}

string GSF::Trim(const string& value)
{
    return trim_copy(value);
}

string GSF::Trim(const string& value, const string& trimValues)
{
    return trim_copy_if(value, is_any_of(trimValues));
}

string GSF::TrimRight(const string& value)
{
    return trim_right_copy(value);
}

string GSF::TrimRight(const string& value, const string& trimValues)
{
    return trim_right_copy_if(value, is_any_of(trimValues));
}

string GSF::TrimLeft(const string& value)
{
    return trim_left_copy(value);
}

string GSF::TrimLeft(const string& value, const string& trimValues)
{
    return trim_left_copy_if(value, is_any_of(trimValues));
}

string GSF::PadLeft(const string& value, uint32_t count, char padChar)
{
    if (value.size() < count)
        return string(count - value.size(), padChar) + value;

    return value;
}

string GSF::PadRight(const string& value, uint32_t count, char padChar)
{
    if (value.size() < count)
        return value + string(count - value.size(), padChar);

    return value;
}

StringMap<string> GSF::ParseKeyValuePairs(const string& value, const char parameterDelimiter, const char keyValueDelimiter, const char startValueDelimiter, const char endValueDelimiter)
{
    if (parameterDelimiter == keyValueDelimiter ||
        parameterDelimiter == startValueDelimiter ||
        parameterDelimiter == endValueDelimiter ||
        keyValueDelimiter == startValueDelimiter ||
        keyValueDelimiter == endValueDelimiter ||
        startValueDelimiter == endValueDelimiter)
            throw invalid_argument("All delimiters must be unique");

    const string& escapedParameterDelimiter = RegExEncode(parameterDelimiter);
    const string& escapedKeyValueDelimiter = RegExEncode(keyValueDelimiter);
    const string& escapedStartValueDelimiter = RegExEncode(startValueDelimiter);
    const string& escapedEndValueDelimiter = RegExEncode(endValueDelimiter);
    const string& escapedBackslashDelimiter = RegExEncode('\\');
    const string& parameterDelimiterStr = string(1, parameterDelimiter);
    const string& keyValueDelimiterStr = string(1, keyValueDelimiter);
    const string& startValueDelimiterStr = string(1, startValueDelimiter);
    const string& endValueDelimiterStr = string(1, endValueDelimiter);
    const string& backslashDelimiterStr = "\\";

    StringMap<string> keyValuePairs;
    vector<string> escapedValue;
    bool valueEscaped = false;
    uint32_t delimiterDepth = 0;

    // Escape any parameter or key/value delimiters within tagged value sequences
    //      For example, the following string:
    //          "normalKVP=-1; nestedKVP={p1=true; p2=false}")
    //      would be encoded as:
    //          "normalKVP=-1; nestedKVP=p1\\u003dtrue\\u003b p2\\u003dfalse")
    for (uint32_t i = 0; i < value.size(); i++)
    {
        const char character = value[i];

        if (character == startValueDelimiter)
        {
            if (!valueEscaped)
            {
                valueEscaped = true;
                continue;   // Don't add tag start delimiter to final value
            }

            // Handle nested delimiters
            delimiterDepth++;
        }

        if (character == endValueDelimiter)
        {
            if (valueEscaped)
            {
                if (delimiterDepth > 0)
                {
                    // Handle nested delimiters
                    delimiterDepth--;
                }
                else
                {
                    valueEscaped = false;
                    continue;   // Don't add tag stop delimiter to final value
                }
            }
            else
            {
                throw runtime_error("Failed to parse key/value pairs: invalid delimiter mismatch. Encountered end value delimiter '" + endValueDelimiterStr + "' before start value delimiter '" + startValueDelimiterStr + "'.");  // NOLINT
            }
        }

        if (valueEscaped)
        {
            // Escape any delimiter characters inside nested key/value pair
            if (character == parameterDelimiter)
                escapedValue.push_back(escapedParameterDelimiter);
            else if (character == keyValueDelimiter)
                escapedValue.push_back(escapedKeyValueDelimiter);
            else if (character == startValueDelimiter)
                escapedValue.push_back(escapedStartValueDelimiter);
            else if (character == endValueDelimiter)
                escapedValue.push_back(escapedEndValueDelimiter);
            else if (character == '\\')
                escapedValue.push_back(escapedBackslashDelimiter);
            else
                escapedValue.emplace_back(1, character);
        }
        else
        {
            if (character == '\\')
                escapedValue.push_back(escapedBackslashDelimiter);
            else
                escapedValue.emplace_back(1, character);
        }
    }

    if (delimiterDepth != 0 || valueEscaped)
    {
        // If value is still escaped, tagged expression was not terminated
        if (valueEscaped)
            delimiterDepth = 1;

        const bool moreStartDelimiters = delimiterDepth > 0;

        throw runtime_error(
            "Failed to parse key/value pairs: invalid delimiter mismatch. Encountered more " +
            (moreStartDelimiters ? "start value delimiters '" + startValueDelimiterStr + "'" : "end value delimiters '" + endValueDelimiterStr + "'") + " than " +
            (moreStartDelimiters ? "end value delimiters '" + endValueDelimiterStr + "'" : "start value delimiters '" + startValueDelimiterStr + "'") + ".");
    }

    // Parse key/value pairs from escaped value
    vector<string> pairs = Split(join(escapedValue, ""), parameterDelimiterStr, false);

    for (uint32_t i = 0; i < pairs.size(); i++)
    {
        // Separate key from value
        vector<string> elements = Split(pairs[i], keyValueDelimiterStr, false);

        if (elements.size() == 2)
        {
            // Get key
            const string key = Trim(elements[0]);

            // Get unescaped value
            const string unescapedValue = Replace(Replace(Replace(Replace(Replace(Trim(elements[1]),
                escapedParameterDelimiter, parameterDelimiterStr, false),
                escapedKeyValueDelimiter, keyValueDelimiterStr, false),
                escapedStartValueDelimiter, startValueDelimiterStr, false),
                escapedEndValueDelimiter, endValueDelimiterStr, false),                    
                escapedBackslashDelimiter, backslashDelimiterStr, false);

            // Add or replace key elements with unescaped value
            keyValuePairs[key] = unescapedValue;
        }
    }

    return keyValuePairs;
}

DateTime GSF::DateAdd(const DateTime& value, int32_t addValue, TimeInterval interval)
{
    switch (interval)
    {
        case TimeInterval::Year:
            return value + years(addValue);
        case TimeInterval::Month:
            return value + months(addValue);
        case TimeInterval::DayOfYear:
        case TimeInterval::Day:
        case TimeInterval::WeekDay:
            return value + days(addValue);
        case TimeInterval::Week:
            return value + weeks(addValue);
        case TimeInterval::Hour:
            return value + hours(addValue);
        case TimeInterval::Minute:
            return value + minutes(addValue);
        case TimeInterval::Second:
            return value + seconds(addValue);
        case TimeInterval::Millisecond:
            return value + milliseconds(addValue);
        default:
            throw runtime_error("Unexpected time interval encountered");
    }
}

int32_t GSF::DateDiff(const DateTime& startTime, const DateTime& endTime, TimeInterval interval)
{
    if (interval < TimeInterval::Hour)
    {
        switch (interval)
        {
            case TimeInterval::Year:
                return endTime.date().year() - startTime.date().year();
            case TimeInterval::Month:
                return DateDiff(startTime, endTime, TimeInterval::Year) * 12 + (endTime.date().month() - startTime.date().month());
            case TimeInterval::DayOfYear:
            case TimeInterval::Day:
            case TimeInterval::WeekDay:
                return (endTime.date() - startTime.date()).days();
            case TimeInterval::Week:
                return (endTime.date() - startTime.date()).days() / 7;
            default:
                throw runtime_error("Unexpected time interval encountered");
        }
    }

    time_duration duration = endTime - startTime;

    switch (interval)
    {
        case TimeInterval::Hour:
            return static_cast<int32_t>(duration.hours());
        case TimeInterval::Minute:
            return static_cast<int32_t>(duration.total_seconds() / 60);
        case TimeInterval::Second:
            return static_cast<int32_t>(duration.total_seconds());
        case TimeInterval::Millisecond:
            return static_cast<int32_t>(duration.total_milliseconds());
        default:
            throw runtime_error("Unexpected time interval encountered");
    }
}

int32_t GSF::DatePart(const DateTime& value, TimeInterval interval)
{
    static float64_t baseFraction = pow(10.0, time_duration::num_fractional_digits());

    switch (interval)
    {
        case TimeInterval::Year:
            return value.date().year();
        case TimeInterval::Month:
            return value.date().month();
        case TimeInterval::DayOfYear:
            return value.date().day_of_year();
        case TimeInterval::Day:
            return value.date().day();
        case TimeInterval::Week:
            return value.date().week_number();
        case TimeInterval::WeekDay:
            return value.date().day_of_week() + 1;
        case TimeInterval::Hour:
            return static_cast<int32_t>(value.time_of_day().hours());
        case TimeInterval::Minute:
            return static_cast<int32_t>(value.time_of_day().minutes());
        case TimeInterval::Second:
            return static_cast<int32_t>(value.time_of_day().seconds());
        case TimeInterval::Millisecond:
            return static_cast<int32_t>(value.time_of_day().fractional_seconds() / baseFraction * 1000.0);
        default:
            throw runtime_error("Unexpected time interval encountered");
    }
}

DateTime GSF::Now()
{
    return DateTime { microsec_clock::local_time() };
}

DateTime GSF::UtcNow()
{
    return DateTime { microsec_clock::universal_time() };
}
