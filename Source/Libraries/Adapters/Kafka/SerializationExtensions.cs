//******************************************************************************************************
//  MeasurementExtensions.cs - Gbtc
//
//  Copyright © 2015, Grid Protection Alliance.  All Rights Reserved.
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
//  10/22/2015 - Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Runtime.CompilerServices;
using GSF;
using GSF.TimeSeries;
using Misakai.Kafka;

namespace KafkaAdapters
{
    #region [ Enumerations ]

    /// <summary>
    /// Defines the data quality flags applied to a Kafka serialized measurement.
    /// </summary>
    [Flags]
    public enum DataQualityFlags : byte
    {
        /// <summary>
        /// If no flags are set, measurement quality is normal.
        /// </summary>
        Normal = (byte)Bits.Nil,
        /// <summary>
        /// If bit 0 is set, measurement value quality is bad - as reported by source.
        /// </summary>
        ValueQualityIsBad = (byte)Bits.Bit00,
        /// <summary>
        /// If bit 1 is set, measurement time quality is bad - as reported by source.
        /// </summary>
        TimeQualityIsBad = (byte)Bits.Bit01,
        /// <summary>
        /// Decode version mask - remaining bits used for decoding version, allows 64 possible versions (0 to 63).
        /// </summary>
        DecodeVersionMask = (byte)(Bits.Bit02 | Bits.Bit03 | Bits.Bit04 | Bits.Bit05 | Bits.Bit06 | Bits.Bit07)
    }

    #endregion

    /// <summary>
    /// Defines extension methods for handling Kafka message serialization to and from time-series measurements.
    /// </summary>
    /// <remarks>
    /// The goal of this serialization implementation is to make it very simple to deserialize
    /// Kafka time-series messages from other language implementations, such as C++ or Java.
    /// </remarks>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Defines the size of serialized Kafka measurement key.
        /// </summary>
        /// <remarks>
        /// Key format:<br/>
        /// - long Time: 64-bit time of measurement in ticks, i.e., 100-nanosecond intervals since 0/0/0000<br/>
        /// - uint ID:   32-bit measurement identifier (temporal index into metadata)
        /// </remarks>
        public const int KeySize = sizeof(long) + sizeof(uint);                     // 12 bytes

        /// <summary>
        /// Defines the size of serialized Kafka measurement value.
        /// </summary>
        /// <remarks>
        /// Value format:<br/>
        /// - byte Version: 8-bit metadata serialization version<br/>
        /// - byte Quality: 8-bit measurement quality - bit 0 = bad value, bit 1 = bad time*<br/>
        /// - double Value: 64-bit floating-point measurement value
        /// * Remaining bits used to define decoding version.
        /// </remarks>
        public const int ValueSize = sizeof(byte) + sizeof(byte) + sizeof(double);  // 10 bytes

        /// <summary>
        /// Defines the current message decoding version.
        /// </summary>
        public const byte DecodeVersion = 0;

        /// <summary>
        /// Serializes a time-series <see cref="IMeasurement"/> instance into key-value buffers for a Kafka message.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> instance to serialize.</param>
        /// <param name="message">Kafka message to hold serialized key-value buffers.</param>
        /// <param name="metadataVersion">Current metadata serialization version.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void KakfaSerialize(this IMeasurement measurement, Message message, byte metadataVersion)
        {
            if ((object)measurement == null)
                throw new ArgumentNullException(nameof(measurement));

            if ((object)message == null)
                throw new ArgumentNullException(nameof(message));

            // Copy in "key" for Kafka message, for a time-series entity, the "timestamp" and "ID"
            message.Key = new byte[KeySize];
            Buffer.BlockCopy(BitConverter.GetBytes(measurement.Timestamp), 0, message.Key, 0, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(measurement.Key.ID), 0, message.Key, 8, 4);

            // Copy in "value" for Kafka message, for a time-series entity, the "version", "quality" and "value"
            message.Value = new byte[ValueSize];
            message.Value[0] = metadataVersion;
            message.Value[1] = (byte)((int)measurement.StateFlags.DeriveQualityFlags() | (DecodeVersion << 2));
            Buffer.BlockCopy(BitConverter.GetBytes(measurement.AdjustedValue), 0, message.Value, 2, 8);
        }

        /// <summary>
        /// Deserializes a Kafka message into a time-series <see cref="Measurement"/>.
        /// </summary>
        /// <param name="message">Time-series serialized Kafka message.</param>
        /// <param name="id">Numeric ID of measurement deserialized from Kafka message.</param>
        /// <param name="metadataVersion">Message metadata serialization version.</param>
        /// <returns><see cref="Measurement"/> value deserialized from a time-series serialized Kafka message.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Measurement KafkaDeserialize(this Message message, out uint id, out byte metadataVersion)
        {
            if ((object)message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.Key.Length < KeySize)
                throw new ArgumentOutOfRangeException(nameof(message.Key), $"Buffer size too small - expected {KeySize} bytes for Kafka time-series message key");

            if (message.Value.Length < ValueSize)
                throw new ArgumentOutOfRangeException(nameof(message.Value), $"Buffer size too small - expected {ValueSize} bytes for Kafka time-series message value");

            byte messageDecodeVersion = (byte)((message.Value[1] & (byte)DataQualityFlags.DecodeVersionMask) >> 2);

            if (messageDecodeVersion > DecodeVersion)
                throw new InvalidOperationException($"This Kafka time-series deserialization engine cannot decode version {messageDecodeVersion} serialized messages");

            id = BitConverter.ToUInt32(message.Key, 8);
            metadataVersion = message.Value[0];

            return new Measurement
            {
                Timestamp = BitConverter.ToInt64(message.Key, 0),
                Value = BitConverter.ToDouble(message.Value, 2),
                StateFlags = DeriveStateFlags((DataQualityFlags)message.Value[1] & ~DataQualityFlags.DecodeVersionMask)                
            };
        }

        /// <summary>
        /// Derive Kafka time-seres data quality flags from measurement state flags 
        /// </summary>
        /// <param name="stateFlags"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DataQualityFlags DeriveQualityFlags(this MeasurementStateFlags stateFlags)
        {
            DataQualityFlags qualityFlags = DataQualityFlags.Normal;

            if (stateFlags.HasFlag(MeasurementStateFlags.BadData))
                qualityFlags |= DataQualityFlags.ValueQualityIsBad;

            if (stateFlags.HasFlag(MeasurementStateFlags.BadTime))
                qualityFlags |= DataQualityFlags.TimeQualityIsBad;

            return qualityFlags;
        }

        /// <summary>
        /// Derive measurement state flags from Kafka time-seres data quality flags
        /// </summary>
        /// <param name="qualityFlags"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MeasurementStateFlags DeriveStateFlags(this DataQualityFlags qualityFlags)
        {
            MeasurementStateFlags stateFlags = MeasurementStateFlags.Normal;

            if (qualityFlags.HasFlag(DataQualityFlags.ValueQualityIsBad))
                stateFlags |= MeasurementStateFlags.BadData;

            if (qualityFlags.HasFlag(DataQualityFlags.TimeQualityIsBad))
                stateFlags |= MeasurementStateFlags.BadTime;

            return stateFlags;
        }
    }
}
