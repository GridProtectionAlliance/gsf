//******************************************************************************************************
//  CompactMeasurement.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//  08/23/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  05/15/2011 - J. Ritchie Carroll
//       Added runtime size optimizations.
//  06/07/2011 - J. Ritchie Carroll
//       Implemented initialize issue fix as found and proposed by Luc Cezard.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using GSF.IO;
using GSF.IO.Compression;
using GSF.Parsing;

namespace GSF.TimeSeries.Transport
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
        /// A time quality flag was set.
        /// </summary>
        TimeQuality = (byte)Bits.Bit02,
        /// <summary>
        /// A system flag was set.
        /// </summary>
        SystemIssue = (byte)Bits.Bit03,
        /// <summary>
        /// Calculated value bit was set.
        /// </summary>
        CalculatedValue = (byte)Bits.Bit04,
        /// <summary>
        /// Discarded value bit was set.
        /// </summary>
        DiscardedValue = (byte)Bits.Bit05,
        /// <summary>
        /// Compact measurement timestamp was serialized using base time offset when set.
        /// </summary>
        BaseTimeOffset = (byte)Bits.Bit06,
        /// <summary>
        /// Use odd time index (i.e., 1) when bit is set; even time index (i.e., 0) when bit is clear.
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
    public class CompactMeasurement : Measurement, IBinaryMeasurement
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Fixed byte length of a <see cref="CompactMeasurement"/>.
        /// </summary>
        public const int FixedLength = 7;

        // Members
        private readonly SignalIndexCache m_signalIndexCache;
        private readonly bool m_includeTime;
        private readonly long[] m_baseTimeOffsets;
        private int m_timeIndex;
        private readonly bool m_useMillisecondResolution;
        private bool m_usingBaseTimeOffset;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CompactMeasurement"/>.
        /// </summary>
        /// <param name="signalIndexCache">Signal index cache used to serialize or deserialize runtime information.</param>
        /// <param name="includeTime">Set to <c>true</c> to include time in serialized packet; otherwise <c>false</c>.</param>
        /// <param name="baseTimeOffsets">Base time offset array - set to <c>null</c> to use full fidelity measurement time.</param>
        /// <param name="timeIndex">Time index to use for base offset.</param>
        /// <param name="useMillisecondResolution">Flag that determines if millisecond resolution is in use for this serialization.</param>
        public CompactMeasurement(SignalIndexCache signalIndexCache, bool includeTime = true, long[] baseTimeOffsets = null, int timeIndex = 0, bool useMillisecondResolution = false)
        {
            m_signalIndexCache = signalIndexCache;
            m_includeTime = includeTime;

            // We keep a clone of the base time offsets, if provided, since array contents can change at any time
            if ((object)baseTimeOffsets == null)
                m_baseTimeOffsets = s_emptyBaseTimeOffsets;
            else
                m_baseTimeOffsets = new[] { baseTimeOffsets[0], baseTimeOffsets[1] };

            m_timeIndex = timeIndex;
            m_useMillisecondResolution = useMillisecondResolution;
        }

        /// <summary>
        /// Creates a new <see cref="CompactMeasurement"/> from an existing <see cref="IMeasurement"/> value.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement"/> value.</param>
        /// <param name="signalIndexCache">Signal index cache used to serialize or deserialize runtime information.</param>
        /// <param name="includeTime">Set to <c>true</c> to include time in serialized packet; otherwise <c>false</c>.</param>
        /// <param name="baseTimeOffsets">Base time offset array - set to <c>null</c> to use full fidelity measurement time.</param>
        /// <param name="timeIndex">Time index to use for base offset.</param>
        /// <param name="useMillisecondResolution">Flag that determines if millisecond resolution is in use for this serialization.</param>
        public CompactMeasurement(IMeasurement measurement, SignalIndexCache signalIndexCache, bool includeTime = true, long[] baseTimeOffsets = null, int timeIndex = 0, bool useMillisecondResolution = false)
        {
            Metadata = measurement.Metadata;
            Value = measurement.Value;
            Timestamp = measurement.Timestamp;
            StateFlags = measurement.StateFlags;

            m_signalIndexCache = signalIndexCache;
            m_includeTime = includeTime;

            // We keep a clone of the base time offsets, if provided, since array contents can change at any time
            if ((object)baseTimeOffsets == null)
                m_baseTimeOffsets = s_emptyBaseTimeOffsets;
            else
                m_baseTimeOffsets = new[] { baseTimeOffsets[0], baseTimeOffsets[1] };

            m_timeIndex = timeIndex;
            m_useMillisecondResolution = useMillisecondResolution;
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
        /// Gets the length of the <see cref="CompactMeasurement"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                int length = FixedLength;

                if (m_includeTime)
                {
                    long baseTimeOffset = m_baseTimeOffsets[m_timeIndex];

                    if (baseTimeOffset > 0)
                    {
                        // See if timestamp will fit within space allowed for active base offset. We cache result so that post call
                        // to binary length, result will speed other subsequent parsing operations by not having to reevaluate.
                        long difference = (long)Timestamp - m_baseTimeOffsets[m_timeIndex];

                        if (difference > 0)
                            m_usingBaseTimeOffset = m_useMillisecondResolution ? difference / Ticks.PerMillisecond < ushort.MaxValue : difference < uint.MaxValue;
                        else
                            m_usingBaseTimeOffset = false;

                        if (m_usingBaseTimeOffset)
                        {
                            if (m_useMillisecondResolution)
                                length += 2;    // Use two bytes for millisecond resolution timestamp with valid offset
                            else
                                length += 4;    // Use four bytes for tick resolution timestamp with valid offset
                        }
                        else
                        {
                            // Use eight bytes for full fidelity time
                            length += 8;
                        }
                    }
                    else
                    {
                        // Use eight bytes for full fidelity time
                        length += 8;
                    }
                }

                return length;
            }
        }

        /// <summary>
        /// Gets offset compressed millisecond-resolution 2-byte timestamp.
        /// </summary>
        public ushort TimestampC2
        {
            get
            {
                return (ushort)((Timestamp - m_baseTimeOffsets[m_timeIndex]).ToMilliseconds());
            }
        }

        /// <summary>
        /// Gets offset compressed tick-resolution 4-byte timestamp.
        /// </summary>
        public uint TimestampC4
        {
            get
            {
                return (uint)((long)Timestamp - m_baseTimeOffsets[m_timeIndex]);
            }
        }

        /// <summary>
        /// Gets or sets byte level compact state flags with encoded time index and base time offset bits.
        /// </summary>
        public byte CompactStateFlags
        {
            get
            {
                // Encode compact state flags
                CompactMeasurementStateFlags flags = StateFlags.MapToCompactFlags();

                if (m_timeIndex != 0)
                    flags |= CompactMeasurementStateFlags.TimeIndex;

                if (m_usingBaseTimeOffset)
                    flags |= CompactMeasurementStateFlags.BaseTimeOffset;

                return (byte)flags;
            }
            set
            {
                // Decode compact state flags
                CompactMeasurementStateFlags flags = (CompactMeasurementStateFlags)value;
                StateFlags = flags.MapToFullFlags();
                m_timeIndex = (flags & CompactMeasurementStateFlags.TimeIndex) > 0 ? 1 : 0;
                m_usingBaseTimeOffset = (flags & CompactMeasurementStateFlags.BaseTimeOffset) > 0;
            }
        }

        /// <summary>
        /// Gets or sets the 2-byte run-time signal index for this measurement.
        /// </summary>
        public ushort RuntimeID
        {
            get
            {
                return m_signalIndexCache.GetSignalIndex(ID);
            }
            set
            {
                // Attempt to restore signal identification
                Tuple<Guid, string, uint> tuple;

                if (m_signalIndexCache.Reference.TryGetValue(value, out tuple))
                {
                    Metadata = MeasurementKey.LookUpOrCreate(tuple.Item1, tuple.Item2, tuple.Item3).Metadata;
                }
                else
                    throw new InvalidOperationException("Failed to find associated signal identification for runtime ID " + value);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="CompactMeasurement"/> from the specified binary image.
        /// </summary>
        /// <param name="buffer">Buffer containing binary image to parse.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start parsing.</param>
        /// <param name="length">Valid number of bytes within <paramref name="buffer"/> from <paramref name="startIndex"/>.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        /// <exception cref="InvalidOperationException">Not enough buffer available to deserialize measurement.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="length"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <paramref name="length"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
        public int ParseBinaryImage(byte[] buffer, int startIndex, int length)
        {
            buffer.ValidateParameters(startIndex, length);

            if (length < 1)
                throw new InvalidOperationException("Not enough buffer available to deserialize measurement.");

            int index = startIndex;

            // Decode state flags
            CompactStateFlags = buffer[index++];

            // Decode runtime ID
            RuntimeID = BigEndian.ToUInt16(buffer, index);
            index += 2;

            // Decode value
            Value = BigEndian.ToSingle(buffer, index);
            index += 4;

            if (m_includeTime)
            {
                if (m_usingBaseTimeOffset)
                {
                    long baseTimeOffset = m_baseTimeOffsets[m_timeIndex];

                    if (m_useMillisecondResolution)
                    {
                        // Decode 2-byte millisecond offset timestamp
                        if (baseTimeOffset > 0)
                            Timestamp = baseTimeOffset + BigEndian.ToUInt16(buffer, index) * Ticks.PerMillisecond;

                        index += 2;
                    }
                    else
                    {
                        // Decode 4-byte tick offset timestamp
                        if (baseTimeOffset > 0)
                            Timestamp = baseTimeOffset + BigEndian.ToUInt32(buffer, index);

                        index += 4;
                    }
                }
                else
                {
                    // Decode 8-byte full fidelity timestamp
                    Timestamp = BigEndian.ToInt64(buffer, index);
                    index += 8;
                }
            }

            return (index - startIndex);
        }

        /// <summary>
        /// Generates binary image of the <see cref="CompactMeasurement"/> and copies it into the given buffer, for <see cref="ISupportBinaryImage.BinaryLength"/> bytes.
        /// </summary>
        /// <param name="buffer">Buffer used to hold generated binary image of the source object.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to start writing.</param>
        /// <returns>The number of bytes written to the <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <see cref="ISupportBinaryImage.BinaryLength"/> is less than 0 -or- 
        /// <paramref name="startIndex"/> and <see cref="ISupportBinaryImage.BinaryLength"/> will exceed <paramref name="buffer"/> length.
        /// </exception>
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
        /// Variable Length = 0, 2, 4 or 8 (i.e., total size is 7, 9, 11 or 15)
        /// </para>
        /// </remarks>
        public int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            // Call to binary length property caches result of m_usingBaseTimeOffset
            int length = BinaryLength;

            buffer.ValidateParameters(startIndex, length);

            // Added encoded compact state flags to beginning of buffer
            buffer[startIndex++] = CompactStateFlags;

            // Encode runtime ID
            startIndex += BigEndian.CopyBytes(RuntimeID, buffer, startIndex);

            // Encode adjusted value (accounts for adder and multiplier)
            startIndex += BigEndian.CopyBytes((float)AdjustedValue, buffer, startIndex);

            if (m_includeTime)
            {
                if (m_usingBaseTimeOffset)
                {
                    if (m_useMillisecondResolution)
                    {
                        // Encode 2-byte millisecond offset timestamp
                        BigEndian.CopyBytes(TimestampC2, buffer, startIndex);
                    }
                    else
                    {
                        // Encode 4-byte ticks offset timestamp
                        BigEndian.CopyBytes(TimestampC4, buffer, startIndex);
                    }
                }
                else
                {
                    // Encode 8-byte full fidelity timestamp
                    BigEndian.CopyBytes((long)Timestamp, buffer, startIndex);
                }
            }

            return length;
        }

        #endregion

        #region [ Static ]

        // Static Fields
        private static readonly long[] s_emptyBaseTimeOffsets = new long[] { 0, 0 };

        #endregion
    }

    /// <summary>
    /// Defines extension functions related to <see cref="CompactMeasurement"/>.
    /// </summary>
    public static class CompactMeasurementExtensions
    {
        /// <summary>
        /// Attempts to compress payload of <see cref="CompactMeasurement"/> values onto the <paramref name="destination"/> stream.
        /// </summary>
        /// <param name="compactMeasurements">Payload of <see cref="CompactMeasurement"/> values.</param>
        /// <param name="destination">Memory based <paramref name="destination"/> stream to hold compressed payload.</param>
        /// <param name="compressionStrength">Compression strength to use.</param>
        /// <param name="includeTime">Flag that determines if time should be included in the compressed payload.</param>
        /// <param name="flags">Current <see cref="DataPacketFlags"/>.</param>
        /// <returns><c>true</c> if payload was compressed and encoded onto <paramref name="destination"/> stream; otherwise <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// Compressed payload will only be encoded onto <paramref name="destination"/> stream if compressed size would be smaller
        /// than normal serialized size.
        /// </para>
        /// <para>
        /// As an optimization this function uses a compression method that uses pointers to native structures, as such the
        /// endian order encoding of the compressed data will always be in the native-endian order of the operating system.
        /// This will be an important consideration when writing a endian order neutral payload decompressor. To help with
        /// this the actual endian order used during compression is marked in the data flags. However, measurements values
        /// are consistently encoded in big-endian order prior to buffer compression.
        /// </para>
        /// </remarks>
        public static bool CompressPayload(this IEnumerable<CompactMeasurement> compactMeasurements, BlockAllocatedMemoryStream destination, byte compressionStrength, bool includeTime, ref DataPacketFlags flags)
        {
            // Instantiate a buffer that is larger than we'll need
            byte[] buffer = new byte[ushort.MaxValue];

            // Go ahead an enumerate all the measurements - this will cast all values to compact measurements
            CompactMeasurement[] measurements = compactMeasurements.ToArray();
            int measurementCount = measurements.Length;
            int sizeToBeat = measurementCount * measurements[0].BinaryLength;
            int index = 0;

            // Encode compact state flags and runtime IDs together --
            // Together these are three bytes, so we pad with a zero byte.
            // The zero byte and state flags are considered to be more compressible
            // than the runtime ID, so these are stored in the higher order bytes.
            for (int i = 0; i < measurementCount; i++)
            {
                uint value = ((uint)measurements[i].CompactStateFlags << 16) | measurements[i].RuntimeID;
                index += NativeEndianOrder.Default.CopyBytes(value, buffer, index);
            }

            // Encode values
            for (int i = 0; i < measurementCount; i++)
            {
                // Encode using adjusted value (accounts for adder and multiplier)
                index += NativeEndianOrder.Default.CopyBytes((float)measurements[i].AdjustedValue, buffer, index);
            }

            if (includeTime)
            {
                // Encode timestamps
                for (int i = 0; i < measurementCount; i++)
                {
                    // Since large majority of 8-byte tick values will be repeated, they should compress well
                    index += NativeEndianOrder.Default.CopyBytes((long)measurements[i].Timestamp, buffer, index);
                }
            }

            // Attempt to compress buffer
            int compressedSize = PatternCompressor.CompressBuffer(buffer, 0, index, ushort.MaxValue, compressionStrength);

            // Only encode compressed buffer if compression actually helped payload size
            if (compressedSize <= sizeToBeat)
            {
                // Set payload compression flag
                flags |= DataPacketFlags.Compressed;

                // Make sure decompressor knows original endian encoding order
                if (BitConverter.IsLittleEndian)
                    flags |= DataPacketFlags.LittleEndianCompression;
                else
                    flags &= ~DataPacketFlags.LittleEndianCompression;

                // Copy compressed payload onto destination stream
                destination.Write(buffer, 0, compressedSize);
                return true;
            }

            // Clear payload compression flag
            flags &= ~DataPacketFlags.Compressed;
            return false;
        }

        /// <summary>
        /// Decompresses <see cref="CompactMeasurement"/> values from the given <paramref name="source"/> buffer.
        /// </summary>
        /// <param name="source">Buffer with compressed <see cref="CompactMeasurement"/> payload.</param>
        /// <param name="signalIndexCache">Current <see cref="SignalIndexCache"/>.</param>
        /// <param name="index">Index into buffer where compressed payload begins.</param>
        /// <param name="dataLength">Length of all data within <paramref name="source"/> buffer.</param>
        /// <param name="measurementCount">Number of compressed measurements in the payload.</param>
        /// <param name="includeTime">Flag that determines if timestamps as included in the payload.</param>
        /// <param name="flags">Current <see cref="DataPacketFlags"/>.</param>
        /// <returns>Decompressed <see cref="CompactMeasurement"/> values from the given <paramref name="source"/> buffer.</returns>
        public static CompactMeasurement[] DecompressPayload(this byte[] source, SignalIndexCache signalIndexCache, int index, int dataLength, int measurementCount, bool includeTime, DataPacketFlags flags)
        {
            CompactMeasurement[] measurements = new CompactMeasurement[measurementCount];

            // Actual data length has to take into account response byte and in-response-to server command byte in the payload header
            //int dataLength = length - index - 2;
            int bufferLength = PatternDecompressor.MaximumSizeDecompressed(dataLength);

            // Copy source data into a decompression buffer
            byte[] buffer = new byte[bufferLength];
            Buffer.BlockCopy(source, index, buffer, 0, dataLength);

            // Check that OS endian-order matches endian-order of compressed data
            if (!(BitConverter.IsLittleEndian && (flags & DataPacketFlags.LittleEndianCompression) > 0))
            {
                // TODO: Set a flag, e.g., Endianness decompressAs, to pass into pattern decompressor so it
                // can be modified to decompress a payload that is in a non-native Endian order
                throw new NotImplementedException("Cannot currently decompress payload that is not in native endian-order.");
            }

            // Attempt to decompress buffer
            int uncompressedSize = PatternDecompressor.DecompressBuffer(buffer, 0, dataLength, bufferLength);

            if (uncompressedSize == 0)
                throw new InvalidOperationException("Failed to decompress payload buffer - possible data corruption.");

            index = 0;

            // Decode ID and state flags
            for (int i = 0; i < measurementCount; i++)
            {
                uint value = NativeEndianOrder.Default.ToUInt32(buffer, index);

                measurements[i] = new CompactMeasurement(signalIndexCache, includeTime)
                {
                    CompactStateFlags = (byte)(value >> 16),
                    RuntimeID = (ushort)value
                };

                index += 4;
            }

            // Decode values
            for (int i = 0; i < measurementCount; i++)
            {
                measurements[i].Value = NativeEndianOrder.Default.ToSingle(buffer, index);
                index += 4;
            }

            if (includeTime)
            {
                // Decode timestamps
                for (int i = 0; i < measurementCount; i++)
                {
                    measurements[i].Timestamp = NativeEndianOrder.Default.ToInt64(buffer, index);
                    index += 8;
                }
            }

            return measurements;
        }
    }
}