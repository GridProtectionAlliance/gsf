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
using GSF.TimeSeries;
using Misakai.Kafka;

namespace KafkaAdapters
{
    /// <summary>
    /// Defines extension methods for <see cref="IMeasurement"/> implementations for use in Kafka.
    /// </summary>
    static class MeasurementExtensions
    {
        /// <summary>
        /// Serializes an <see cref="IMeasurement"/> instance into key-value buffers for a Kafka message.
        /// </summary>
        /// <param name="measurement"><see cref="IMeasurement"/> instance to serialize.</param>
        /// <param name="message">Kafka message to hold serialized key-value buffers.</param>
        public static void KakfaSerialize(this IMeasurement measurement, Message message)
        {
            if ((object)measurement == null)
                throw new ArgumentNullException(nameof(measurement));

            if ((object)message == null)
                throw new ArgumentNullException(nameof(message));

            // Copy in "key" for Kafka message, for time-series value, the "timestamp"
            message.Key = BitConverter.GetBytes(measurement.Timestamp);

            // Copy in "value" for Kafka message, for a time-series value, the "value" and "quality"
            message.Value = new byte[8];
            Buffer.BlockCopy(BitConverter.GetBytes((float)measurement.AdjustedValue), 0, message.Value, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes((uint)measurement.StateFlags), 0, message.Value, 4, 4);
        }

        /// <summary>
        /// Deserializes a Kafka message into a <see cref="Measurement"/>.
        /// </summary>
        /// <param name="message">Time-series serialized Kafka message.</param>
        /// <returns><see cref="Measurement"/> value deserialized from a time-series serialized Kafka message.</returns>
        public static Measurement KafkaDeserialize(this Message message)
        {
            if ((object)message == null)
                throw new ArgumentNullException(nameof(message));

            if (message.Key.Length < 8)
                throw new ArgumentOutOfRangeException(nameof(message.Key), "Buffer size too small - expected 8 bytes for Kafka time-series message key");

            if (message.Value.Length < 8)
                throw new ArgumentOutOfRangeException(nameof(message.Value), "Buffer size too small - expected 8 bytes for Kafka time-series message value");

            return new Measurement
            {
                Timestamp = BitConverter.ToInt64(message.Key, 0),
                Value = BitConverter.ToSingle(message.Value, 0),
                StateFlags = (MeasurementStateFlags)BitConverter.ToUInt32(message.Value, 4)
            };
        }
    }
}
