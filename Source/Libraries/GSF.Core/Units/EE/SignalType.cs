//******************************************************************************************************
//  SignalType.cs - Gbtc
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

namespace GSF.Units.EE
{
    /// <summary>
    /// Fundamental signal type enumeration for common EE measurements that represents an explicit type of signal.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This enumeration represents the explicit type of a signal that a value represents.
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
        /// Digital values.
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
        /// Quality flags.
        /// </summary>
        QUAL = 13,
        /// <summary>
        /// Undefined signal.
        /// </summary>
        NONE = -1
    }

    /// <summary>
    /// Defines extension functions for the <see cref="SignalType"/> enumeration.
    /// </summary>
    public static class SignalTypeExtensions
    {
        /// <summary>
        /// Returns display friendly signal type name.
        /// </summary>
        /// <param name="signalType"><see cref="SignalType"/> to return display name for.</param>
        /// <returns>Friendly protocol display name for specified <paramref name="signalType"/>.</returns>
        public static string GetFormattedName(this SignalType signalType)
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
                    return "Digital Values";
                case SignalType.CALC:
                    return "Calculated Value";
                case SignalType.STAT:
                    return "Statistic";
                case SignalType.ALRM:
                    return "Alarm";
                case SignalType.QUAL:
                    return "Quality Flags";
                case SignalType.NONE:
                    return "Undefined";
                default:
                    return ((Enum)signalType).GetFormattedName();
            }
        }
    }
}
