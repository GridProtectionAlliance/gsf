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

const datetime_t DateTime::MaxValue(max_date_time);

const datetime_t DateTime::MinValue(min_date_time);

const string Empty::String {};

const datetime_t Empty::DateTime {};

const Guid Empty::Guid = NilGuidGen();

const Object Empty::Object(nullptr);

const IPAddress Empty::IPAddress {};

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

    return std::distance(value.begin(), it.begin());
}

int32_t GSF::IndexOf(const string& value, const string& findValue, int32_t index, bool ignoreCase)
{
    iterator_range<string::const_iterator> it = ignoreCase ? ifind_nth(value, findValue, index) : find_nth(value, findValue, index);

    if (it.empty())
        return -1;

    return std::distance(value.begin(), it.begin());
}

int32_t GSF::LastIndexOf(const string& value, const string& findValue, bool ignoreCase)
{
    iterator_range<string::const_iterator> it = ignoreCase ? ifind_last(value, findValue) : find_last(value, findValue);

    if (it.empty())
        return -1;

    return std::distance(value.begin(), it.begin());
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

datetime_t GSF::DateAdd(const datetime_t& value, int32_t addValue, TimeInterval interval)
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

int32_t GSF::DateDiff(const datetime_t& startTime, const datetime_t& endTime, TimeInterval interval)
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

int32_t GSF::DatePart(const datetime_t& value, TimeInterval interval)
{
    static float64_t tickInterval = pow(10.0, time_duration::num_fractional_digits());

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
            return static_cast<int32_t>(value.time_of_day().fractional_seconds() / tickInterval * 1000.0);
        default:
            throw runtime_error("Unexpected time interval encountered");
    }
}

datetime_t GSF::Now()
{
    return datetime_t { microsec_clock::local_time() };
}

datetime_t GSF::UtcNow()
{
    return datetime_t { microsec_clock::universal_time() };
}
