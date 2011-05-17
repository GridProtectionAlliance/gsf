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
//
//******************************************************************************************************

using System;
using TVA;
using TVA.Parsing;
using TVA.Security.Cryptography;

namespace TimeSeriesFramework.Transport
{
    /// <summary>
    /// <see cref="CompactMeasurement"/> state flags.
    /// </summary>
    [Flags]
    public enum StateFlags : byte
    {
        /// <summary>
        /// Use even time index when set; odd time index when not set.
        /// </summary>
        TimeIndex = (byte)Bits.Bit00,
        /// <summary>
        /// Use even cipher index when set; off cipher index when not set.
        /// </summary>
        CipherIndex = (byte)Bits.Bit01,
        /// <summary>
        /// Value quality is good is true when set; otherwise false.
        /// </summary>
        ValueQualityIsGood = (byte)Bits.Bit02,
        /// <summary>
        /// Time quality is good is true when set; otherwise false.
        /// </summary>
        TimeQualityIsGood = (byte)Bits.Bit03,
        /// <summary>
        /// Discarded is true when set; otherwise false.
        /// </summary>
        Discarded = (byte)Bits.Bit04,
        /// <summary>
        /// Bit reserved for future use.
        /// </summary>
        Reserved01 = (byte)Bits.Bit05,
        /// <summary>
        /// Bit reserved for future use.
        /// </summary>
        Reserved02 = (byte)Bits.Bit06,
        /// <summary>
        /// Bit reserved for future use.
        /// </summary>
        Reserved03 = (byte)Bits.Bit07,
        /// <summary>
        /// No flags.
        /// </summary>
        NoFlags = (byte)Bits.Nil
    }

    /// <summary>
    /// Represents a <see cref="IMeasurement"/> that can be serialized with minimal size.
    /// </summary>
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
        private byte[][][] m_keyIVs;
        private int m_timeIndex;
        private int m_cipherIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="CompactMeasurement"/>.
        /// </summary>
        /// <param name="signalIndexCache">Signal index cache used to serialize or deserialize runtime information.</param>
        /// <param name="includeTime">Set to <c>true</c> to include time in serialized packet; otherwise <c>false</c>.</param>
        /// <param name="baseTimeOffsets">Base time offset array - set to <c>null</c> to use full fidelity measurement time.</param>
        /// <param name="keyIVs">Key and initialization vector array - set to <c>null</c> to not use encryption.</param>
        public CompactMeasurement(SignalIndexCache signalIndexCache, bool includeTime = true, long[] baseTimeOffsets = null, byte[][][] keyIVs = null)
        {
            m_signalIndexCache = signalIndexCache;
            m_includeTime = includeTime;

            if (baseTimeOffsets == null)
                m_baseTimeOffsets = new long[] { 0, 0 };
            else
                m_baseTimeOffsets = baseTimeOffsets;

            m_keyIVs = keyIVs;
        }

        /// <summary>
        /// Creates a new <see cref="CompactMeasurement"/> from source <see cref="IMeasurement"/> value.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement"/> value.</param>
        /// <param name="signalIndexCache">Signal index cache used to serialize or deserialize runtime information.</param>
        /// <param name="includeTime">Set to <c>true</c> to include time in serialized packet; otherwise <c>false</c>.</param>
        /// <param name="baseTimeOffsets">Base time offset array - set to <c>null</c> to use full fidelity measurement time.</param>
        /// <param name="keyIVs">Key and initialization vector array - set to <c>null</c> to not use encryption.</param>
        /// <param name="timeIndex">Time index to use for base offset.</param>
        /// <param name="cipherIndex">Cipher index to use for cryptography.</param>
        public CompactMeasurement(IMeasurement measurement, SignalIndexCache signalIndexCache, bool includeTime = true, long[] baseTimeOffsets = null, byte[][][] keyIVs = null, int timeIndex = 0, int cipherIndex = 0)
            : base(measurement.ID, measurement.Source, measurement.SignalID, measurement.Value, measurement.Adder, measurement.Multiplier, measurement.Timestamp)
        {
            this.ValueQualityIsGood = measurement.ValueQualityIsGood;
            this.TimestampQualityIsGood = measurement.TimestampQualityIsGood;
            m_signalIndexCache = signalIndexCache;
            m_includeTime = includeTime;

            if (baseTimeOffsets == null)
                m_baseTimeOffsets = new long[] { 0, 0 };
            else
                m_baseTimeOffsets = baseTimeOffsets;

            m_keyIVs = keyIVs;
            m_timeIndex = timeIndex;
            m_cipherIndex = cipherIndex;
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
                StateFlags flags =
                    (m_timeIndex == 0 ? StateFlags.NoFlags : StateFlags.TimeIndex) |
                    (m_cipherIndex == 0 ? StateFlags.NoFlags : StateFlags.CipherIndex) |
                    (ValueQualityIsGood ? StateFlags.ValueQualityIsGood : StateFlags.NoFlags) |
                    (TimestampQualityIsGood ? StateFlags.TimeQualityIsGood : StateFlags.NoFlags) |
                    (IsDiscarded ? StateFlags.Discarded : StateFlags.NoFlags);

                // Allocate buffer to hold binary image
                long baseTimeOffset = m_baseTimeOffsets[m_timeIndex];
                int length = FixedLength + (m_includeTime ? (baseTimeOffset > 0 ? 2 : 8) : 0);
                byte[] buffer = new byte[length];

                // Added flags to beginning of buffer
                buffer[index++] = (byte)flags;

                // Encode runtime ID
                EndianOrder.BigEndian.CopyBytes(m_signalIndexCache.GetSignalIndex(Key), buffer, index);
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

                // If crypto keys were provided, encrypted data portion of buffer
                if (m_keyIVs != null)
                    Buffer.BlockCopy(buffer.Encrypt(1, length - 1, m_keyIVs[m_cipherIndex][0], m_keyIVs[m_cipherIndex][1], CipherStrength.Aes256), 0, buffer, 1, length - 1);

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
            StateFlags flags = (StateFlags)buffer[startIndex];
            m_timeIndex = (byte)(flags & StateFlags.TimeIndex) > 0 ? 1 : 0;
            m_cipherIndex = (byte)(flags & StateFlags.CipherIndex) > 0 ? 1 : 0;
            ValueQualityIsGood = ((byte)(flags & StateFlags.ValueQualityIsGood) > 0);
            TimestampQualityIsGood = ((byte)(flags & StateFlags.TimeQualityIsGood) > 0);
            IsDiscarded = ((byte)(flags & StateFlags.Discarded) > 0);

            long baseTimeOffset = m_baseTimeOffsets[m_timeIndex];
            int length = FixedLength + (m_includeTime ? (baseTimeOffset > 0 ? 2 : 8) : 0);
            int index = 0;

            if (count < length)
                throw new InvalidOperationException("Not enough buffer available to deserialize measurement.");

            // If crypto keys were provided, decrypt data portion of buffer
            if (m_keyIVs != null)
                buffer = buffer.Decrypt(startIndex + 1, length - 1, m_keyIVs[m_cipherIndex][0], m_keyIVs[m_cipherIndex][1], CipherStrength.Aes256);
            else
                index = 1;

            // Decode runtime ID
            ushort id = EndianOrder.BigEndian.ToUInt16(buffer, index);
            index += 2;

            // Restore signal identification
            Tuple<Guid, MeasurementKey> tuple;

            if (m_signalIndexCache.Reference.TryGetValue(id, out tuple))
            {
                SignalID = tuple.Item1;
                Key = tuple.Item2;
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