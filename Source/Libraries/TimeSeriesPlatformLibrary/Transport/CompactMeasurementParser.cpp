//******************************************************************************************************
//  CompactMeasurementParser.cpp - Gbtc
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
//  03/09/2012 - Stephen C. Wills
//       Generated original version of source code.
//
//******************************************************************************************************

#include "CompactMeasurementParser.h"

using namespace GSF::TimeSeries;
using namespace GSF::TimeSeries::Transport;

CompactMeasurementParser::CompactMeasurementParser(SignalIndexCache& signalIndexCache, int64_t* baseTimeOffsets, bool includeTime, bool useMillisecondResolution) :
    m_parsedMeasurement(nullptr),
    m_signalIndexCache(signalIndexCache),
    m_baseTimeOffsets(baseTimeOffsets),
    m_includeTime(includeTime),
    m_useMillisecondResolution(useMillisecondResolution)
{
}

MeasurementPtr CompactMeasurementParser::GetParsedMeasurement() const
{
    return m_parsedMeasurement;
}

// Takes the 8-bit compact measurement flags and maps
// them to the full 32-bit measurement flags format.
uint32_t CompactMeasurementParser::MapToFullFlags(uint8_t compactFlags)
{
    unsigned int fullFlags = 0;

    if ((compactFlags & CompactMeasurementParser::CompactDataRangeFlag) > 0)
        fullFlags |= CompactMeasurementParser::DataRangeMask;

    if ((compactFlags & CompactMeasurementParser::CompactDataQualityFlag) > 0)
        fullFlags |= CompactMeasurementParser::DataQualityMask;

    if ((compactFlags & CompactMeasurementParser::CompactTimeQualityFlag) > 0)
        fullFlags |= CompactMeasurementParser::TimeQualityMask;

    if ((compactFlags & CompactMeasurementParser::CompactSystemIssueFlag) > 0)
        fullFlags |= CompactMeasurementParser::SystemIssueMask;

    if ((compactFlags & CompactMeasurementParser::CompactCalculatedValueFlag) > 0)
        fullFlags |= CompactMeasurementParser::CalculatedValueMask;

    if ((compactFlags & CompactMeasurementParser::CompactDiscardedValueFlag) > 0)
        fullFlags |= CompactMeasurementParser::DiscardedValueMask;

    return fullFlags;
}

// Gets the byte length of measurements parsed by this parser.
uint32_t CompactMeasurementParser::GetMeasurementByteLength(bool usingBaseTimeOffset) const
{
    uint32_t byteLength = 7;

    if (m_includeTime)
    {
        if (!usingBaseTimeOffset)
            byteLength += 8;
        else if (!m_useMillisecondResolution)
            byteLength += 4;
        else
            byteLength += 2;
    }

    return byteLength;
}

// Attempts to parse a measurement from the buffer. Return value of false indicates
// that there is not enough data to parse the measurement. Offset and length will be
// updated by this method to indicate how many bytes were used when parsing.
bool CompactMeasurementParser::TryParseMeasurement(const vector<uint8_t>& buffer, uint32_t& offset, uint32_t& length)
{
    // Ensure that we at least have enough
    // data to read the compact state flags
    if (length < 1)
        return false;

    const uint32_t end = offset + length;

    // Read the compact state flags to determine
    // the size of the measurement being parsed
    const uint8_t compactFlags = buffer[offset] & 0xFF;
    const bool usingBaseTimeOffset = (compactFlags & CompactBaseTimeOffsetFlag) != 0;    
    const int32_t timeIndex = (compactFlags & CompactTimeIndexFlag) ? 1 : 0;

    // If we are using base time offsets, ensure that it is defined
    if (usingBaseTimeOffset && (m_baseTimeOffsets == nullptr || m_baseTimeOffsets[timeIndex] == 0))
        return false;

    // Ensure that we have enough data to read the rest of the measurement
    if (length < GetMeasurementByteLength(usingBaseTimeOffset))
        return false;

    // Read the signal index from the buffer
    const uint16_t signalIndex = m_endianConverter.ConvertBigEndian<uint16_t>(*reinterpret_cast<const uint16_t*>(&buffer[offset + 1]));

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
    const float32_t measurementValue = m_endianConverter.ConvertBigEndian<float32_t>(*reinterpret_cast<const float32_t*>(&buffer[offset]));
    offset += 4;

    if (m_includeTime)
    {
        if (!usingBaseTimeOffset)
        {
            // Read full 8-byte timestamp from the buffer
            timestamp = m_endianConverter.ConvertBigEndian<int64_t>(*reinterpret_cast<const int64_t*>(&buffer[offset]));
            offset += 8;
        }
        else if (!m_useMillisecondResolution)
        {
            // Read 4-byte offset from the buffer and apply the appropriate base time offset
            timestamp = m_endianConverter.ConvertBigEndian<uint32_t>(*reinterpret_cast<const uint32_t*>(&buffer[offset]));
            timestamp += m_baseTimeOffsets[timeIndex];
            offset += 4;
        }
        else
        {
            // Read 2-byte offset from the buffer, convert from milliseconds to ticks, and apply the appropriate base time offset
            timestamp = m_endianConverter.ConvertBigEndian<uint16_t>(*reinterpret_cast<const uint16_t*>(&buffer[offset]));
            timestamp *= 10000;
            timestamp += m_baseTimeOffsets[timeIndex];
            offset += 2;
        }
    }

    length = end - offset;

    m_parsedMeasurement = NewSharedPtr<Measurement>();
    m_parsedMeasurement->Flags = MapToFullFlags(compactFlags);
    m_parsedMeasurement->SignalID = signalID;
    m_parsedMeasurement->Source = measurementSource;
    m_parsedMeasurement->ID = measurementID;
    m_parsedMeasurement->Value = measurementValue;
    m_parsedMeasurement->Timestamp = timestamp;

    return true;
}