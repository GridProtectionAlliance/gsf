//******************************************************************************************************
//  CompactMeasurement.cs - Gbtc
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
//  08/23/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  05/15/2011 - J. Ritchie Carroll
//       Added runtime size optimizations.
//  06/07/2011 - J. Ritchie Carroll
//       Implemented initialize bug fix as found and proposed by Luc Cezard.
//
//******************************************************************************************************

using System;
using TVA;
using TVA.Parsing;

namespace TimeSeriesFramework.Transport
{
    #region [ Enumerations ]

    /// <summary>
    /// <see cref="CompactMeasurement"/> state flags.
    /// </summary>
    [Flags]
    internal enum CompactMeasurementStateFlags : byte
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
        /// Time quality flag was set.
        /// </summary>
        TimeQuality = (byte)Bits.Bit02,
        /// <summary>
        /// System flag was set.
        /// </summary>
        SystemIssue = (byte)Bits.Bit03,
        /// <summary>
        /// Calculated bit was set.
        /// </summary>
        CalculatedValue = (byte)Bits.Bit04,
        /// <summary>
        /// Discarded bit was set.
        /// </summary>
        DiscardedValue = (byte)Bits.Bit05,
        /// <summary>
        /// A user flag was set.
        /// </summary>
        UserFlag = (byte)Bits.Bit06,
        /// <summary>
        /// Use even time index when set; odd time index when not set.
        /// </summary>
        TimeIndex = (byte)Bits.Bit07,
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (byte)Bits.Nil
    }

    /// <summary>
    /// Defines static methods for mapping between compact and full measurement states.
    /// </summary>
    internal static class MeasurementStateMappingExtensions
    {
        private const MeasurementStateFlags DataRangeMask = MeasurementStateFlags.OverRangeError | MeasurementStateFlags.UnderRangeError | MeasurementStateFlags.AlarmHigh | MeasurementStateFlags.AlarmLow | MeasurementStateFlags.WarningHigh | MeasurementStateFlags.WarningLow;
        private const MeasurementStateFlags DataQualityMask = MeasurementStateFlags.BadData | MeasurementStateFlags.SuspectData | MeasurementStateFlags.FlatlineAlarm | MeasurementStateFlags.ComparisonAlarm | MeasurementStateFlags.ROCAlarm | MeasurementStateFlags.ReceivedAsBad | MeasurementStateFlags.CalculationError | MeasurementStateFlags.CalculationWarning | MeasurementStateFlags.ReservedQualityFlag;
        private const MeasurementStateFlags TimeQualityMask = MeasurementStateFlags.BadTime | MeasurementStateFlags.SuspectTime | MeasurementStateFlags.LateTimeAlarm | MeasurementStateFlags.FutureTimeAlarm | MeasurementStateFlags.UpSampled | MeasurementStateFlags.DownSampled | MeasurementStateFlags.ReservedTimeFlag;
        private const MeasurementStateFlags SystemIssueMask = MeasurementStateFlags.SystemError | MeasurementStateFlags.SystemWarning | MeasurementStateFlags.MeasurementError;
        private const MeasurementStateFlags UserFlagMask = MeasurementStateFlags.UserDefinedFlag1 | MeasurementStateFlags.UserDefinedFlag2 | MeasurementStateFlags.UserDefinedFlag3 | MeasurementStateFlags.UserDefinedFlag4 | MeasurementStateFlags.UserDefinedFlag5;
        private const MeasurementStateFlags CalculatedValueMask = MeasurementStateFlags.CalcuatedValue;
        private const MeasurementStateFlags DiscardedValueMask = MeasurementStateFlags.DiscardedValue;

        /// <summary>
        /// Maps <see cref="MeasurementStateFlags"/> to <see cref="CompactMeasurementStateFlags"/>.
        /// </summary>
        /// <param name="stateFlags">Flags to map.</param>
        /// <returns><see cref="CompactMeasurementStateFlags"/> mapped from <see cref="MeasurementStateFlags"/>.</returns>
        public static CompactMeasurementStateFlags MapToCompactFlags(this MeasurementStateFlags stateFlags)
        {
            CompactMeasurementStateFlags mappedStateFlags = CompactMeasurementStateFlags.NoFlags;

            if ((stateFlags & DataRangeMask) > 0)
                mappedStateFlags |= CompactMeasurementStateFlags.DataRange;

            if ((stateFlags & DataQualityMask) > 0)
                mappedStateFlags |= CompactMeasurementStateFlags.DataQuality;

            if ((stateFlags & TimeQualityMask) > 0)
                mappedStateFlags |= CompactMeasurementStateFlags.TimeQuality;

            if ((stateFlags & SystemIssueMask) > 0)
                mappedStateFlags |= CompactMeasurementStateFlags.SystemIssue;

            if ((stateFlags & UserFlagMask) > 0)
                mappedStateFlags |= CompactMeasurementStateFlags.UserFlag;

            if ((stateFlags & CalculatedValueMask) > 0)
                mappedStateFlags |= CompactMeasurementStateFlags.CalculatedValue;

            if ((stateFlags & DiscardedValueMask) > 0)
                mappedStateFlags |= CompactMeasurementStateFlags.DiscardedValue;

            return mappedStateFlags;
        }

        /// <summary>
        /// Maps <see cref="CompactMeasurementStateFlags"/> to <see cref="MeasurementStateFlags"/>.
        /// </summary>
        /// <param name="stateFlags">Flags to map.</param>
        /// <returns><see cref="MeasurementStateFlags"/> mapped from <see cref="CompactMeasurementStateFlags"/>.</returns>
        public static MeasurementStateFlags MapToFullFlags(this CompactMeasurementStateFlags stateFlags)
        {
            MeasurementStateFlags mappedStateFlags = MeasurementStateFlags.Normal;

            if ((stateFlags & CompactMeasurementStateFlags.DataRange) > 0)
                mappedStateFlags |= DataRangeMask;

            if ((stateFlags & CompactMeasurementStateFlags.DataQuality) > 0)
                mappedStateFlags |= DataQualityMask;

            if ((stateFlags & CompactMeasurementStateFlags.TimeQuality) > 0)
                mappedStateFlags |= TimeQualityMask;

            if ((stateFlags & CompactMeasurementStateFlags.SystemIssue) > 0)
                mappedStateFlags |= SystemIssueMask;

            if ((stateFlags & CompactMeasurementStateFlags.UserFlag) > 0)
                mappedStateFlags |= UserFlagMask;

            if ((stateFlags & CompactMeasurementStateFlags.CalculatedValue) > 0)
                mappedStateFlags |= CalculatedValueMask;

            if ((stateFlags & CompactMeasurementStateFlags.DiscardedValue) > 0)
                mappedStateFlags |= DiscardedValueMask;

            return mappedStateFlags;
        }
    }

    #endregion

    /// <summary>
    /// Represents a <see cref="IMeasurement"/> that can be serialized with minimal size.
    /// </summary>
    /// <remarks>
    /// This measurement implementation is serialized through <see cref="ISupportBinaryImage"/>
    /// to allow complete control of binary format. Only critical measurements properties are
    /// serialized and every attempt is made to optimize the binary image for purposes of size
    /// reduction.
    /// </remarks>
    public class CompactMeasurement : Measurement, ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Fixed byte length of a <see cref="CompactMeasurement"/>.
        /// </summary>
        public const int FixedLength = 7;

        // Members
        private SignalIndexCache m_signalIndexCache;
        private bool m_includeTime;
        private long[] m_baseTimeOffsets;
        private int m_timeIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CompactMeasurement"/>.
        /// </summary>
        /// <param name="signalIndexCache">Signal index cache used to serialize or deserialize runtime information.</param>
        /// <param name="includeTime">Set to <c>true</c> to include time in serialized packet; otherwise <c>false</c>.</param>
        /// <param name="baseTimeOffsets">Base time offset array - set to <c>null</c> to use full fidelity measurement time.</param>
        public CompactMeasurement(SignalIndexCache signalIndexCache, bool includeTime = true, long[] baseTimeOffsets = null)
        {
            m_signalIndexCache = signalIndexCache;
            m_includeTime = includeTime;

            if (baseTimeOffsets == null)
                m_baseTimeOffsets = new long[] { 0, 0 };
            else
                m_baseTimeOffsets = baseTimeOffsets;
        }

        /// <summary>
        /// Creates a new <see cref="CompactMeasurement"/> from an existing <see cref="IMeasurement"/> value.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement"/> value.</param>
        /// <param name="signalIndexCache">Signal index cache used to serialize or deserialize runtime information.</param>
        /// <param name="includeTime">Set to <c>true</c> to include time in serialized packet; otherwise <c>false</c>.</param>
        /// <param name="baseTimeOffsets">Base time offset array - set to <c>null</c> to use full fidelity measurement time.</param>
        /// <param name="timeIndex">Time index to use for base offset.</param>
        public CompactMeasurement(IMeasurement measurement, SignalIndexCache signalIndexCache, bool includeTime = true, long[] baseTimeOffsets = null, int timeIndex = 0)
        {
            ID = measurement.ID;
            Key = measurement.Key;
            Value = measurement.Value;
            Adder = measurement.Adder;
            Multiplier = measurement.Multiplier;
            Timestamp = measurement.Timestamp;
            StateFlags = measurement.StateFlags;

            m_signalIndexCache = signalIndexCache;
            m_includeTime = includeTime;

            if (baseTimeOffsets == null)
                m_baseTimeOffsets = new long[] { 0, 0 };
            else
                m_baseTimeOffsets = baseTimeOffsets;

            m_timeIndex = timeIndex;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets flag that determines if time is serialized into measurement binary image.
        /// </summary>
        public bool IncludeTime
        {
            get
            {
                return m_includeTime;
            }
        }

        /// <summary>
        /// Gets the binary image of the <see cref="CompactMeasurement"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Field:     Bytes: <br/>
        /// --------   -------<br/>
        ///  Flags        1   <br/>
        ///   ID          2   <br/>
        ///  Value        4   <br/>
        ///  [Time]       2?  <br/>
        /// </para>
        /// <para>
        /// Constant Length = 7<br/>
        /// Variable Length = 2 or 8 (milliseconds plus offset or full fidelity time)
        /// </para>
        /// </remarks>
        public byte[] BinaryImage
        {
            get
            {
                int index = 0;

                // Encode flags
                CompactMeasurementStateFlags flags = StateFlags.MapToCompactFlags() | (m_timeIndex == 0 ? CompactMeasurementStateFlags.NoFlags : CompactMeasurementStateFlags.TimeIndex);

                // Allocate buffer to hold binary image
                long baseTimeOffset = m_baseTimeOffsets[m_timeIndex];
                int length = FixedLength + (m_includeTime ? (baseTimeOffset > 0 ? 2 : 8) : 0);
                byte[] buffer = new byte[length];

                // Added flags to beginning of buffer
                buffer[index++] = (byte)flags;

                // Encode runtime ID
                EndianOrder.BigEndian.CopyBytes(m_signalIndexCache.GetSignalIndex(ID), buffer, index);

                index += 2;

                // Encode adjusted value (accounts for adder and multipler)
                EndianOrder.BigEndian.CopyBytes((float)AdjustedValue, buffer, index);
                index += 4;

                if (m_includeTime)
                {
                    if (baseTimeOffset > 0)
                    {
                        // Encode millisecond offset timestamp
                        EndianOrder.BigEndian.CopyBytes((ushort)(Timestamp - baseTimeOffset).ToMilliseconds(), buffer, index);
                        index += 2;
                    }
                    else
                    {
                        // Encode full fidelity timestamp
                        EndianOrder.BigEndian.CopyBytes((long)Timestamp, buffer, index);
                        index += 8;
                    }
                }

                return buffer;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return FixedLength + (m_includeTime ? (m_baseTimeOffsets[m_timeIndex] > 0 ? 2 : 8) : 0);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="CompactMeasurement"/> from the specified binary image.
        /// </summary>
        /// <param name="buffer">Binary image to be used for initialization.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to be used for initialization.</param>
        /// <param name="count">Valid number of bytes within binary image.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        public int Initialize(byte[] buffer, int startIndex, int count)
        {
            if (count < 1)
                throw new InvalidOperationException("Not enough buffer available to deserialize measurement.");

            // Decode flags
            CompactMeasurementStateFlags flags = (CompactMeasurementStateFlags)buffer[startIndex];

            StateFlags = flags.MapToFullFlags();
            m_timeIndex = (byte)(flags & CompactMeasurementStateFlags.TimeIndex) > 0 ? 1 : 0;

            long baseTimeOffset = m_baseTimeOffsets[m_timeIndex];
            int length = FixedLength + (m_includeTime ? (baseTimeOffset > 0 ? 2 : 8) : 0);
            int index = 0;

            if (count < length)
                throw new InvalidOperationException("Not enough buffer available to deserialize measurement.");

            index = startIndex + 1;

            // Decode runtime ID
            ushort id = EndianOrder.BigEndian.ToUInt16(buffer, index);
            index += 2;

            // Restore signal identification
            Tuple<Guid, string, uint> tuple;

            if (m_signalIndexCache.Reference.TryGetValue(id, out tuple))
            {
                ID = tuple.Item1;
                Key = new MeasurementKey(tuple.Item1, tuple.Item3, tuple.Item2);
            }
            else
                throw new InvalidOperationException("Failed to find associated signal identification for runtime ID " + id);

            // Decode value
            Value = EndianOrder.BigEndian.ToSingle(buffer, index);
            index += 4;

            if (m_includeTime)
            {
                if (baseTimeOffset > 0)
                {
                    // Decode millisecond offset timestamp
                    Timestamp = baseTimeOffset + EndianOrder.BigEndian.ToUInt16(buffer, index) * Ticks.PerMillisecond;
                    index += 2;
                }
                else
                {
                    // Decode full fidelity timestamp
                    Timestamp = EndianOrder.BigEndian.ToInt64(buffer, index);
                    index += 8;
                }
            }

            return length;
        }

        #endregion
    }
}