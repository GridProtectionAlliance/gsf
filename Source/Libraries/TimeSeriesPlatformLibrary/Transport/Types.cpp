//******************************************************************************************************
//  Types.cpp - Gbtc
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

#include <string>
#include <boost/algorithm/string/case_conv.hpp>
#include <boost/algorithm/string/trim.hpp>

#include "Types.h"
#include "../Common/Convert.h"

using namespace GSF::TimeSeries;
using namespace boost::algorithm;

Measurement::Measurement() : Adder(0), Multiplier(1)
{
}

float64_t Measurement::AdjustedValue()
{
	return Value * Multiplier + Adder;
}

void Measurement::GetUnixTime(time_t& unixSOC, int16_t& milliseconds)
{
	GSF::TimeSeries::GetUnixTime(Timestamp, unixSOC, milliseconds);
}

SignalReference::SignalReference()
{
}

SignalReference::SignalReference(const string& signal)
{
	// Signal reference may contain multiple dashes, we're interested in the last one
	int splitIndex = signal.find_last_of('-');

	// Assign default values to fields
	Index = 0;

	if (splitIndex > -1)
	{
		string signalType = to_upper_copy(trim_copy(signal.substr(splitIndex + 1)));
		Acronym = to_upper_copy(trim_copy(signal.substr(0, splitIndex)));

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
	else
	{
		// This represents an error - best we can do is assume entire string is the acronym
		Acronym = to_upper_copy(trim_copy(signal));
		Kind = SignalKind::Unknown;
	}
}

// Gets the "SignalKind" enum for the specified "acronym".
//  params:
//	   acronym: Acronym of the desired "SignalKind"
//  returns: The "SignalKind" for the specified "acronym".
SignalKind GSF::TimeSeries::ParseSignalKind(string acronym)
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

MeasurementMetaData::MeasurementMetaData()
{
}

MeasurementMetaData::MeasurementMetaData(const MeasurementMetaData& value)
{
	DeviceAcronym = value.DeviceAcronym;
	ID = value.ID;
	SignalID = value.SignalID;
	PointTag = value.PointTag;
	Reference = value.Reference;
	PhasorSourceIndex = value.PhasorSourceIndex;
	Description = value.Description;
	UpdatedOn = value.UpdatedOn;
}