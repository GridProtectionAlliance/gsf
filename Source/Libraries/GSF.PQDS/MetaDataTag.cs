//******************************************************************************************************
//  MetaDataTag.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
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
//  03/06/2020 - Christoph Lackner
//       Generated original version of source code.
//
//******************************************************************************************************


using System;
using System.Collections.Generic;

namespace GSF.PQDS
{
    /// <summary>
    /// PQDS metadata tag Datatypes according to PQDS spec.
    /// </summary>
    public enum PQDSMetaDataType
    {
        /// <summary>
        /// Enumeration data type
        /// </summary>
        Enumeration = 0,
        /// <summary>
        /// Numeric data type
        /// </summary>
        Numeric = 1,
        /// <summary>
        /// AlphaNumeric data type
        /// </summary>
        AlphaNumeric = 2,
        /// <summary>
        /// Text data type
        /// </summary>
        Text = 3,
        /// <summary>
        /// Binary data type
        /// </summary>
        Binary = 4

    }

    /// <summary>
    /// Abstract Class of MetaData Tags for a <see cref="PQDSFile"/>.
    /// </summary>
    public abstract class MetaDataTag
    {
        #region[Properties]
        /// <summary>
        /// Meta data key value of row
        /// </summary>
        protected string m_key;
        /// <summary>
        /// Meta data unit value of row
        /// </summary>
        protected string m_unit;
        /// <summary>
        /// Meta data raw value of row
        /// </summary>
        protected string m_rawValue;

        /// <summary>
        /// Meta data expected data type of row
        /// </summary>
        protected PQDSMetaDataType m_expectedDataType;
        /// <summary>
        /// Meta data note value of row
        /// </summary>
        protected string m_note;

        #endregion[Properties]

        #region[Methods]

        /// <summary>
        /// the Metadata Tag key.
        /// </summary>
        public string Key { get { return (m_key); } }

        /// <summary>
        /// the Metadata Tag unit.
        /// </summary>
        public string Unit { get { return (m_unit); } }

        /// <summary>
        /// the Metadata Tag raw value without typing.
        /// </summary>
        public string RawValue { get { return (m_rawValue); } }

        /// <summary>
        /// Converst the Metadata tag into a line of a PQDS file
        /// </summary>
        /// <returns>The metadataTag as a String</returns>
        public abstract String Write();

        /// <summary>
        /// Returns the PQDS datatype <see cref="PQDSMetaDataType"/>
        /// </summary>
        /// <returns>The PQDS Datatype </returns>
        public abstract PQDSMetaDataType Type();

        #endregion[Methods]
    }

    /// <summary>
    /// Class of MetaData Tags for a <see cref="PQDSFile"/>.
    /// </summary>
    public class MetaDataTag<DataType> : MetaDataTag
    {
        #region[Properties]

        private DataType m_value;

        /// <summary>
        /// Value of the MetadataTag.
        /// </summary>
        public DataType Value { get { return m_value; } }

        #endregion[Properties]

        #region[Constructor]

        /// <summary>
        /// Creates a <see cref="MetaDataTag"/>.
        /// </summary>
        /// <param name="key"> key of the MetadataTag</param>
        /// <param name="value"> Value of the MetadataTag</param>
        public MetaDataTag(String key, DataType value)
        {
            m_value = value;
            m_rawValue = value?.ToString() ?? "";

            m_key = key;
            if (!keyToDataTypeLookup.TryGetValue(key, out m_expectedDataType))
                m_expectedDataType = PQDSMetaDataType.Text;

            if (!keyToUnitLookup.TryGetValue(key, out m_unit))
                m_unit = null;

            if (!keyToNoteLookup.TryGetValue(key, out m_note))
                m_note = null;

            //Check to ensure a string does not end up being a number etc...
            if (m_expectedDataType == PQDSMetaDataType.AlphaNumeric)
            {
                if (!((value is string) | (value is Guid)))
                { throw new InvalidCastException("Can not cast object to Alphanumeric Type"); }
            }
            else if (m_expectedDataType == PQDSMetaDataType.Numeric)
            {
                if (!((value is int) | (value is double)))
                { throw new InvalidCastException("Can not cast object to Numeric Type"); }
            }
            else if (m_expectedDataType == PQDSMetaDataType.Enumeration)
            {
                if (!((value is int)))
                { throw new InvalidCastException("Can not cast object to Numeric Type"); }
            }
            else if (m_expectedDataType == PQDSMetaDataType.Binary)
            {
                if (!((value is int) | (value is Boolean)))
                { throw new InvalidCastException("Can not cast object to Numeric Type"); }
            }

        }

        /// <summary>
        /// Creates a custom <see cref="MetaDataTag"/>.
        /// </summary>
        /// <param name="key"> key of the MetadataTag</param>
        /// <param name="value"> Value of the MetadataTag</param>
        /// <param name="valueType"> The <see cref="PQDSMetaDataType"/> of the metadata tag</param>
        /// <param name="unit"> The unit of the metadata tag </param>
        /// <param name="description"> a describtion of the metadata tag</param>
        public MetaDataTag(String key, DataType value, PQDSMetaDataType valueType, String unit, String description)
        {
            m_value = value;
            m_rawValue = value?.ToString() ?? "";
            m_key = key;
            m_expectedDataType = valueType;

            if (unit.Trim('"') == "") { m_unit = null; }
            else { m_unit = unit.Trim('"'); }

            if (description.Trim('"') == "") { m_note = null; }
            else { m_note = description.Trim('"'); }

        }

        #endregion[Constructor]

        #region[Methods]

        /// <summary>
        /// Converst the Metadata tag into a line of a PQDS file
        /// </summary>
        /// <returns>The metadataTag as a String</returns>
        public override string Write()
        {
            string result = String.Format("{0},\"{1}\",{2},{3},\"{4}\"",
                m_key, m_value, m_unit, DataTypeToCSV(m_expectedDataType), m_note);

            return result;
        }

        /// <summary>
        /// Returns the PQDS datatype <see cref="PQDSMetaDataType"/>
        /// </summary>
        /// <returns>The PQDS Datatype </returns>
        public override PQDSMetaDataType Type()
        {
            return m_expectedDataType;
        }

        #endregion[Methods]

        #region[Statics]

        private static readonly Dictionary<string, PQDSMetaDataType> keyToDataTypeLookup = new Dictionary<string, PQDSMetaDataType>()
        {
            {"DeviceName", PQDSMetaDataType.Text },
            {"DeviceAlias", PQDSMetaDataType.Text },
            {"DeviceLocation", PQDSMetaDataType.Text },
            {"DeviceLocationAlias", PQDSMetaDataType.Text },
            {"DeviceLatitude", PQDSMetaDataType.Text },
            {"DeviceLongitude", PQDSMetaDataType.Text },
            {"Accountname", PQDSMetaDataType.Text },
            {"AccountNameAlias", PQDSMetaDataType.Text },
            {"DeviceDistanceToXFMR", PQDSMetaDataType.Numeric },
            {"DeviceConnectionTypeCode", PQDSMetaDataType.Enumeration },
            {"DeviceOwner", PQDSMetaDataType.Text },
            {"NominalVoltage-LG", PQDSMetaDataType.Numeric },
            {"NominalFrequency", PQDSMetaDataType.Numeric },
            {"UpstreamXFMR-kVA", PQDSMetaDataType.Numeric },
            {"LineLength", PQDSMetaDataType.Numeric },
            {"AssetName", PQDSMetaDataType.Text },
            {"EventGUID", PQDSMetaDataType.AlphaNumeric },
            {"EventID", PQDSMetaDataType.Text },
            {"EventYear", PQDSMetaDataType.Enumeration },
            {"EventMonth", PQDSMetaDataType.Enumeration },
            {"EventDay", PQDSMetaDataType.Enumeration },
            {"EventHour", PQDSMetaDataType.Enumeration },
            {"EventMinute", PQDSMetaDataType.Enumeration },
            {"EventSecond", PQDSMetaDataType.Enumeration },
            {"EventNanoSecond", PQDSMetaDataType.Numeric },
            {"EventDate", PQDSMetaDataType.Text },
            {"EventTime", PQDSMetaDataType.Text },
            {"EventTypeCode", PQDSMetaDataType.Enumeration },
            {"EventFaultTypeCode", PQDSMetaDataType.Enumeration },
            {"EventPeakCurrent", PQDSMetaDataType.Numeric },
            {"EventPeakVoltage", PQDSMetaDataType.Numeric },
            {"EventMaxVA", PQDSMetaDataType.Numeric },
            {"EventMaxVB", PQDSMetaDataType.Numeric },
            {"EventMaxVC", PQDSMetaDataType.Numeric },
            {"EventMinVA", PQDSMetaDataType.Numeric },
            {"EventMinVB", PQDSMetaDataType.Numeric },
            {"EventMinVC", PQDSMetaDataType.Numeric },
            {"EventMaxIA", PQDSMetaDataType.Numeric },
            {"EventMaxIB", PQDSMetaDataType.Numeric },
            {"EventMaxIC", PQDSMetaDataType.Numeric },
            {"EventPreEventCurrent", PQDSMetaDataType.Numeric },
            {"EventPreEventVoltage", PQDSMetaDataType.Numeric },
            {"EventDuration", PQDSMetaDataType.Numeric },
            {"EventFaultI2T", PQDSMetaDataType.Numeric },
            {"DistanceToFault", PQDSMetaDataType.Numeric },
            {"EventCauseCode", PQDSMetaDataType.Enumeration },
            {"WaveformDataType", PQDSMetaDataType.Enumeration },
            {"WaveFormSensitivityCode", PQDSMetaDataType.Enumeration },
            {"WaveFormSensitivityNote", PQDSMetaDataType.Text },
            {"Utility",  PQDSMetaDataType.Text },
            {"ContactEmail",  PQDSMetaDataType.Text }
        };

        private static readonly Dictionary<string, string> keyToUnitLookup = new Dictionary<string, string>()
        {
            {"DeviceName", null },
            {"DeviceAlias", null },
            {"DeviceLocation", null },
            {"DeviceLocationAlias", null },
            {"DeviceLatitude", null },
            {"DeviceLongitude", null },
            {"Accountname", null },
            {"AccountNameAlias", null },
            {"DeviceDistanceToXFMR", "feet" },
            {"DeviceConnectionTypeCode", null },
            {"DeviceOwner", null },
            {"NominalVoltage-LG", "Volts" },
            {"NominalFrequency", "Hz" },
            {"UpstreamXFMR-kVA", "kVA" },
            {"LineLength", "miles" },
            {"AssetName", null },
            {"EventGUID", null },
            {"EventID", null },
            {"EventYear", null },
            {"EventMonth", null },
            {"EventDay", null },
            {"EventHour", null },
            {"EventMinute", null },
            {"EventSecond", null },
            {"EventNanoSecond", null },
            {"EventDate", null },
            {"EventTime", null },
            {"EventTypeCode", null },
            {"EventFaultTypeCode", null },
            {"EventPeakCurrent", "Amps" },
            {"EventPeakVoltage", "Volts" },
            {"EventMaxVA", "Volts" },
            {"EventMaxVB", "Volts" },
            {"EventMaxVC", "Volts" },
            {"EventMinVA", "Volts" },
            {"EventMinVB", "Volts" },
            {"EventMinVC", "Volts" },
            {"EventMaxIA", "Amps" },
            {"EventMaxIB", "Amps" },
            {"EventMaxIC", "Amps" },
            {"EventPreEventCurrent", "Amps" },
            {"EventPreEventVoltage", "Volts" },
            {"EventDuration", "ms" },
            {"EventFaultI2T", "A2s" },
            {"DistanceToFault", "miles" },
            {"EventCauseCode", null },
            {"WaveformDataType", null },
            {"WaveFormSensitivityCode", null },
            {"WaveFormSensitivityNote", null },
            {"Utility", null },
            {"ContactEmail", null }
        };

        private static readonly Dictionary<string, string> keyToNoteLookup = new Dictionary<string, string>()
        {
            {"DeviceName", "Meter or measurement device name" },
            {"DeviceAlias", "Alternate meter or measurement device name" },
            {"DeviceLocation", "Meter or measurment device location name" },
            {"DeviceLocationAlias", "Alternate meter or device location name" },
            {"DeviceLatitude", "Latitude" },
            {"DeviceLongitude", "Longtitude" },
            {"Accountname", "Name of customer or account" },
            {"AccountNameAlias", "Alternate name of customer or account" },
            {"DeviceDistanceToXFMR", "Distance to the upstream transformer" },
            {"DeviceConnectionTypeCode", "PQDS code for meter connection type" },
            {"DeviceOwner", "Utility name" },
            {"NominalVoltage-LG", "Nominal Line to Ground Voltage" },
            {"NominalFrequency", "Nominal System frequency" },
            {"UpstreamXFMR-kVA", "Upstream Transformer size" },
            {"LineLength", "Length of the Line" },
            {"AssetName", "Asset name" },
            {"EventGUID", "Globally Unique Event Identifier" },
            {"EventID", "A user defined Event Name" },
            {"EventYear", "Year" },
            {"EventMonth", "Month" },
            {"EventDay", "Day" },
            {"EventHour", "Hour" },
            {"EventMinute", "Minute" },
            {"EventSecond", "Second" },
            {"EventNanoSecond", "Nanosconds" },
            {"EventDate", "Event Date" },
            {"EventTime", "Event Time" },
            {"EventTypeCode", "PQDS Event Type Code" },
            {"EventFaultTypeCode", "PQDS Fault Type Code" },
            {"EventPeakCurrent", "Peak Current"},
            {"EventPeakVoltage", "Peak Voltage" },
            {"EventMaxVA", "RMS Maximum A Phase Voltage" },
            {"EventMaxVB", "RMS Maximum B Phase Voltage" },
            {"EventMaxVC", "RMS Maximum C Phase Voltage" },
            {"EventMinVA", "RMS Minimum A Phase Voltage" },
            {"EventMinVB", "RMS Minimum B Phase Voltage" },
            {"EventMinVC", "RMS Minimum C Phase Voltage" },
            {"EventMaxIA", "RMS Maximum A Phase Current" },
            {"EventMaxIB", "RMS Maximum B Phase Current" },
            {"EventMaxIC", "RMS Maximum C Phase Current" },
            {"EventPreEventCurrent", "Pre Event Current" },
            {"EventPreEventVoltage", "pre Event Voltage" },
            {"EventDuration", "Event Duration" },
            {"EventFaultI2T", "I2(t) during Fault duration" },
            {"DistanceToFault", "Distance to Fault" },
            {"EventCauseCode", "PQDS Event Cause Code" },
            { "WaveformDataType", "PQDS Data Type Code"},
            {"WaveFormSensitivityCode", "PQDS Data Sensitivity Code" },
            {"WaveFormSensitivityNote", "Notes on the PQDS Data Sensitivity Code" },
            {"Utility", "Utility that Generated this Dataset" },
            {"ContactEmail", "Contact for Utility that Created this Dataset" }
        };

        private static string DataTypeToCSV(PQDSMetaDataType dataType)
        {
            switch (dataType)
            {
                case (PQDSMetaDataType.Text):
                    return "T";
                case (PQDSMetaDataType.Numeric):
                    return "N";
                case (PQDSMetaDataType.Enumeration):
                    return "E";
                case (PQDSMetaDataType.AlphaNumeric):
                    return "A";
                case (PQDSMetaDataType.Binary):
                    return "B";
                default:
                    return "T";
            }
        }


        #endregion[Statics]






    }


}
