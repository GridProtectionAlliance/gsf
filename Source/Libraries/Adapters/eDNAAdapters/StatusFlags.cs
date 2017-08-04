//******************************************************************************************************
//  StatusFlags.cs - Gbtc
//
//  Copyright © 2017, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  03/14/2017 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using GSF;
using GSF.TimeSeries;

namespace eDNAAdapters
{
    /// <summary>
    /// eDNA point status flags.
    /// </summary>
    [Flags]
    public enum StatusFlags : short
    {
        /// <summary>
        /// A data range flag was set.
        /// </summary>
        DataRange = (byte)Bits.Bit00,
        /// <summary>
        /// A data quality flag was set.
        /// </summary>
        DataQuality = (byte)Bits.Bit01,
        /// <summary>
        /// A time quality flag was set.
        /// </summary>
        TimeQuality = (byte)Bits.Bit02,
        /// <summary>
        /// A system flag was set.
        /// </summary>
        SystemIssue = (byte)Bits.Bit03,
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (byte)Bits.Nil
    }

    /// <summary>
    /// Defines static methods for mapping between eDNA status flags and full measurement states.
    /// </summary>
    public static class MeasurementStateMappingExtensions
    {
        private const MeasurementStateFlags DataRangeMask = MeasurementStateFlags.OverRangeError | MeasurementStateFlags.UnderRangeError | MeasurementStateFlags.AlarmHigh | MeasurementStateFlags.AlarmLow | MeasurementStateFlags.WarningHigh | MeasurementStateFlags.WarningLow;
        private const MeasurementStateFlags DataQualityMask = MeasurementStateFlags.BadData | MeasurementStateFlags.SuspectData | MeasurementStateFlags.FlatlineAlarm | MeasurementStateFlags.ComparisonAlarm | MeasurementStateFlags.ROCAlarm | MeasurementStateFlags.ReceivedAsBad | MeasurementStateFlags.CalculationError | MeasurementStateFlags.CalculationWarning | MeasurementStateFlags.ReservedQualityFlag;
        private const MeasurementStateFlags TimeQualityMask = MeasurementStateFlags.BadTime | MeasurementStateFlags.SuspectTime | MeasurementStateFlags.LateTimeAlarm | MeasurementStateFlags.FutureTimeAlarm | MeasurementStateFlags.UpSampled | MeasurementStateFlags.DownSampled | MeasurementStateFlags.ReservedTimeFlag;
        private const MeasurementStateFlags SystemIssueMask = MeasurementStateFlags.SystemError | MeasurementStateFlags.SystemWarning | MeasurementStateFlags.MeasurementError;

        /// <summary>
        /// Maps <see cref="MeasurementStateFlags"/> to eDNA status (short value).
        /// </summary>
        /// <param name="stateFlags">Flags to map.</param>
        /// <returns>eDNA status mapped from <see cref="MeasurementStateFlags"/>.</returns>
        public static short MapToStatus(this MeasurementStateFlags stateFlags)
        {
            return (short)stateFlags.MapToStatusFlags();
        }

        /// <summary>
        /// Maps <see cref="MeasurementStateFlags"/> to <see cref="StatusFlags"/>.
        /// </summary>
        /// <param name="stateFlags">Flags to map.</param>
        /// <returns><see cref="StatusFlags"/> mapped from <see cref="MeasurementStateFlags"/>.</returns>
        public static StatusFlags MapToStatusFlags(this MeasurementStateFlags stateFlags)
        {
            StatusFlags mappedStateFlags = StatusFlags.NoFlags;

            if ((stateFlags & DataRangeMask) > 0)
                mappedStateFlags |= StatusFlags.DataRange;

            if ((stateFlags & DataQualityMask) > 0)
                mappedStateFlags |= StatusFlags.DataQuality;

            if ((stateFlags & TimeQualityMask) > 0)
                mappedStateFlags |= StatusFlags.TimeQuality;

            if ((stateFlags & SystemIssueMask) > 0)
                mappedStateFlags |= StatusFlags.SystemIssue;

            return mappedStateFlags;
        }

        /// <summary>
        /// Maps <see cref="StatusFlags"/> to <see cref="MeasurementStateFlags"/>.
        /// </summary>
        /// <param name="statusFlags">Flags to map.</param>
        /// <returns><see cref="MeasurementStateFlags"/> mapped from <see cref="StatusFlags"/>.</returns>
        public static MeasurementStateFlags MapToFullFlags(this StatusFlags statusFlags)
        {
            MeasurementStateFlags mappedStateFlags = MeasurementStateFlags.Normal;

            if ((statusFlags & StatusFlags.DataRange) > 0)
                mappedStateFlags |= DataRangeMask;

            if ((statusFlags & StatusFlags.DataQuality) > 0)
                mappedStateFlags |= DataQualityMask;

            if ((statusFlags & StatusFlags.TimeQuality) > 0)
                mappedStateFlags |= TimeQualityMask;

            if ((statusFlags & StatusFlags.SystemIssue) > 0)
                mappedStateFlags |= SystemIssueMask;

            return mappedStateFlags;
        }
    }
}
