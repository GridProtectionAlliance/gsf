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

namespace tsf = TimeSeriesFramework;

// Takes the 8-bit compact measurement flags and maps
// them to the full 32-bit measurement flags format.
uint32_t tsf::Transport::CompactMeasurementParser::MapToFullFlags(uint8_t compactFlags) const
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

	if ((compactFlags & CompactMeasurementParser::CompactUserFlag) > 0)
		fullFlags |= CompactMeasurementParser::UserFlagMask;

	return fullFlags;
}

// Returns the measurement that was parsed by the last successful call to TryParseMeasurement.
std::size_t tsf::Transport::CompactMeasurementParser::GetMeasurementByteLength() const
{
	const std::size_t FixedLength = 7;
	std::size_t length = FixedLength;

	if(m_includeTime)
		length += 8;

	return length;
}

// Attempts to parse a measurement from the buffer. Return value of false indicates
// that there is not enough data to parse the measurement. Offset and length will be
// updated by this method to indicate how many bytes were used when parsing.
bool tsf::Transport::CompactMeasurementParser::TryParseMeasurement(uint8_t buffer[], std::size_t& offset, std::size_t& length)
{
	uint8_t compactFlags;
	uint16_t runtimeID;
	Guid signalID;
	std::string measurementSource;
	uint32_t measurementID;
	float32_t measurementValue;
	int64_t timestamp = 0;

	std::size_t end = offset + length;

	if (length < GetMeasurementByteLength())
		return false;

	compactFlags = buffer[offset] & 0xFF;
	++offset;

	runtimeID = m_endianConverter.ConvertBigEndian<uint16_t>(*(uint16_t*)(buffer + offset));
	m_signalIndexCache.GetMeasurementKey(runtimeID, signalID, measurementSource, measurementID);
	offset += 2;

	measurementValue = m_endianConverter.ConvertBigEndian<float32_t>(*(float32_t*)(buffer + offset));
	offset += 4;

	if(m_includeTime)
	{
		timestamp = m_endianConverter.ConvertBigEndian<int64_t>(*(int64_t*)(buffer + offset));
		offset += 8;
	}

	length = end - offset;

	m_parsedMeasurement.Flags = MapToFullFlags(compactFlags);
	m_parsedMeasurement.SignalID = signalID;
	m_parsedMeasurement.Source = measurementSource;
	m_parsedMeasurement.ID = measurementID;
	m_parsedMeasurement.Value = measurementValue;
	m_parsedMeasurement.Timestamp = timestamp;

	return true;
}