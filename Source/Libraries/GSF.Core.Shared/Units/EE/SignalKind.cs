//******************************************************************************************************
//  SignalKind.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/05/2009 - J. Ritchie Carroll
//       Generated original version of source code
//
//******************************************************************************************************

using System;
using System.Diagnostics.CodeAnalysis;

namespace GSF.Units.EE
{
    /// <summary>
    /// Fundamental signal type enumeration for common EE measurements that represents a kind of signal, not an explicit type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This enumeration represents the basic type of a signal used to suffix a formatted signal
    /// reference. When used in context along with an optional index the fundamental signal type
    /// will identify a signal's location within a frame of data (see <see cref="SignalReference"/>).
    /// </para>
    /// <para>
    /// Contrast this to the <see cref="SignalType"/> enumeration which further defines an explicit
    /// type for a signal (e.g., a voltage or current type for an angle).
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
        /// Quality flags.
        /// </summary>
        Quality,
        /// <summary>
        /// Undetermined signal type.
        /// </summary>
        Unknown
    }

    /// <summary>
    /// Defines extension functions for the <see cref="SignalKind"/> enumeration.
    /// </summary>
    public static class SignalKindExtensions
    {
        /// <summary>
        /// Gets the acronym for the specified <see cref="SignalKind"/>.
        /// </summary>
        /// <param name="signal"><see cref="SignalKind"/> to convert to an acronym.</param>
        /// <returns>The acronym for the specified <see cref="SignalKind"/>.</returns>
        public static string GetAcronym(this SignalKind signal)
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
                case SignalKind.Quality:
                    return "QF"; // Quality Flags
                default:
                    return "??";
            }
        }

        /// <summary>
        /// Gets the <see cref="SignalKind"/> for the specified <paramref name="acronym"/>.
        /// </summary>
        /// <param name="acronym">Acronym of the desired <see cref="SignalKind"/>.</param>
        /// <returns>The <see cref="SignalKind"/> for the specified <paramref name="acronym"/>.</returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public static SignalKind ParseSignalKind(this string acronym)
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
                case "QF": // Quality Flags
                    return SignalKind.Quality;
                default:
                    return SignalKind.Unknown;
            }
        }
    }
}
