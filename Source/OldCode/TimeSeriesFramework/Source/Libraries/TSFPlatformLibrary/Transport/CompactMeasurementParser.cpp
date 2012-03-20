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
unsigned int tsf::Transport::CompactMeasurementParser::MapToFullFlags(int compactFlags) const
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
int tsf::Transport::CompactMeasurementParser::GetMeasurementByteLength() const
{
	const int FixedLength = 7;
	int length = FixedLength;

	if(m_includeTime)
		length += 8;

	return length;
}

// Attempts to parse a measurement from the buffer. Return value of false indicates
// that there is not enough data to parse the measurement. Offset and length will be
// updated by this method to indicate how many bytes were used when parsing.
bool tsf::Transport::CompactMeasurementParser::TryParseMeasurement(char buffer[], int& offset, int& length)
{
	int compactFlags;
	unsigned short runtimeID;
	tsf::Guid signalID;
	std::string measurementSource;
	unsigned int measurementID;
	float measurementValue;
	long timestamp = 0L;

	int end = offset + length;

	if (length < GetMeasurementByteLength())
		return false;

	compactFlags = buffer[offset] & 0xFF;
	++offset;

	runtimeID = m_endianConverter.ConvertBigEndian<unsigned short>(*(unsigned short*)(buffer + offset));
	m_signalIndexCache.GetMeasurementKey(runtimeID, signalID, measurementSource, measurementID);
	offset += 2;

	measurementValue = m_endianConverter.ConvertBigEndian<float>(*(float*)(buffer + offset));
	offset += 4;

	if(m_includeTime)
	{
		timestamp = m_endianConverter.ConvertBigEndian<long>(*(long*)(buffer + offset));
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