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

using namespace std;
using namespace GSF::TimeSeries;
using namespace boost::algorithm;

const decimal_t Decimal::MaxValue = numeric_limits<decimal_t>::max();

const decimal_t Decimal::MinValue = numeric_limits<decimal_t>::min();

const decimal_t Decimal::DotNetMaxValue = decimal_t("79228162514264337593543950335");

const decimal_t Decimal::DotNetMinValue = decimal_t("-79228162514264337593543950335");

const string Empty::String;

const Guid Empty::Guid = ParseGuid("00000000-0000-0000-0000-000000000000");

const Object Empty::Object(nullptr);

const IPAddress Empty::IPAddress;

const uint8_t* Empty::ZeroLengthBytes = new uint8_t[4] { 0, 0, 0, 0 };

bool GSF::TimeSeries::IsEqual(const std::string& left, const std::string& right, bool ignoreCase)
{
    if (ignoreCase)
        return boost::iequals(left, right);

    return boost::equals(left, right);
}

bool GSF::TimeSeries::StartsWith(const std::string& value, const std::string& findValue, bool ignoreCase)
{
    if (ignoreCase)
        return istarts_with(value, findValue);

    return starts_with(value, findValue);
}

bool GSF::TimeSeries::EndsWith(const std::string& value, const std::string& findValue, bool ignoreCase)
{
    if (ignoreCase)
        return iends_with(value, findValue);

    return ends_with(value, findValue);
}

bool GSF::TimeSeries::Contains(const std::string& value, const std::string& findValue, bool ignoreCase)
{
    if (ignoreCase)
        return icontains(value, findValue);

    return contains(value, findValue);
}

int32_t GSF::TimeSeries::Compare(const std::string& leftValue, const std::string& rightValue, bool ignoreCase)
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

std::string GSF::TimeSeries::Replace(const std::string& value, const std::string& findValue, const std::string& replaceValue, bool ignoreCase)
{
    if (ignoreCase)
        return ireplace_all_copy(value, findValue, replaceValue);

    return replace_all_copy(value, findValue, replaceValue);
}

std::string GSF::TimeSeries::ToUpper(const std::string& value)
{
    return to_upper_copy(value);
}

std::string GSF::TimeSeries::ToLower(const std::string& value)
{
    return to_lower_copy(value);
}

std::string GSF::TimeSeries::Trim(const std::string& value)
{
    return trim_copy(value);
}

std::string GSF::TimeSeries::TrimRight(const std::string& value)
{
    return trim_right_copy(value);
}

std::string GSF::TimeSeries::TrimLeft(const std::string& value)
{
    return trim_left_copy(value);
}