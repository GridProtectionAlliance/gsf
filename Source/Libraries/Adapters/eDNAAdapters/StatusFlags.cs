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
using GSF.TimeSeries;

namespace eDNAAdapters
{
    #region [ Enumerations ]

    /// <summary>
    /// Defines eDNA data types.
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Analog data type.
        /// </summary>
        Analog,
        /// <summary>
        /// Digital data type.
        /// </summary>
        Digital
    }

    /// <summary>
    /// eDNA point status flags.
    /// </summary>
    [Flags]
    public enum StatusFlags : short
    {
        /// <summary>
        /// Evaluate status.
        /// </summary>
        EVAL = -1,
        /// <summary>
        /// Not yet initialized.
        /// </summary>
        RTSTAT_UNUSED = 0,
        /// <summary>
        /// Has been initialized.
        /// </summary>
        RTSTAT_INITED = 1,
        /// <summary>
        /// Has been updated.
        /// </summary>
        RTSTAT_UPDATED = 2,
        /// <summary>
        /// Data is bad.
        /// </summary>
        RTSTAT_UNRELIABLE = 4,
        /// <summary>
        /// Sensor bad.
        /// </summary>
        RTSTAT_OOR = 8,
        /// <summary>
        /// Low-low alarm.
        /// </summary>
        RTSTAT_LL_ALARM = 16,
        /// <summary>
        /// Low alarm.
        /// </summary>
        RTSTAT_L_ALARM = 32,
        /// <summary>
        /// High alarm.
        /// </summary>
        RTSTAT_H_ALARM = 64,
        /// <summary>
        /// High-high alarm.
        /// </summary>
        RTSTAT_HH_ALARM = 128,
        /// <summary>
        /// Manual input.
        /// </summary>
        RTSTAT_MANUAL = 256,
        /// <summary>
        /// Reset not acknowledged.
        /// </summary>
        RTSTAT_UNACKRST = 512,
        /// <summary>
        /// Alarm not acknowledged.
        /// </summary>
        RTSTAT_UNACKAL = 1024,
        /// <summary>
        /// Tickle flags for history repeat.
        /// </summary>
        RTSTAT_REPEAT = 2048,
        /// <summary>
        /// Digital point type.
        /// </summary>
        RTSTAT_DIGITAL_IN = 4096,
        /// <summary>
        /// Analog point type.
        /// </summary>
        RTSTAT_ANALOG_OUT = 8192,
        /// <summary>
        /// Digital point type.
        /// </summary>
        RTSTAT_DIGITAL_OUT = 12288,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality.
        /// </summary>
        ANALOG_OK = 3,
        /// <summary>
        /// Analog Point Initialized, Received Value, Unreliable Quality,
        /// </summary>
        ANALOG_UNRELIABLE = 7,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes below Low Warning Threshold.
        /// </summary>
        ANALOG_LOW_WARNING = 35,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes below Low Alarm Threshold.
        /// </summary>
        ANALOG_LOW_ALARM = 51,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes below Low Out of Range Threshold.
        /// </summary>
        ANALOG_LOW_OUT_OF_RANGE = 59,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes above High Warning Threshold.
        /// </summary>
        ANALOG_HIGH_WARNING = 67,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes above High Alarm Threshold.
        /// </summary>
        ANALOG_HIGH_ALARM = 195,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes above High Out of Range Threshold.
        /// </summary>
        ANALOG_HIGH_OUT_OF_RANGE = 203,
        /// <summary>
        /// Analog Point Initialized, Received Value, Manually Input into eDNA, Reliable Quality.
        /// </summary>
        ANALOG_OK_MANUAL = 259,
        /// <summary>
        /// Analog Point Initialized, Received Value, Manually Input into eDNA, Unreliable Quality.
        /// </summary>
        ANALOG_UNRELIABLE_MANUAL = 263,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Reset Unacknowledged Alarm.
        /// </summary>
        ANALOG_OK_UNACK_ALARM = 515,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes below Low Warning Threshold, Unacknowledged Alarm.
        /// </summary>
        ANALOG_LOW_WARNING_UNACK_ALARM = 1059,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes below Low Alarm Threshold, Unacknowledged Alarm.
        /// </summary>
        ANALOG_LOW_ALARM_UNACK_ALARM = 1075,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes below Low Out of Range Threshold, Unacknowledged Alarm.
        /// </summary>
        ANALOG_LOW_OUT_OF_RANGE_UNACK_ALARM = 1083,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes above High Warning Threshold, Unacknowledged Alarm.
        /// </summary>
        ANALOG_HIGH_WARNING_UNACK_ALARM = 1091,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes above High Alarm Threshold, Unacknowledged Alarm.
        /// </summary>
        ANALOG_HIGH_ALARM_UNACK_ALARM = 1219,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Value goes above High Out of Range Threshold, Unacknowledged Alarm.
        /// </summary>
        ANALOG_HIGH_OUT_OF_RANGE_UNACK_ALARM = 1227,
        /// <summary>
        /// Analog Point Initialized, Received Value, Reliable Quality, Repeat History Value (Forced).
        /// </summary>
        ANALOG_OK_FORCED = 2051,
        /// <summary>
        /// Digital Point Initialized, not received a value.
        /// </summary>
        DIGITAL_NOT_UPDATED = 4097,
        /// <summary>
        /// Digital Point Initialized, Received Value, Reliable Quality, Value = 0.
        /// </summary>
        DIGITAL_OK_NOT_SET = 4099,
        /// <summary>
        /// Digital Point Initialized, Received Value, Reliable Quality, Value = 1.
        /// </summary>
        DIGITAL_OK_SET = 4163,
        /// <summary>
        /// Digital Point Initialized, Received Value, Unreliable Quality, Value = 0.
        /// </summary>
        DIGITAL_UNRELIABLE_NOT_SET = 4103,
        /// <summary>
        /// Digital Point Initialized, Received Value, Unreliable Quality, Value = 1.
        /// </summary>
        DIGITAL_UNRELIABLE_SET = 4167,
        /// <summary>
        /// Digital Point Initialized, Received Value, Reliable Quality, Value = 0, Repeat History Value (Forced).
        /// </summary>
        DIGITAL_OK_NOT_SET_FORCED = 6147,
        /// <summary>
        /// Digital Point Initialized, Received Value, Reliable Quality, Value = 1, Repeat History Value (Forced).
        /// </summary>
        DIGITAL_OK_SET_FORCED = 6211,
        /// <summary>
        /// Digital Point Initialized, Received Value, Reliable Quality, Value = 0, Warning Flag Set.
        /// </summary>
        DIGITAL_WARNING_NOT_SET = 4227,
        /// <summary>
        /// Digital Point Initialized, Received Value, Reliable Quality, Value = 1, Warning Flag Set.
        /// </summary>
        DIGITAL_WARNING_SET = 4291,
        /// <summary>
        /// Digital Point Initialized, Received Value, Reliable Quality, Value = 0, Chattering Flag Set.
        /// </summary>
        DIGITAL_CHATTERING_NOT_SET = 4115,
        /// <summary>
        /// Digital Point Initialized, Received Value, Reliable Quality, Value = 1, Chattering Flag Set.
        /// </summary>
        DIGITAL_CHATTERING_SET = 4179
    }

    #endregion

    /// <summary>
    /// Defines static methods for mapping between eDNA status flags and full measurement states.
    /// </summary>
    public static class MeasurementStateMappingExtensions
    {
        /// <summary>
        /// Maps <see cref="MeasurementStateFlags"/> to eDNA status (short value).
        /// </summary>
        /// <param name="stateFlags">Flags to map.</param>
        /// <param name="type">Data type of measurement.</param>
        /// <param name="value">Data value, determines if digital value is set.</param>
        /// <returns>eDNA status mapped from <see cref="MeasurementStateFlags"/>.</returns>
        public static short MapToStatus(this MeasurementStateFlags stateFlags, DataType type, double value)
        {
            return (short)stateFlags.MapToStatusFlags(type, value);
        }

        /// <summary>
        /// Maps <see cref="MeasurementStateFlags"/> to <see cref="StatusFlags"/>.
        /// </summary>
        /// <param name="stateFlags">Flags to map.</param>
        /// <param name="type">Data type of measurement.</param>
        /// <param name="value">Data value, determines if digital value is set.</param>
        /// <returns><see cref="StatusFlags"/> mapped from <see cref="MeasurementStateFlags"/>.</returns>
        public static StatusFlags MapToStatusFlags(this MeasurementStateFlags stateFlags, DataType type, double value)
        {
            StatusFlags status = StatusFlags.RTSTAT_UNUSED;

            if ((stateFlags & MeasurementStateFlags.ReceivedAsBad) > 0)
                status |= StatusFlags.RTSTAT_UNRELIABLE;

            if ((stateFlags & MeasurementStateFlags.BadTime) > 0 ||
                (stateFlags & MeasurementStateFlags.SuspectTime) > 0 ||
                (stateFlags & MeasurementStateFlags.LateTimeAlarm) > 0 ||
                (stateFlags & MeasurementStateFlags.FutureTimeAlarm) > 0 ||
                (stateFlags & MeasurementStateFlags.SystemError) > 0 ||
                (stateFlags & MeasurementStateFlags.SystemWarning) > 0)
                    status |= StatusFlags.RTSTAT_OOR;

            if (type == DataType.Analog)
            {
                // Point is analog
                if (stateFlags == MeasurementStateFlags.Normal)
                {
                    status = StatusFlags.ANALOG_OK;
                }
                else
                {
                    if ((stateFlags & MeasurementStateFlags.BadData) > 0 ||
                        (stateFlags & MeasurementStateFlags.SuspectData) > 0 ||
                        (stateFlags & MeasurementStateFlags.FlatlineAlarm) > 0 ||
                        (stateFlags & MeasurementStateFlags.ComparisonAlarm) > 0 ||
                        (stateFlags & MeasurementStateFlags.ROCAlarm) > 0 ||
                        (stateFlags & MeasurementStateFlags.CalculationError) > 0 ||
                        (stateFlags & MeasurementStateFlags.CalculationWarning) > 0 ||
                        (stateFlags & MeasurementStateFlags.MeasurementError) > 0)
                            status |= StatusFlags.ANALOG_UNRELIABLE;

                    if ((stateFlags & MeasurementStateFlags.OverRangeError) > 0)
                        status |= StatusFlags.ANALOG_HIGH_OUT_OF_RANGE;

                    if ((stateFlags & MeasurementStateFlags.UnderRangeError) > 0)
                        status |= StatusFlags.ANALOG_LOW_OUT_OF_RANGE;

                    if ((stateFlags & MeasurementStateFlags.AlarmHigh) > 0)
                        status |= StatusFlags.ANALOG_HIGH_ALARM;

                    if ((stateFlags & MeasurementStateFlags.AlarmLow) > 0)
                        status |= StatusFlags.ANALOG_LOW_ALARM;

                    if ((stateFlags & MeasurementStateFlags.WarningHigh) > 0)
                        status |= StatusFlags.ANALOG_HIGH_WARNING;

                    if ((stateFlags & MeasurementStateFlags.WarningLow) > 0)
                        status |= StatusFlags.ANALOG_LOW_WARNING;
                }
            }
            else if (type == DataType.Digital)
            {
                // Point is digital
                if (value != 0.0D)
                {
                    if (stateFlags == MeasurementStateFlags.Normal)
                    {
                        status = StatusFlags.DIGITAL_OK_SET;
                    }
                    else
                    {
                        if ((stateFlags & MeasurementStateFlags.BadData) > 0 ||
                            (stateFlags & MeasurementStateFlags.SuspectData) > 0 ||
                            (stateFlags & MeasurementStateFlags.FlatlineAlarm) > 0 ||
                            (stateFlags & MeasurementStateFlags.ComparisonAlarm) > 0 ||
                            (stateFlags & MeasurementStateFlags.ROCAlarm) > 0 ||
                            (stateFlags & MeasurementStateFlags.CalculationError) > 0 ||
                            (stateFlags & MeasurementStateFlags.CalculationWarning) > 0 ||
                            (stateFlags & MeasurementStateFlags.MeasurementError) > 0)
                                status |= StatusFlags.DIGITAL_UNRELIABLE_SET;

                        if ((stateFlags & MeasurementStateFlags.OverRangeError) > 0 ||
                            (stateFlags & MeasurementStateFlags.UnderRangeError) > 0 ||
                            (stateFlags & MeasurementStateFlags.AlarmHigh) > 0 ||
                            (stateFlags & MeasurementStateFlags.AlarmLow) > 0 ||
                            (stateFlags & MeasurementStateFlags.WarningHigh) > 0 ||
                            (stateFlags & MeasurementStateFlags.WarningLow) > 0)
                                status |= StatusFlags.DIGITAL_WARNING_SET;
                    }
                }
                else
                {
                    if (stateFlags == MeasurementStateFlags.Normal)
                    {
                        status = StatusFlags.DIGITAL_OK_NOT_SET;
                    }
                    else
                    {
                        if ((stateFlags & MeasurementStateFlags.BadData) > 0 ||
                            (stateFlags & MeasurementStateFlags.SuspectData) > 0 ||
                            (stateFlags & MeasurementStateFlags.FlatlineAlarm) > 0 ||
                            (stateFlags & MeasurementStateFlags.ComparisonAlarm) > 0 ||
                            (stateFlags & MeasurementStateFlags.ROCAlarm) > 0 ||
                            (stateFlags & MeasurementStateFlags.CalculationError) > 0 ||
                            (stateFlags & MeasurementStateFlags.CalculationWarning) > 0 ||
                            (stateFlags & MeasurementStateFlags.MeasurementError) > 0)
                                status |= StatusFlags.DIGITAL_UNRELIABLE_NOT_SET;

                        if ((stateFlags & MeasurementStateFlags.OverRangeError) > 0 ||
                            (stateFlags & MeasurementStateFlags.UnderRangeError) > 0 ||
                            (stateFlags & MeasurementStateFlags.AlarmHigh) > 0 ||
                            (stateFlags & MeasurementStateFlags.AlarmLow) > 0 ||
                            (stateFlags & MeasurementStateFlags.WarningHigh) > 0 ||
                            (stateFlags & MeasurementStateFlags.WarningLow) > 0)
                                status |= StatusFlags.DIGITAL_WARNING_NOT_SET;
                    }
                }
            }

            return status;
        }
    }
}
