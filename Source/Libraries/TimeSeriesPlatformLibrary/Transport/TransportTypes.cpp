//******************************************************************************************************
//  TransportTypes.cpp - Gbtc
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

#include "TransportTypes.h"
#include "Constants.h"
#include "../Common/Convert.h"

using namespace std;
using namespace GSF;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

SubscriberException::SubscriberException(string message) noexcept :
    m_message(move(message))
{
}

const char* SubscriberException::what() const noexcept
{
    return &m_message[0];
}

PublisherException::PublisherException(string message) noexcept :
    m_message(move(message))
{
}

const char* PublisherException::what() const noexcept
{
    return &m_message[0];
}

Measurement::Measurement() :
    ID(0),
    SignalID(Empty::Guid),
    Value(NAN),
    Adder(0),
    Multiplier(1),
    Timestamp(0),
    Flags(MeasurementStateFlags::Normal)
{
}

float64_t Measurement::AdjustedValue() const
{
    return Value * Multiplier + Adder;
}

datetime_t Measurement::GetDateTime() const
{
    return FromTicks(Timestamp);
}

void Measurement::GetUnixTime(time_t& unixSOC, uint16_t& milliseconds) const
{
    ToUnixTime(Timestamp, unixSOC, milliseconds);
}

MeasurementPtr TimeSeries::ToPtr(const Measurement& source)
{
    MeasurementPtr destination = NewSharedPtr<Measurement>();

    destination->ID = source.ID;
    destination->Source = source.Source;
    destination->SignalID = source.SignalID;
    destination->Tag = source.Tag;
    destination->Value = source.Value;
    destination->Adder = source.Adder;
    destination->Multiplier = source.Multiplier;
    destination->Timestamp = source.Timestamp;
    destination->Flags = source.Flags;

    return destination;
}

SignalReference::SignalReference() :
    SignalID(Empty::Guid),
    Index(0),
    Kind(SignalKind::Unknown)
{    
}

SignalReference::SignalReference(const string& signal) : SignalID(Guid())
{
    // Signal reference may contain multiple dashes, we're interested in the last one
    const auto splitIndex = signal.find_last_of('-');

    // Assign default values to fields
    Index = 0;

    if (splitIndex == string::npos)
    {
        // This represents an error - best we can do is assume entire string is the acronym
        Acronym = ToUpper(Trim(signal));
        Kind = SignalKind::Unknown;
    }
    else
    {
        string signalType = ToUpper(Trim(signal.substr(splitIndex + 1)));
        Acronym = ToUpper(Trim(signal.substr(0, splitIndex)));

        // If the length of the signal type acronym is greater than 2, then this
        // is an indexed signal type (e.g., CORDOVA-PA2)
        if (signalType.length() > 2)
        {
            Kind = ParseSignalKind(signalType.substr(0, 2));

            if (Kind != SignalKind::Unknown)
                Index = stoi(signalType.substr(2));
        }
        else
        {
            Kind = ParseSignalKind(signalType);
        }
    }
}

const char* GSF::TimeSeries::SignalKindDescription[] =
{
    "Angle",
    "Magnitude",
    "Frequency",
    "DfDt",
    "Status",
    "Digital",
    "Analog",
    "Calculation",
    "Statistic",
    "Alarm",
    "Quality",
    "Unknown"
};

const char* GSF::TimeSeries::SignalKindAcronym[] =
{
    "PA",
    "PM",
    "FQ",
    "DF",
    "SF",
    "DV",
    "AV",
    "CV",
    "ST",
    "AL",
    "QF",
    "??"
};

// SignalReference.ToString()
ostream& GSF::TimeSeries::operator << (ostream& stream, const SignalReference& reference)
{
    if (reference.Index > 0)
        return stream << reference.Acronym << "-" << SignalKindAcronym[reference.Kind] << reference.Index;

    return stream << reference.Acronym << "-" << SignalKindAcronym[reference.Kind];
}

string GSF::TimeSeries::GetSignalTypeAcronym(SignalKind kind, char phasorType)
{
    switch (kind)
    {
        case Angle:
            return toupper(phasorType) == 'V' ? "VPHA" : "IPHA";
        case Magnitude:
            return toupper(phasorType) == 'V' ? "VPHM" : "IPHM";
        case Frequency:
            return "FREQ";
        case DfDt:
            return "DFDT";
        case Status:
            return "FLAG";
        case Digital:
            return "DIGI";
        case Analog:
            return "ALOG";
        case Calculation:
            return "CALC";
        case Statistic:
            return "STAT";
        case Alarm:
            return "ALRM";
        case Quality:
            return "QUAL";
        case Unknown:
        default:
            return "NULL";
    }
}

std::string TimeSeries::GetEngineeringUnits(const std::string& signalType)
{
    if (IsEqual(signalType, "IPHM"))
        return "Amps";

    if (IsEqual(signalType, "VPHM"))
        return "Volts";

    if (IsEqual(signalType, "FREQ"))
        return "Hz";

    if (EndsWith(signalType, "PHA"))
        return "Degrees";

    return Empty::String;
}

std::string GSF::TimeSeries::GetProtocolType(const std::string& protocolName)
{
    if (StartsWith(protocolName, "Gateway") ||
        StartsWith(protocolName, "Modbus") ||
        StartsWith(protocolName, "DNP"))
            return "Measurement";

    return "Frame";
}

void TimeSeries::ParseMeasurementKey(const std::string& key, std::string& source, uint32_t& id)
{
    const vector<string> parts = Split(key, ":");

    if (parts.size() == 2)
    {
        source = parts[0];
        id = uint32_t(stoul(parts[1]));
    }
    else
    {
        source = parts[0];
        id = UInt32::MaxValue;
    }
}

// Gets the "SignalKind" enum for the specified "acronym".
//  params:
//	   acronym: Acronym of the desired "SignalKind"
//  returns: The "SignalKind" for the specified "acronym".
SignalKind GSF::TimeSeries::ParseSignalKind(const string& acronym)
{
    if (acronym == "PA") // Phase Angle
        return SignalKind::Angle;

    if (acronym == "PM") // Phase Magnitude
        return SignalKind::Magnitude;

    if (acronym == "FQ") // Frequency
        return SignalKind::Frequency;

    if (acronym == "DF") // dF/dt
        return SignalKind::DfDt;

    if (acronym == "SF") // Status Flags
        return SignalKind::Status;

    if (acronym == "DV") // Digital Value
        return SignalKind::Digital;

    if (acronym == "AV") // Analog Value
        return SignalKind::Analog;

    if (acronym == "CV") // Calculated Value
        return SignalKind::Calculation;

    if (acronym == "ST") // Statistical Value
        return SignalKind::Statistic;

    if (acronym == "AL") // Alarm Value
        return SignalKind::Alarm;

    if (acronym == "QF") // Quality Flags
        return SignalKind::Quality;

    return SignalKind::Unknown;
}

TSSCPointMetadata::TSSCPointMetadata(function<void(int32_t, int32_t)> writeBits) :
    TSSCPointMetadata::TSSCPointMetadata(
        std::move(writeBits),
        function<int32_t()>(nullptr),
        function<int32_t()>(nullptr))
{
}

TSSCPointMetadata::TSSCPointMetadata(function<int32_t()> readBit, function<int32_t()> readBits5) :
    TSSCPointMetadata::TSSCPointMetadata(
        function<void(int32_t, int32_t)>(nullptr),
        std::move(readBit),
        std::move(readBits5))
{
}

TSSCPointMetadata::TSSCPointMetadata(
    function<void(int32_t, int32_t)> writeBits,
    function<int32_t()> readBit,
    function<int32_t()> readBits5) :
    m_commandsSentSinceLastChange(0),
    m_mode(4),
    m_mode21(0),
    m_mode31(0),
    m_mode301(0),
    m_mode41(TSSCCodeWords::Value1),
    m_mode401(TSSCCodeWords::Value2),
    m_mode4001(TSSCCodeWords::Value3),
    m_startupMode(0),
    m_writeBits(std::move(writeBits)),
    m_readBit(std::move(readBit)),
    m_readBits5(std::move(readBits5)),
    PrevNextPointId1(0),
    PrevQuality1(0),
    PrevQuality2(0),
    PrevValue1(0),
    PrevValue2(0),
    PrevValue3(0)
{
    for (uint8_t i = 0; i < CommandStatsLength; i++)
        m_commandStats[i] = 0;
}

void TSSCPointMetadata::WriteCode(int32_t code)
{
    switch (m_mode)
    {
        case 1:
            m_writeBits(code, 5);
            break;
        case 2:
            if (code == m_mode21)
                m_writeBits(1, 1);
            else
                m_writeBits(code, 6);
            break;
        case 3:
            if (code == m_mode31)
                m_writeBits(1, 1);
            else if (code == m_mode301)
                m_writeBits(1, 2);
            else
                m_writeBits(code, 7);
            break;
        case 4:
            if (code == m_mode41)
                m_writeBits(1, 1);
            else if (code == m_mode401)
                m_writeBits(1, 2);
            else if (code == m_mode4001)
                m_writeBits(1, 3);
            else
                m_writeBits(code, 8);
            break;
        default:
            throw PublisherException("Coding Error");
    }

    UpdatedCodeStatistics(code);
}

int32_t TSSCPointMetadata::ReadCode()
{
    int32_t code;

    switch (m_mode)
    {
        case 1:
            code = m_readBits5();
            break;
        case 2:
            if (m_readBit() == 1)
                code = m_mode21;
            else
                code = m_readBits5();
            break;
        case 3:
            if (m_readBit() == 1)
                code = m_mode31;
            else if (m_readBit() == 1)
                code = m_mode301;
            else
                code = m_readBits5();
            break;
        case 4:
            if (m_readBit() == 1)
                code = m_mode41;
            else if (m_readBit() == 1)
                code = m_mode401;
            else if (m_readBit() == 1)
                code = m_mode4001;
            else
                code = m_readBits5();
            break;
        default:
            throw SubscriberException("Unsupported compression mode");
    }

    UpdatedCodeStatistics(code);
    return code;
}

void TSSCPointMetadata::UpdatedCodeStatistics(int32_t code)
{
    m_commandsSentSinceLastChange++;
    m_commandStats[code]++;

    if (m_startupMode == 0 && m_commandsSentSinceLastChange > 5)
    {
        m_startupMode++;
        AdaptCommands();
    }
    else if (m_startupMode == 1 && m_commandsSentSinceLastChange > 20)
    {
        m_startupMode++;
        AdaptCommands();
    }
    else if (m_startupMode == 2 && m_commandsSentSinceLastChange > 100)
    {
        AdaptCommands();
    }
}

void TSSCPointMetadata::AdaptCommands()
{
    uint8_t code1 = 0;
    int32_t count1 = 0;

    uint8_t code2 = 1;
    int32_t count2 = 0;

    uint8_t code3 = 2;
    int32_t count3 = 0;

    int32_t total = 0;

    for (int32_t i = 0; i < CommandStatsLength; i++)
    {
        const int32_t count = m_commandStats[i];
        m_commandStats[i] = 0;

        total += count;

        if (count > count3)
        {
            if (count > count1)
            {
                code3 = code2;
                count3 = count2;

                code2 = code1;
                count2 = count1;

                code1 = static_cast<uint8_t>(i);
                count1 = count;
            }
            else if (count > count2)
            {
                code3 = code2;
                count3 = count2;

                code2 = static_cast<uint8_t>(i);
                count2 = count;
            }
            else
            {
                code3 = static_cast<uint8_t>(i);
                count3 = count;
            }
        }
    }

    const int32_t mode1Size = total * 5;
    const int32_t mode2Size = count1 * 1 + (total - count1) * 6;
    const int32_t mode3Size = count1 * 1 + count2 * 2 + (total - count1 - count2) * 7;
    const int32_t mode4Size = count1 * 1 + count2 * 2 + count3 * 3 + (total - count1 - count2 - count3) * 8;

    int32_t minSize = Int32::MaxValue;

    minSize = min(minSize, mode1Size);
    minSize = min(minSize, mode2Size);
    minSize = min(minSize, mode3Size);
    minSize = min(minSize, mode4Size);

    if (minSize == mode1Size)
    {
        m_mode = 1;
    }
    else if (minSize == mode2Size)
    {
        m_mode = 2;
        m_mode21 = code1;
    }
    else if (minSize == mode3Size)
    {
        m_mode = 3;
        m_mode31 = code1;
        m_mode301 = code2;
    }
    else if (minSize == mode4Size)
    {
        m_mode = 4;
        m_mode41 = code1;
        m_mode401 = code2;
        m_mode4001 = code3;
    }
    else
    {
        if (m_writeBits == nullptr)
            throw SubscriberException("Coding Error");
        else
            throw PublisherException("Coding Error");
    }

    m_commandsSentSinceLastChange = 0;
}