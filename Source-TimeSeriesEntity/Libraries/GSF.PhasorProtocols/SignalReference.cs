//******************************************************************************************************
//  SignalReference.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  05/05/2009 - J. Ritchie Carroll
//       Generated original version of source code
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GSF.TimeSeries.Adapters;

namespace GSF.PhasorProtocols
{
    // TODO: The entire signal reference class and its enumerations may/should need to move into Time-Series entity framework...

    #region [ Enumerations ]

    /// <summary>
    /// Fundamental signal kind enumeration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This signal enumeration represents the basic kind of a signal used to suffix a formatted signal reference.
    /// When used in context along with an optional index the fundamental signal enumeration will identify a signal's
    /// location within a frame of data (see <see cref="SignalReference"/>).
    /// </para>
    /// <para>
    /// Contrast this to the <see cref="SignalType"/> enumeration which further defines an
    /// explicit type for a signal (e.g., a "voltage" or "current" type for an angle).
    /// </para>
    /// </remarks>
    [Serializable]
    public enum SignalKind
    {
        /// <summary>
        /// Phase angle.
        /// </summary>
        Angle,
        /// <summary>
        /// Phase magnitude.
        /// </summary>
        Magnitude,
        /// <summary>
        /// Line frequency.
        /// </summary>
        Frequency,
        /// <summary>
        /// Frequency delta over time (dF/dt).
        /// </summary>
        DfDt,
        /// <summary>
        /// Status flags.
        /// </summary>
        Status,
        /// <summary>
        /// Digital value.
        /// </summary>
        Digital,
        /// <summary>
        /// Analog value.
        /// </summary>
        Analog,
        /// <summary>
        /// Calculated value.
        /// </summary>
        Calculation,
        /// <summary>
        /// Statistical value.
        /// </summary>
        Statistic,
        /// <summary>
        /// Alarm value.
        /// </summary>
        Alarm,
        /// <summary>
        /// Undetermined signal type.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Signal type enumeration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The signal type represents the explicit type of a signal that a value represents.
    /// </para>
    /// <para>
    /// Contrast this to the <see cref="SignalKind"/> enumeration which only defines an
    /// abstract type for a signal (e.g., simply a phase or an angle).
    /// </para>
    /// </remarks>
    [Serializable]
    public enum SignalType
    {
        /// <summary>
        /// Current phase magnitude.
        /// </summary>
        IPHM = 1,
        /// <summary>
        /// Current phase angle.
        /// </summary>
        IPHA = 2,
        /// <summary>
        /// Voltage phase magnitude.
        /// </summary>
        VPHM = 3,
        /// <summary>
        /// Voltage phase angle.
        /// </summary>
        VPHA = 4,
        /// <summary>
        /// Frequency.
        /// </summary>
        FREQ = 5,
        /// <summary>
        /// Frequency delta (dF/dt).
        /// </summary>
        DFDT = 6,
        /// <summary>
        /// Analog value.
        /// </summary>
        ALOG = 7,
        /// <summary>
        /// Status flags.
        /// </summary>
        FLAG = 8,
        /// <summary>
        /// Digital value.
        /// </summary>
        DIGI = 9,
        /// <summary>
        /// Calculated value.
        /// </summary>
        CALC = 10,
        /// <summary>
        /// Statistical value.
        /// </summary>
        STAT = 11,
        /// <summary>
        /// Alarm value.
        /// </summary>
        ALRM = 12,
        /// <summary>
        /// Undefined signal.
        /// </summary>
        NONE = -1
    }

    /// <summary>
    /// Defines extension functions related to <see cref="SignalType"/> enumerations.
    /// </summary>
    public static class SignalTypeExtensions
    {
        /// <summary>
        /// Returns display friendly signal type name.
        /// </summary>
        /// <param name="signalType"><see cref="SignalType"/> to return display name for.</param>
        /// <returns>Friendly protocol display name for specified <paramref name="signalType"/>.</returns>
        public static string GetFormattedSignalTypeName(this SignalType signalType)
        {
            switch (signalType)
            {
                case SignalType.IPHM:
                    return "Current phase magnitude";
                case SignalType.IPHA:
                    return "Current phase angle";
                case SignalType.VPHM:
                    return "Voltage phase magnitude";
                case SignalType.VPHA:
                    return "Voltage phase angle";
                case SignalType.FREQ:
                    return "Frequency";
                case SignalType.DFDT:
                    return "Frequency delta (dF/dt)";
                case SignalType.ALOG:
                    return "Analog";
                case SignalType.FLAG:
                    return "Status flags";
                case SignalType.DIGI:
                    return "Digital";
                case SignalType.CALC:
                    return "Calculated";
                case SignalType.NONE:
                    return "Undefined";
                default:
                    return signalType.ToString().ToTitleCase();
            }
        }

        /// <summary>
        /// Parses a <see cref="SignalType"/> value from a <see cref="DataRow"/> for the specified <paramref name="signalTypeColumn"/>.
        /// </summary>
        /// <param name="row"><see cref="DataRow"/> that contains the field to parse as a <see cref="SignalType"/>.</param>
        /// <param name="signalTypeColumn">Name of column that contains the data to parse as a <see cref="SignalType"/>.</param>
        /// <returns>
        /// <see cref="SignalType"/> from <see cref="DataRow"/> if parse succeeds; otherwise an empty <see cref="SignalType"/>.
        /// </returns>
        public static SignalType GetSignalType(this DataRow row, string signalTypeColumn = "SignalType")
        {
            SignalType type = SignalType.NONE;

            if ((object)row != null && row.Table.Columns.Contains(signalTypeColumn))
                Enum.TryParse(row[signalTypeColumn].ToNonNullString("NONE"), true, out type);

            return type;
        }

        /// <summary>
        /// Attempts to lookup meta-data associated with the specified <paramref name="signalID"/>.
        /// </summary>
        /// <param name="adapter">Adapter to get meta-data for.</param>
        /// <param name="signalID">The <see cref="Guid"/> for the signal to look up the meta-data.</param>
        /// <param name="type">The <see cref="SignalType"/> instance to return.</param>
        /// <param name="measurementTable">Measurement table name to search for meta-data.</param>
        /// <param name="signalTypeColumn">Name of column that contains the data to parse as a <see cref="SignalType"/>.</param>
        /// <returns><c>true</c> if meta-data record for <paramref name="signalID"/> was found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="adapter"/> is <c>null</c>.</exception>
        public static bool TryGetSignalType(this IAdapter adapter, Guid signalID, out SignalType type, string measurementTable = "ActiveMeasurements", string signalTypeColumn = "SignalType")
        {
            DataRow row;

            if (adapter.TryGetMetadata(signalID, out row, measurementTable))
            {
                type = row.GetSignalType(signalTypeColumn);

                if (type != SignalType.NONE)
                    return true;
            }

            type = SignalType.NONE;
            return false;
        }

        /// <summary>
        /// Attempts to parse an individual signal ID from the specified <paramref name="parameterName"/> out of the <see cref="IAdapter.ConnectionString"/>
        /// and validate that the returned signal is the specified <see cref="SignalType"/>.
        /// </summary>
        /// <param name="adapter">The <see cref="IAdapter"/> used for source configuration.</param>
        /// <param name="signalType">The desired <see cref="SignalType"/> of the <paramref name="signalID"/>.</param>
        /// <param name="parameterName">Parameter name for the signal ID expected in the <see cref="IAdapter.ConnectionString"/>.</param>
        /// <param name="signalID">The returned <see cref="Guid"/> based signal ID if successfully parsed.</param>
        /// <returns><c>true</c> if signal ID was successfully parsed; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// This function is useful when input or output signals need to be individually specified, e.g., in a calculation, where the signals are
        /// identified as connection string parameters.
        /// </para>
        /// <para>
        /// If <paramref name="parameterName"/> value is a filter expression that returns more than one value, first value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="adapter"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><see cref="IAdapter.Settings"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Could not determine signal type or signal type is not specified <paramref name="signalType"/> for given <paramref name="parameterName"/>.
        /// </exception>
        public static bool TryParseSignalID(this IAdapter adapter, SignalType signalType, string parameterName, out Guid signalID)
        {
            if ((object)adapter == null)
                throw new ArgumentNullException("adapter");

            return signalType.TryParseSignalID(adapter.DataSource, adapter.Settings, parameterName, out signalID);
        }

        /// <summary>
        /// Attempts to parse an individual signal ID from the specified <paramref name="parameterName"/> out of the <see cref="IAdapter.ConnectionString"/>
        /// and validate that the returned signal is the specified <see cref="SignalType"/>.
        /// </summary>
        /// <param name="signalType">The desired <see cref="SignalType"/> of the <paramref name="signalID"/>.</param>
        /// <param name="dataSource">The <see cref="DataSet"/>, if any, used to define data that can be used for the filter expression found in <paramref name="parameterName"/>.</param>
        /// <param name="settings">Connection string settings dictionary.</param>
        /// <param name="parameterName">Parameter name for the signal ID expected in the <see cref="IAdapter.ConnectionString"/>.</param>
        /// <param name="signalID">The returned <see cref="Guid"/> based signal ID if successfully parsed.</param>
        /// <returns><c>true</c> if signal ID was successfully parsed; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// This function is useful when input or output signals need to be individually specified, e.g., in a calculation, where the signals are
        /// identified as connection string parameters.
        /// </para>
        /// <para>
        /// If <paramref name="parameterName"/> value is a filter expression that returns more than one value, first value is returned.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Could not determine signal type or signal type is not specified <paramref name="signalType"/> for given <paramref name="parameterName"/>.
        /// </exception>
        public static bool TryParseSignalID(this SignalType signalType, DataSet dataSource, Dictionary<string, string> settings, string parameterName, out Guid signalID)
        {
            if (AdapterBase.TryParseSignalID(dataSource, settings, parameterName, out signalID))
            {
                DataRow row;

                if (!AdapterBase.TryGetMetadata(dataSource, signalID, out row) || row.GetSignalType() != signalType)
                    throw new InvalidOperationException(string.Format("Could not determine signal type or signal type is not a {0} for specified \"{1}\".", signalType.GetFormattedSignalTypeName(), parameterName));

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to parse signal IDs from the specified <paramref name="parameterName"/> out of the <see cref="IAdapter.ConnectionString"/>
        /// and validate that the returned signals are all the specified <see cref="SignalType"/>.
        /// </summary>
        /// <param name="adapter">The <see cref="IAdapter"/> used for source configuration.</param>
        /// <param name="signalType">The desired <see cref="SignalType"/> of the <paramref name="signalIDs"/>.</param>
        /// <param name="parameterName">Parameter name for the signal ID expected in the <see cref="IAdapter.ConnectionString"/>.</param>
        /// <param name="signalIDs">The returned <see cref="Guid"/> based signal IDs if successfully parsed.</param>
        /// <returns><c>true</c> if any signal IDs were successfully parsed; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="adapter"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException"><see cref="IAdapter.Settings"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Could not determine signal type or signal type is not specified <paramref name="signalType"/> for given <paramref name="parameterName"/>.
        /// </exception>
        public static bool TryParseSignalIDs(this IAdapter adapter, SignalType signalType, string parameterName, out Guid[] signalIDs)
        {
            if ((object)adapter == null)
                throw new ArgumentNullException("adapter");

            return signalType.TryParseSignalIDs(adapter.DataSource, adapter.Settings, parameterName, out signalIDs);
        }

        /// <summary>
        /// Attempts to parse signal IDs from the specified <paramref name="parameterName"/> out of the <see cref="IAdapter.ConnectionString"/>
        /// and validate that the returned signals are all the specified <see cref="SignalType"/>.
        /// </summary>
        /// <param name="signalType">The desired <see cref="SignalType"/> of the <paramref name="signalIDs"/>.</param>
        /// <param name="dataSource">The <see cref="DataSet"/>, if any, used to define data that can be used for the filter expression found in <paramref name="parameterName"/>.</param>
        /// <param name="settings">Connection string settings dictionary.</param>
        /// <param name="parameterName">Parameter name for the signal ID expected in the <see cref="IAdapter.ConnectionString"/>.</param>
        /// <param name="signalIDs">The returned <see cref="Guid"/> based signal IDs if successfully parsed.</param>
        /// <returns><c>true</c> if any signal IDs were successfully parsed; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="settings"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">
        /// Could not determine signal type or signal type is not specified <paramref name="signalType"/> for given <paramref name="parameterName"/>.
        /// </exception>
        public static bool TryParseSignalIDs(this SignalType signalType, DataSet dataSource, Dictionary<string, string> settings, string parameterName, out Guid[] signalIDs)
        {
            if (AdapterBase.TryParseSignalIDs(dataSource, settings, parameterName, out signalIDs))
            {
                DataRow row;

                if (!signalIDs.All(id => AdapterBase.TryGetMetadata(dataSource, id, out row) && row.GetSignalType() == signalType))
                    throw new InvalidOperationException(string.Format("Could not determine signal type or signal type is not a {0} for all signals specified for \"{1}\".", signalType.GetFormattedSignalTypeName(), parameterName));

                return true;
            }

            return false;
        }
    }

    #endregion

    /// <summary>
    /// Represents a signal that can be referenced by its constituent components.
    /// </summary>
    public struct SignalReference : IEquatable<SignalReference>, IComparable<SignalReference>
    {
        #region [ Members ]

        // Fields

        /// <summary>
        /// Gets or sets the acronym of this <see cref="SignalReference"/>.
        /// </summary>
        public string Acronym;

        /// <summary>
        /// Gets or sets the signal index of this <see cref="SignalReference"/>.
        /// </summary>
        public int Index;

        /// <summary>
        /// Gets or sets the <see cref="SignalKind"/> of this <see cref="SignalReference"/>.
        /// </summary>
        public SignalKind Kind;

        /// <summary>
        /// Gets or sets the cell index of this <see cref="SignalReference"/>.
        /// </summary>
        public int CellIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SignalReference"/>.
        /// </summary>
        /// <param name="signal"><see cref="string"/> representation of this <see cref="SignalReference"/>.</param>
        public SignalReference(string signal)
        {
            // Signal reference may contain multiple dashes, we're interested in the last one
            int splitIndex = signal.LastIndexOf('-');

            // Assign default values to fields
            Index = 0;
            CellIndex = 0;

            if (splitIndex > -1)
            {
                string signalType = signal.Substring(splitIndex + 1).Trim().ToUpper();
                Acronym = signal.Substring(0, splitIndex).Trim().ToUpper();

                // If the length of the signal type acronym is greater than 2, then this
                // is an indexed signal type (e.g., CORDOVA-PA2)
                if (signalType.Length > 2)
                {
                    Kind = ParseSignalKind(signalType.Substring(0, 2));

                    if (Kind != SignalKind.Unknown)
                        Index = int.Parse(signalType.Substring(2));
                }
                else
                    Kind = ParseSignalKind(signalType);
            }
            else
            {
                // This represents an error - best we can do is assume entire string is the acronym
                Acronym = signal.Trim().ToUpper();
                Kind = SignalKind.Unknown;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current <see cref="SignalReference"/>.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents the current <see cref="SignalReference"/>.</returns>
        public override string ToString()
        {
            return ToString(Acronym, Kind, Index);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="SignalReference"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return ToString(Acronym, Kind, Index).GetHashCode();
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Object is not a <see cref="SignalReference"/>.</exception>
        public override bool Equals(object obj)
        {
            if (obj is SignalReference)
                return Equals((SignalReference)obj);

            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.</returns>
        public bool Equals(SignalReference other)
        {
            return (string.Compare(Acronym, other.Acronym, true) == 0 && Kind == other.Kind && Index == other.Index);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">Another <see cref="SignalReference"/> to compare with this <see cref="SignalReference"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(SignalReference other)
        {
            int acronymCompare = string.Compare(Acronym, other.Acronym, true);

            if (acronymCompare == 0)
            {
                int signalTypeCompare = Kind < other.Kind ? -1 : (Kind > other.Kind ? 1 : 0);

                if (signalTypeCompare == 0)
                    return Index.CompareTo(other.Index);

                return signalTypeCompare;
            }

            return acronymCompare;
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this <see cref="SignalReference"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException">Object is not a <see cref="SignalReference"/>.</exception>
        public int CompareTo(object obj)
        {
            if (obj is SignalReference)
                return CompareTo((SignalReference)obj);

            throw new ArgumentException("Object is not a SignalReference");
        }

        #endregion

        #region [ Operators ]

        /// <summary>
        /// Compares two <see cref="SignalReference"/> values for equality.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator ==(SignalReference signal1, SignalReference signal2)
        {
            return signal1.Equals(signal2);
        }

        /// <summary>
        /// Compares two <see cref="SignalReference"/> values for inequality.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator !=(SignalReference signal1, SignalReference signal2)
        {
            return !signal1.Equals(signal2);
        }

        /// <summary>
        /// Returns true if left <see cref="SignalReference"/> value is greater than right <see cref="SignalReference"/> value.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >(SignalReference signal1, SignalReference signal2)
        {
            return signal1.CompareTo(signal2) > 0;
        }

        /// <summary>
        /// Returns true if left <see cref="SignalReference"/> value is greater than or equal to right <see cref="SignalReference"/> value.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator >=(SignalReference signal1, SignalReference signal2)
        {
            return signal1.CompareTo(signal2) >= 0;
        }

        /// <summary>
        /// Returns true if left <see cref="SignalReference"/> value is less than right <see cref="SignalReference"/> value.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <(SignalReference signal1, SignalReference signal2)
        {
            return signal1.CompareTo(signal2) < 0;
        }

        /// <summary>
        /// Returns true if left <see cref="SignalReference"/> value is less than or equal to right <see cref="SignalReference"/> value.
        /// </summary>
        /// <param name="signal1">A <see cref="SignalReference"/> left hand operand.</param>
        /// <param name="signal2">A <see cref="SignalReference"/> right hand operand.</param>
        /// <returns>A boolean representing the result.</returns>
        public static bool operator <=(SignalReference signal1, SignalReference signal2)
        {
            return signal1.CompareTo(signal2) <= 0;
        }

        #endregion

        #region [ Static ]

        // Static Methods

        /// <summary>
        /// Gets the <see cref="SignalKind"/> for the specified <paramref name="acronym"/>.
        /// </summary>
        /// <param name="acronym">Acronym of the desired <see cref="SignalKind"/>.</param>
        /// <returns>The <see cref="SignalKind"/> for the specified <paramref name="acronym"/>.</returns>
        public static SignalKind ParseSignalKind(string acronym)
        {
            switch (acronym)
            {
                case "PA": // Phase Angle
                    return SignalKind.Angle;
                case "PM": // Phase Magnitude
                    return SignalKind.Magnitude;
                case "FQ": // Frequency
                    return SignalKind.Frequency;
                case "DF": // dF/dt
                    return SignalKind.DfDt;
                case "SF": // Status Flags
                    return SignalKind.Status;
                case "DV": // Digital Value
                    return SignalKind.Digital;
                case "AV": // Analog Value
                    return SignalKind.Analog;
                case "CV": // Calculated Value
                    return SignalKind.Calculation;
                case "ST": // Statistical Value
                    return SignalKind.Statistic;
                case "AL": // Alarm Value
                    return SignalKind.Alarm;
                default:
                    return SignalKind.Unknown;
            }
        }

        /// <summary>
        /// Gets the acronym for the specified <see cref="SignalKind"/>.
        /// </summary>
        /// <param name="signal"><see cref="SignalKind"/> to convert to an acronym.</param>
        /// <returns>The acronym for the specified <see cref="SignalKind"/>.</returns>
        public static string GetSignalKindAcronym(SignalKind signal)
        {
            switch (signal)
            {
                case SignalKind.Angle:
                    return "PA"; // Phase Angle
                case SignalKind.Magnitude:
                    return "PM"; // Phase Magnitude
                case SignalKind.Frequency:
                    return "FQ"; // Frequency
                case SignalKind.DfDt:
                    return "DF"; // dF/dt
                case SignalKind.Status:
                    return "SF"; // Status Flags
                case SignalKind.Digital:
                    return "DV"; // Digital Value
                case SignalKind.Analog:
                    return "AV"; // Analog Value
                case SignalKind.Calculation:
                    return "CV"; // Calculated Value
                case SignalKind.Statistic:
                    return "ST"; // Statistical Value
                case SignalKind.Alarm:
                    return "AL"; // Alarm Value
                default:
                    return "??";
            }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the specified <paramref name="acronym"/> and <see cref="SignalKind"/>.
        /// </summary>
        /// <param name="acronym">Acronym portion of the desired <see cref="string"/> representation.</param>
        /// <param name="type"><see cref="SignalKind"/> portion of the desired <see cref="string"/> representation.</param>
        /// <returns>A <see cref="string"/> that represents the specified <paramref name="acronym"/> and <see cref="SignalKind"/>.</returns>
        public static string ToString(string acronym, SignalKind type)
        {
            return ToString(acronym, type, 0);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the specified <paramref name="acronym"/>, <see cref="SignalKind"/> and <paramref name="index"/>.
        /// </summary>
        /// <param name="acronym">Acronym portion of the desired <see cref="string"/> representation.</param>
        /// <param name="type"><see cref="SignalKind"/> portion of the desired <see cref="string"/> representation.</param>
        /// <param name="index">Index of <see cref="SignalKind"/> portion of the desired <see cref="string"/> representation.</param>
        /// <returns>A <see cref="string"/> that represents the specified <paramref name="acronym"/>, <see cref="SignalKind"/> and <paramref name="index"/>.</returns>
        public static string ToString(string acronym, SignalKind type, int index)
        {
            if (index > 0)
                return string.Format("{0}-{1}{2}", acronym, GetSignalKindAcronym(type), index);

            return string.Format("{0}-{1}", acronym, GetSignalKindAcronym(type));
        }

        #endregion
    }
}