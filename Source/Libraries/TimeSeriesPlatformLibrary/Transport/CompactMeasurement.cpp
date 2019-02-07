//******************************************************************************************************
//  CompactMeasurement.cpp - Gbtc
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
//  03/09/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#include "CompactMeasurement.h"
#include "../Common/EndianConverter.h"

using namespace std;
using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

// These constants represent each flag in the 8-bit compact measurement state flags.
static const uint8_t CompactDataRangeFlag = 0x01;
static const uint8_t CompactDataQualityFlag = 0x02;
static const uint8_t CompactTimeQualityFlag = 0x04;
static const uint8_t CompactSystemIssueFlag = 0x08;
static const uint8_t CompactCalculatedValueFlag = 0x10;
static const uint8_t CompactDiscardedValueFlag = 0x20;
static const uint8_t CompactBaseTimeOffsetFlag = 0x40;
static const uint8_t CompactTimeIndexFlag = 0x80;

// These constants are masks used to set flags within the full 32-bit measurement state flags.
static const uint32_t DataRangeMask = 0x000000FC;
static const uint32_t DataQualityMask = 0x0000EF03;
static const uint32_t TimeQualityMask = 0x00BF0000;
static const uint32_t SystemIssueMask = 0xE0000000;
static const uint32_t CalculatedValueMask = 0x00001000;
static const uint32_t DiscardedValueMask = 0x00400000;

// Takes the 8-bit compact measurement flags and maps
// them to the full 32-bit measurement flags format.
inline uint32_t MapToFullFlags(uint8_t compactFlags)
{
    uint32_t fullFlags = 0U;

    if ((compactFlags & CompactDataRangeFlag) > 0)
        fullFlags |= DataRangeMask;

    if ((compactFlags & CompactDataQualityFlag) > 0)
        fullFlags |= DataQualityMask;

    if ((compactFlags & CompactTimeQualityFlag) > 0)
        fullFlags |= TimeQualityMask;

    if ((compactFlags & CompactSystemIssueFlag) > 0)
        fullFlags |= SystemIssueMask;

    if ((compactFlags & CompactCalculatedValueFlag) > 0)
        fullFlags |= CalculatedValueMask;

    if ((compactFlags & CompactDiscardedValueFlag) > 0)
        fullFlags |= DiscardedValueMask;

    return fullFlags;
}

// Takes the full 32-bit measurement flags format and
// maps them to the 8-bit compact measurement flags.
inline uint32_t MapToCompactFlags(uint32_t fullFlags)
{
    uint32_t compactFlags = 0U;

    if ((fullFlags & DataRangeMask) > 0)
        compactFlags |= CompactDataRangeFlag;

    if ((fullFlags & DataQualityMask) > 0)
        compactFlags |= CompactDataQualityFlag;

    if ((fullFlags & TimeQualityMask) > 0)
        compactFlags |= CompactTimeQualityFlag;

    if ((fullFlags & SystemIssueMask) > 0)
        compactFlags |= CompactSystemIssueFlag;

    if ((fullFlags & CalculatedValueMask) > 0)
        compactFlags |= CompactCalculatedValueFlag;

    if ((fullFlags & DiscardedValueMask) > 0)
        compactFlags |= CompactDiscardedValueFlag;

    return compactFlags;
}

CompactMeasurement::CompactMeasurement(SignalIndexCache& signalIndexCache, int64_t* baseTimeOffsets, bool includeTime, bool useMillisecondResolution, int32_t timeIndex) :
    m_signalIndexCache(signalIndexCache),
    m_baseTimeOffsets(baseTimeOffsets),
    m_includeTime(includeTime),
    m_useMillisecondResolution(useMillisecondResolution),
    m_timeIndex(timeIndex)
{
}

// Gets the byte length of measurements parsed by this parser.
uint32_t CompactMeasurement::GetMeasurementByteLength(const bool usingBaseTimeOffset) const
{
    uint32_t byteLength = 7;

    if (m_includeTime)
    {
        if (usingBaseTimeOffset)
        {
            if (m_useMillisecondResolution)
                byteLength += 2; // Use two bytes for millisecond resolution timestamp with valid offset
            else
                byteLength += 4; // Use four bytes for tick resolution timestamp with valid offset
        }
        else
        {
            byteLength += 8; // Use eight bytes for full fidelity time
        }
    }

    return byteLength;
}

// Attempts to parse a measurement from the buffer. Return value of false indicates
// that there is not enough data to parse the measurement. Offset and length will be
// updated by this method to indicate how many bytes were used when parsing.
bool CompactMeasurement::TryParseMeasurement(uint8_t* data, uint32_t& offset, uint32_t length, MeasurementPtr& measurement) const
{
    // Ensure that we at least have enough
    // data to read the compact state flags
    if (length - offset < 1)
        return false;

    // Read the compact state flags to determine
    // the size of the measurement being parsed
    const uint8_t compactFlags = data[offset] & 0xFF;
    const int32_t timeIndex = compactFlags & CompactTimeIndexFlag ? 1 : 0;
    const bool usingBaseTimeOffset = (compactFlags & CompactBaseTimeOffsetFlag) != 0;    

    // If we are using base time offsets, ensure that it is defined
    if (usingBaseTimeOffset && (m_baseTimeOffsets == nullptr || m_baseTimeOffsets[timeIndex] == 0))
        return false;

    // Ensure that we have enough data to read the rest of the measurement
    if (length - offset < GetMeasurementByteLength(usingBaseTimeOffset))
        return false;

    // Read the signal index from the buffer
    const uint16_t signalIndex = EndianConverter::ToBigEndian<uint16_t>(data, offset + 1);

    // If the signal index is not found in the cache, we cannot parse the measurement
    if (!m_signalIndexCache.Contains(signalIndex))
        return false;

    Guid signalID;
    string measurementSource;
    uint32_t measurementID;
    int64_t timestamp = 0;

    // Now that we've validated our failure conditions we can safely start advancing the offset
    m_signalIndexCache.GetMeasurementKey(signalIndex, signalID, measurementSource, measurementID);
    offset += 3;

    // Read the measurement value from the buffer
    const float32_t measurementValue = EndianConverter::ToBigEndian<float32_t>(data, offset);
    offset += 4;

    if (m_includeTime)
    {
        if (!usingBaseTimeOffset)
        {
            // Read full 8-byte timestamp from the buffer
            timestamp = EndianConverter::ToBigEndian<int64_t>(data, offset);
            offset += 8;
        }
        else if (!m_useMillisecondResolution)
        {
            // Read 4-byte offset from the buffer and apply the appropriate base time offset
            timestamp = EndianConverter::ToBigEndian<uint32_t>(data, offset);
            timestamp += m_baseTimeOffsets[timeIndex]; //-V522
            offset += 4;
        }
        else
        {
            // Read 2-byte offset from the buffer, convert from milliseconds to ticks, and apply the appropriate base time offset
            timestamp = EndianConverter::ToBigEndian<uint16_t>(data, offset);
            timestamp *= 10000;
            timestamp += m_baseTimeOffsets[timeIndex];
            offset += 2;
        }
    }

    measurement = NewSharedPtr<Measurement>();
    measurement->Flags = MapToFullFlags(compactFlags);
    measurement->SignalID = signalID;
    measurement->Source = measurementSource;
    measurement->ID = measurementID;
    measurement->Value = measurementValue;
    measurement->Timestamp = timestamp;

    return true;
}

void CompactMeasurement::SerializeMeasurement(const MeasurementPtr& measurement, vector<uint8_t>& buffer) const
{
    // Define the compact state flags
    uint8_t compactFlags = MapToCompactFlags(measurement->Flags);

    int64_t difference = 0L;
    bool usingBaseTimeOffset = false;

    if (m_baseTimeOffsets != nullptr)
    {
        // See if timestamp will fit within space allowed for active base offset. We cache result so that post call
        // to binary length, result will speed other subsequent parsing operations by not having to reevaluate.
        difference = measurement->Timestamp - m_baseTimeOffsets[m_timeIndex];
        
        usingBaseTimeOffset = difference > 0 ? 
            (m_useMillisecondResolution ? difference / Ticks::PerMillisecond < UInt16::MaxValue : difference < UInt16::MaxValue) : false;
    }

    if (usingBaseTimeOffset)
        compactFlags |= CompactBaseTimeOffsetFlag;

    if (m_timeIndex != 0)
        compactFlags |= CompactTimeIndexFlag;

    // Added encoded compact state flags to beginning of buffer
    buffer.push_back(compactFlags);

    // Encode runtime ID
    EndianConverter::WriteBigEndianBytes(buffer, m_signalIndexCache.GetSignalIndex(measurement->SignalID));

    // Encode adjusted value (accounts for adder and multiplier)
    EndianConverter::WriteBigEndianBytes(buffer, static_cast<float32_t>(measurement->AdjustedValue()));

    if (!m_includeTime)
        return;

    if (usingBaseTimeOffset)
    {
        if (m_useMillisecondResolution)
        {
            // Encode 2-byte millisecond offset timestamp
            EndianConverter::WriteBigEndianBytes(buffer, static_cast<uint16_t>(difference / Ticks::PerMillisecond));
        }
        else
        {
            // Encode 4-byte ticks offset timestamp
            EndianConverter::WriteBigEndianBytes(buffer, static_cast<uint32_t>(difference));
        }
    }
    else
    {
        // Encode 8-byte full fidelity timestamp
        EndianConverter::WriteBigEndianBytes(buffer, measurement->Timestamp);
    }
}
