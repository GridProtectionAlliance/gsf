//******************************************************************************************************
//  Types.h - Gbtc
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

#ifndef __TRANSPORT_TYPES_H
#define __TRANSPORT_TYPES_H

#include <string>
#include <ctime>

#include "../Common/Types.h"

using namespace std;

namespace GSF {
namespace TimeSeries
{
	// Fundamental data type used by the Time Series Framework
	struct Measurement
	{
		// Identification number used in
		// human-readable measurement key.
		uint32_t ID;

		// Source used in human-
		// readable measurement key.
		string Source;

		// Measurement's globally
		// unique identifier.
		Guid SignalID;

		// Human-readable tag name to
		// help describe the measurement.
		string Tag;

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
		Measurement();

		// Returns the value after applying the
		// multiplicative and additive value modifiers.
		float64_t AdjustedValue();

		// Gets time in UNIX second of century and milliseconds
		void GetUnixTime(time_t& unixSOC, int16_t& milliseconds);
	};

	enum SignalKind : int16_t
	{			
		Angle,			// Phase angle
		Magnitude,		// Phase magnitude
		Frequency,		// Line frequency
		DfDt,			// Frequency delta over time (dF/dt)
		Status,			// Status flags
		Digital,		// Digital value
		Analog,			// Analog value
		Calculation,	// Calculated value
		Statistic,		// Statistical value
		Alarm,			// Alarm value
		Quality,		// Quality flags
		Unknown			// Undetermined signal type
	};

	// Helper function to parse signal kind
	SignalKind ParseSignalKind(string acronym);

	struct SignalReference
	{
		Guid SignalID;		// Unique UUID of this individual measurement (key to MeasurementMetaData.SignalID)
		string Acronym;		// Associated (parent) device for measurement (key to DeviceMetaData.Acronym / MeasurementMetaData.DeviceAcronym)
		int16_t Index;		// For phasors, digitals and analogs - this is the ordered index, uses 1-based indexing
		SignalKind Kind;	// Signal classification (e.g., phase angle, but not specific type of voltage or current)

		SignalReference();
		SignalReference(const string& signal);
	};

	struct MeasurementMetaData
	{
		string DeviceAcronym;		// Associated (parent) device for measurement (key to DeviceMetaData.Acronym)
		string ID;					// Measurement key string, format: "source:index" (if useful)
		Guid SignalID;				// Unique UUID of this individual measurement (lookup key!)
		string PointTag;			// Well formatted tag name for historians, e.g., OSI-PI, etc.
		SignalReference Reference;	// Parsed signal reference structure
		int PhasorSourceIndex;		// Measurement phasor index, if measurement represents a "Phasor"
		string Description;			// Detailed measurement description (free-form)
		time_t UpdatedOn;			// Time of last meta-data update

		MeasurementMetaData();

		// As this structure is "returned" from functions,
		// copy constructor is needed.
		MeasurementMetaData(const MeasurementMetaData& value);
	};

	struct PhasorMetaData
	{
		string DeviceAcronym;	// Associated (parent) device for phasor (key to DeviceMetaData.Acronym)
		string Label;			// Channel name for "phasor" (covers two measurements)
		string Type;			// Phasor type, i.e., "V" for voltage or "I" for current
		string Phase;			// Phasor phase, one of, "+", "-", "0", "A", "B" or "C"
		int SourceIndex;		// Phasor ordered index, uses 1-based indexing (key to MeasurementMetaData.PhasorSourceIndex)
		time_t UpdatedOn;		// Time of last meta-data update
	};

	struct PhasorReference
	{
		PhasorMetaData Phasor;			// Phasor metadata, includes phasor type, i.e., voltage or current
		MeasurementMetaData Angle;		// Angle measurement metadata for phasor
		MeasurementMetaData Magnitude;	// Magnitude measurement metadata for phasor
	};

	struct DeviceMetaData
	{
		string Acronym;				// Alpha-numeric device, e.g., pmu/station name (all-caps)
		string Name;				// User-defined deivce name / description (free-form)
		Guid UniqueID;			    // Device unique UUID (used for C37.118 v3 config frame)
		int AccessID;				// ID code used for device connection / reference
		int FramesPerSecond;		// Device reporting rate, e.g., 30 fps
		string CompanyAcronym;		// Original device company name (if useful)
		double Longitude;			// Device longitude (if reported)
		double Latitude;			// Device latitude (if reported)
		time_t UpdatedOn;			// Time of last meta-data update
		//string ParentAcronym;		// Original PDC name (if useful)
		//string ProtocolName;		// Original protocol name (if useful)
		//string VendorAcronym;		// Original device vendor name (if useful)
		//string VendorDeviceName;	// Original vendor device name, e.g., PMU brand (if useful)

		// Associated measurement and phasor meta-data
		vector<MeasurementMetaData> Measurements;
		vector<PhasorReference> Phasors;
	};

	// Defines the configuration frame "structure" for a device data frame
	struct ConfigurationFrame
	{
		string DeviceAcronym;
		MeasurementMetaData StatusFlags;
		MeasurementMetaData Frequency;
		vector<PhasorReference> Phasors;
		vector<MeasurementMetaData> Analogs;
		vector<MeasurementMetaData> Digitals;
	};

	struct Phasor
	{
		Measurement Angle;
		Measurement Magnitude;
	};
	
	// Holds the actual values, in order, for a device frame at a specific timestamp
	struct DataFrame
	{
		string DeviceAcronym;
		time_t SOC;
		int milliseconds;
		Measurement StatusFlags;
		Measurement Frequency;
		vector<Phasor> Phasors;
		vector<Measurement> Analogs;
		vector<Measurement> Digitals;
	};
}}

#endif