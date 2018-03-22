//******************************************************************************************************
//  Measurement.h - Gbtc
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

#ifndef __MEASUREMENT_H
#define __MEASUREMENT_H

#include <string>
#include <ctime>

#include "Types.h"
#include "Convert.h"

namespace GSF {
namespace TimeSeries
{
	// Fundamental data type used by
	// the Time Series Framework.
	struct Measurement
	{
		// Identification number used in
		// human-readable measurement key.
		uint32_t ID;

		// Source used in human-
		// readable measurement key.
		std::string Source;

		// Measurement's globally
		// unique identifier.
		Guid SignalID;

		// Human-readable tag name to
		// help describe the measurement.
		std::string Tag;

		// Instantaneous value
		// of the measurement.
		float64_t Value;

		// Additive value modifier.
		float64_t Adder;

		// Multiplicative value modifier.
		float64_t Multiplier;

		// The time, in ticks, that
		// this measurement was taken.
		int64_t Timestamp;

		// Flags indicating the state of the measurement
		// as reported by the device that took it.
		uint32_t Flags;

		// Creates a new instance.
		Measurement() : Adder(0), Multiplier(1)
		{
		}

		// Returns the value after applying the
		// multiplicative and additive value modifiers.
		float64_t AdjustedValue()
		{
			return Value * Multiplier + Adder;
		}

		// Gets time in UNIX second of century and milliseconds
		void GetUnixTime(std::time_t& unixSOC, int& milliseconds)
		{
			GSF::TimeSeries::GetUnixTime(Timestamp, unixSOC, milliseconds);
		}
	};
}}

#endif