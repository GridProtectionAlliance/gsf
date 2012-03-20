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

#include "GatewayMeasurementParser.h"
#include "SignalIndexCache.h"
#include "../Measurement.h"
#include "../EndianConverter.h"

namespace TimeSeriesFramework {
namespace Transport
{
	// Parser for the compact measurement format of the Gateway Exchange Protocol.
	class CompactMeasurementParser : public GatewayMeasurementParser
	{
	private:
		SignalIndexCache& m_signalIndexCache;
		bool m_includeTime;

		EndianConverter m_endianConverter;
		Measurement m_parsedMeasurement;

		// Takes the 8-bit compact measurement flags and maps
		// them to the full 32-bit measurement flags format.
		unsigned int MapToFullFlags(int compactFlags) const;

	public:
		// Creates a new instance of the compact measurement parser
		// that parses compact measurements with the timestamp included.
		CompactMeasurementParser(SignalIndexCache& signalIndexCache)
			: m_signalIndexCache(signalIndexCache), m_includeTime(true)
		{
		}

		// Creates a new instance of the compact measurement parser that can parse measurements with or without the timestamp included.
		CompactMeasurementParser(SignalIndexCache& signalIndexCache, bool includeTime)
			: m_signalIndexCache(signalIndexCache), m_includeTime(includeTime)
		{
		}
		
		// Returns the measurement that was parsed by the last successful call to TryParseMeasurement.
		Measurement GetParsedMeasurement() const
		{
			return m_parsedMeasurement;
		}

		// Gets the byte length of the compact measurements parsed by this compact measurement parser.
		int GetMeasurementByteLength() const;

		// Attempts to parse a measurement from the buffer. Return value of false indicates
		// that there is not enough data to parse the measurement. Offset and length will be
		// updated by this method to indicate how many bytes were used when parsing.
		bool TryParseMeasurement(char buffer[], int& offset, int& length);

		// These constants represent each flag in the 8-bit compact measurement state flags.
		static const int CompactDataRangeFlag       = 0x01;
		static const int CompactDataQualityFlag     = 0x02;
		static const int CompactTimeQualityFlag     = 0x04;
		static const int CompactSystemIssueFlag     = 0x08;
		static const int CompactCalculatedValueFlag = 0x10;
		static const int CompactDiscardedValueFlag  = 0x20;
		static const int CompactUserFlag            = 0x40;
		static const int CompactTimeIndexFlag       = 0x80;

		// These constants are masks used to set flags within the full 32-bit measurement state flags.
		static const unsigned int DataRangeMask       = 0x000000FC;
		static const unsigned int DataQualityMask     = 0x0000EF03;
		static const unsigned int TimeQualityMask     = 0x00BF0000;
		static const unsigned int SystemIssueMask     = 0xE0000000;
		static const unsigned int UserFlagMask        = 0x1F000000;
		static const unsigned int CalculatedValueMask = 0x00001000;
		static const unsigned int DiscardedValueMask  = 0x00800000;
	};
}}

#endif