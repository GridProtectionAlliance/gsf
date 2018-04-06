//******************************************************************************************************
//  CompactMeasurementParser.h - Gbtc
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

#ifndef __COMPACT_MEASUREMENT_PARSER_H
#define __COMPACT_MEASUREMENT_PARSER_H

#include <cstddef>

#include "../Common/EndianConverter.h"
#include "Types.h"
#include "SignalIndexCache.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
	// Parser for the compact measurement format of the Gateway Exchange Protocol.
	class CompactMeasurementParser
	{
	private:
		EndianConverter m_endianConverter;
		MeasurementPtr m_parsedMeasurement;
		
		SignalIndexCache& m_signalIndexCache;
		int64_t* m_baseTimeOffsets;
		bool m_includeTime;
		bool m_useMillisecondResolution;

		// Takes the 8-bit compact measurement flags and maps
		// them to the full 32-bit measurement flags format.
		static uint32_t MapToFullFlags(uint8_t compactFlags) ;

		// Gets the byte length of measurements parsed by this parser.
		size_t GetMeasurementByteLength(bool usingBaseTimeOffset) const;

	public:
		// Creates a new instance of the compact measurement parser.
		CompactMeasurementParser(SignalIndexCache& signalIndexCache, int64_t* baseTimeOffsets = 0, bool includeTime = true, bool useMillisecondResolution = false);

		// Returns the measurement that was parsed by the last successful call to TryParseMeasurement.
		MeasurementPtr GetParsedMeasurement() const;

		// Attempts to parse a measurement from the buffer. Return value of false indicates
		// that there is not enough data to parse the measurement. Offset and length will be
		// updated by this method to indicate how many bytes were used when parsing.
		bool TryParseMeasurement(const uint8_t* buffer, size_t& offset, size_t& length);

		// These constants represent each flag in the 8-bit compact measurement state flags.
		static const uint8_t CompactDataRangeFlag       = 0x01;
		static const uint8_t CompactDataQualityFlag     = 0x02;
		static const uint8_t CompactTimeQualityFlag     = 0x04;
		static const uint8_t CompactSystemIssueFlag     = 0x08;
		static const uint8_t CompactCalculatedValueFlag = 0x10;
		static const uint8_t CompactDiscardedValueFlag  = 0x20;
		static const uint8_t CompactBaseTimeOffsetFlag  = 0x40;
		static const uint8_t CompactTimeIndexFlag       = 0x80;

		// These constants are masks used to set flags within the full 32-bit measurement state flags.
		static const uint32_t DataRangeMask       = 0x000000FC;
		static const uint32_t DataQualityMask     = 0x0000EF03;
		static const uint32_t TimeQualityMask     = 0x00BF0000;
		static const uint32_t SystemIssueMask     = 0xE0000000;
		static const uint32_t CalculatedValueMask = 0x00001000;
		static const uint32_t DiscardedValueMask  = 0x00400000;
	};
}}}

#endif