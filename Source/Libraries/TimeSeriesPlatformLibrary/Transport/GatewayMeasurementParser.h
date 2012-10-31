//******************************************************************************************************
//  GatewayMeasurementParser.h - Gbtc
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

#ifndef __GATEWAY_MEASUREMENT_PARSER_H
#define __GATEWAY_MEASUREMENT_PARSER_H

#include <cstddef>
#include "../Common/Measurement.h"
#include "../Common/Types.h"

namespace GSF {
namespace TimeSeries {
namespace Transport
{
	// Base class for measurement parsers.
	class GatewayMeasurementParser
	{
	public:
		// Attempts to parse a measurement from the buffer. Return value of false indicates
		// that there is not enough data to parse the measurement. Offset and length will be
		// updated by this method to indicate how many bytes were used when parsing.
		// Measurements can be partially parsed if there is not enough data in the buffer,
		// as long as parsing can be resumed by calling this method again with more data.
		virtual bool TryParseMeasurement(uint8_t* buffer, std::size_t& offset, std::size_t& length) = 0;

		// Returns the measurement that was parsed by the last successful call to TryParseMeasurement.
		virtual Measurement GetParsedMeasurement() const = 0;
	};
}}}

#endif