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
#include <unordered_set>

#include "../Common/CommonTypes.h"

using namespace std;

namespace GSF {
namespace TimeSeries
{
    // Simple exception type thrown by the data subscriber
    class SubscriberException : public Exception
    {
    private:
        string m_message;

    public:
        SubscriberException(string message) noexcept;
        const char* what() const noexcept;
    };
    
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
        float64_t AdjustedValue() const;

        // Gets time in UNIX second of century and milliseconds
        void GetUnixTime(time_t& unixSOC, int16_t& milliseconds) const;
    };

    typedef SharedPtr<Measurement> MeasurementPtr;

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

    extern const char* SignalKindDescription[];

    extern const char* SignalKindAcronym[];

    // Helper function to parse signal kind
    SignalKind ParseSignalKind(string acronym);

    struct SignalReference
    {
        Guid SignalID;		// Unique UUID of this individual measurement (key to MeasurementMetadata.SignalID)
        string Acronym;		// Associated (parent) device for measurement (key to DeviceMetadata.Acronym / MeasurementMetadata.DeviceAcronym)
        int16_t Index;		// For phasors, digitals and analogs - this is the ordered index, uses 1-based indexing
        SignalKind Kind;	// Signal classification (e.g., phase angle, but not specific type of voltage or current)

        SignalReference();
        SignalReference(const string& signal);

    };

    ostream& operator << (ostream& stream, const SignalReference& reference);

    struct MeasurementMetadata
    {
        string DeviceAcronym;		// Associated (parent) device for measurement (key to DeviceMetadata.Acronym)
        string ID;					// Measurement key string, format: "source:index" (if useful)
        Guid SignalID;				// Unique UUID of this individual measurement (lookup key!)
        string PointTag;			// Well formatted tag name for historians, e.g., OSI-PI, etc.
        SignalReference Reference;	// Parsed signal reference structure
        int PhasorSourceIndex;		// Measurement phasor index, if measurement represents a "Phasor"
        string Description;			// Detailed measurement description (free-form)
        time_t UpdatedOn;			// Time of last meta-data update
    };

    typedef SharedPtr<MeasurementMetadata> MeasurementMetadataPtr;

    struct PhasorMetadata
    {
        string DeviceAcronym;	// Associated (parent) device for phasor (key to DeviceMetadata.Acronym)
        string Label;			// Channel name for "phasor" (covers two measurements)
        string Type;			// Phasor type, i.e., "V" for voltage or "I" for current
        string Phase;			// Phasor phase, one of, "+", "-", "0", "A", "B" or "C"
        int SourceIndex;		// Phasor ordered index, uses 1-based indexing (key to MeasurementMetadata.PhasorSourceIndex)
        time_t UpdatedOn;		// Time of last meta-data update
    };

    typedef SharedPtr<PhasorMetadata> PhasorMetadataPtr;

    struct PhasorReference
    {
        PhasorMetadataPtr Phasor;			// Phasor metadata, includes phasor type, i.e., voltage or current
        MeasurementMetadataPtr Angle;		// Angle measurement metadata for phasor
        MeasurementMetadataPtr Magnitude;	// Magnitude measurement metadata for phasor
    };

    typedef SharedPtr<PhasorReference> PhasorReferencePtr;

    struct DeviceMetadata
    {
        string Acronym;				// Alpha-numeric device, e.g., pmu/station name (all-caps)
        string Name;				// User-defined device name / description (free-form)
        Guid UniqueID;			    // Device unique UUID (used for C37.118 v3 config frame)
        int AccessID;				// ID code used for device connection / reference
        string ParentAcronym;		// Original PDC name (if useful / not assigned for directly connected devices)
        string ProtocolName;		// Original protocol name (if useful)
        int FramesPerSecond;		// Device reporting rate, e.g., 30 fps
        string CompanyAcronym;		// Original device company name (if useful)
        string VendorAcronym;		// Original device vendor name (if useful / provided)
        string VendorDeviceName;	// Original vendor device name, e.g., PMU brand (if useful / provided)
        double Longitude;			// Device longitude (if reported)
        double Latitude;			// Device latitude (if reported)
        time_t UpdatedOn;			// Time of last meta-data update

        // Associated measurement and phasor meta-data
        vector<MeasurementMetadataPtr> Measurements;
        vector<PhasorReferencePtr> Phasors;
    };

    typedef SharedPtr<DeviceMetadata> DeviceMetadataPtr;

    // Defines the configuration frame "structure" for a device data frame
    struct ConfigurationFrame
    {
        string DeviceAcronym;
        MeasurementMetadataPtr QualityFlags; // This measurement may be null, see below **
        MeasurementMetadataPtr StatusFlags;
        MeasurementMetadataPtr Frequency;
        vector<PhasorReferencePtr> Phasors;
        vector<MeasurementMetadataPtr> Analogs;
        vector<MeasurementMetadataPtr> Digitals;

        // Associated measurements
        unordered_set<Guid> Measurements;
    };

    typedef SharedPtr<ConfigurationFrame> ConfigurationFramePtr;

    // ** QualityFlags Note **
    //
    // The quality flags measurement contains the unsigned 32-bit integer value as
    // defined for the source protocol. In the case of IEEE C37.118, this will be the
    // TimeQualityFlags and TimeQualityIndicatorCode per the standard. To read the
    // value, look for the "Quality" signal kind and convert it to an uint32, e.g.:
    //
    //  for (auto &measurement : measurements)
    //  {
    //      const float64_t value = measurement->AdjustedValue();
    //      ConfigurationFramePtr configurationFrame;
    //      MeasurementMetadataPtr measurementMetadata;
    //  
    //      if (TryFindTargetConfigurationFrame(measurement->SignalID, configurationFrame))
    //      {
    //          if (TryGetMeasurementMetdataFromConfigurationFrame(measurement->SignalID, configurationFrame, measurementMetadata))
    //          {
    //              if (measurementMetadata->Reference.Kind == SignalKind::Quality)
    //              {
    //                  // Handle time quality flags
    //                  uint32_t timeQualityFlags = static_cast<uint32_t>(value);
    //              }
    //          }
    //      }
    //  }
    //
    // These time quality flags are only defined once per data frame and a data frame can
    // define multiple PMUs, e.g., in a data frame created by a PDC. When this C++ code
    // parses the incoming metadata, it defines only one PMU per configuration frame
    // structure. As a result, not all structures will have a quality flags defined, as
    // the source PMU may have come from a parent PDC data frame. The quality flags
    // measurement will only be defined when (1) the source was from a directly connected
    // PMU, i.e., a source data frame with exactly one PMU, and (2) the source protocol
    // supports a quality flags measurements, e.g., IEEE C37.118. Note that other source
    // protocols, e.g., IEEE 1344, do not define a quality flags value. When the quality
    // flags measurement is not available the QualityFlags measurement data pointer will
    // be null and consuming code should check for this expected condition.

    //struct Phasor
    //{
    //	Measurement Angle;
    //	Measurement Magnitude;
    //};
    
    // Holds the actual values, in order, for a device frame at a specific timestamp
    //struct DataFrame
    //{
    //	string DeviceAcronym;
    //	time_t SOC;
    //	int milliseconds;
    //	Measurement StatusFlags;
    //  Measurement QualityFlags;
    //	Measurement Frequency;
    //	vector<Phasor> Phasors;
    //	vector<Measurement> Analogs;
    //	vector<Measurement> Digitals;
    //};
}}

#endif