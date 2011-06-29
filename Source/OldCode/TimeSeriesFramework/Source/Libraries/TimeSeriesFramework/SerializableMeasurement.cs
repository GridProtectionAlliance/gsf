//******************************************************************************************************
//  SerializableMeasurement.cs - Gbtc
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
//  06/07/2011 - J. Ritchie Carroll
//       Implemented binary image bug fix as found and proposed by Luc Cezard.
//
//******************************************************************************************************

using System;
using System.Text;
using TVA;
using TVA.Parsing;

namespace TimeSeriesFramework
{
    /// <summary>
    /// Represents a <see cref="IMeasurement"/> that can be serialized.
    /// </summary>
    /// <remarks>
    /// This measurement implementation is serialized through <see cref="ISupportBinaryImage"/>
    /// to allow complete control of binary format. All measurement properties are serialized
    /// at their full resolution and no attempt is made to optimize the binary image for
    /// purposes of size reduction.
    /// </remarks>
    public class SerializableMeasurement : Measurement, ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Fixed byte length of a <see cref="SerializableMeasurement"/>.
        /// </summary>
        public const int FixedLength = 64;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SerializableMeasurement"/>.
        /// </summary>
        public SerializableMeasurement()
        {
        }

        /// <summary>
        /// Creates a new <see cref="SerializableMeasurement"/> from an existing <see cref="IMeasurement"/> value.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement"/> value.</param>
        public SerializableMeasurement(IMeasurement measurement)
        {
            ID = measurement.ID;
            Key = measurement.Key;
            Value = measurement.Value;
            Adder = measurement.Adder;
            Multiplier = measurement.Multiplier;
            Timestamp = measurement.Timestamp;
            StateFlags = measurement.StateFlags;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the binary image of the <see cref="SerializableMeasurement"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Field:      Bytes:   <br/>
        /// ---------   ---------<br/>
        ///  Key ID         4    <br/>
        /// SourceLen       4    <br/>
        ///  Source     SourceLen<br/>
        /// Signal ID      16    <br/>
        ///  TagLen         4    <br/>
        ///   Tag        TagLen  <br/>
        ///   Value         8    <br/>
        ///   Adder         8    <br/>
        /// Multipler       8    <br/>
        ///   Ticks         8    <br/>
        ///   Flags         4    <br/>
        /// </para>
        /// <para>
        /// Constant Length = 64<br/>
        /// Variable Length = SourceLen + TagLen
        /// </para>
        /// </remarks>
        public byte[] BinaryImage
        {
            get
            {
                byte[] bytes, buffer;
                int length, index = 0;
                string source = Key.Source.ToNonNullString();
                string tagName = TagName.ToNonNullString();

                // Encode source string length
                bytes = Encoding.Unicode.GetBytes(source);
                length = bytes.Length;

                // Encode tag name string length
                bytes = Encoding.Unicode.GetBytes(tagName);
                length += bytes.Length;

                // Allocate buffer to hold binary image
                buffer = new byte[FixedLength + length];

                // Encode key ID
                EndianOrder.BigEndian.CopyBytes(Key.ID, buffer, index);
                index += 4;

                // Encode key source string length
                bytes = Encoding.Unicode.GetBytes(source);
                length = bytes.Length;
                EndianOrder.BigEndian.CopyBytes(length, buffer, index);
                index += 4;

                // Encode key source string
                if (length > 0)
                {
                    Buffer.BlockCopy(bytes, 0, buffer, index, length);
                    index += length;
                }

                // Encode signal ID
                EndianOrder.BigEndian.CopyBytes(ID, buffer, index);
                index += 16;

                // Encode tag name string length
                bytes = Encoding.Unicode.GetBytes(tagName);
                length = bytes.Length;
                EndianOrder.BigEndian.CopyBytes(length, buffer, index);
                index += 4;

                // Encode tag name string
                if (length > 0)
                {
                    Buffer.BlockCopy(bytes, 0, buffer, index, length);
                    index += length;
                }

                // Encode value
                EndianOrder.BigEndian.CopyBytes(Value, buffer, index);
                index += 8;

                // Encode adder
                EndianOrder.BigEndian.CopyBytes(Adder, buffer, index);
                index += 8;

                // Encode multiplier
                EndianOrder.BigEndian.CopyBytes(Multiplier, buffer, index);
                index += 8;

                // Encode timestamp
                EndianOrder.BigEndian.CopyBytes((long)Timestamp, buffer, index);
                index += 8;

                // Encode state flags
                EndianOrder.BigEndian.CopyBytes((uint)StateFlags, buffer, index);

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
                return FixedLength + Key.Source.ToNonNullString().Length + TagName.ToNonNullString().Length;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="SerializableMeasurement"/> from the specified binary image.
        /// </summary>
        /// <param name="buffer">Binary image to be used for initialization.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to be used for initialization.</param>
        /// <param name="count">Valid number of bytes within binary image.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        public int Initialize(byte[] buffer, int startIndex, int count)
        {
            if (count < FixedLength)
                throw new InvalidOperationException("Not enough buffer available to deserialize measurement.");

            int length, index = startIndex;
            uint id;
            string source = "";

            // Decode key ID
            id = EndianOrder.BigEndian.ToUInt32(buffer, index);
            index += 4;

            // Decode key source string length
            length = EndianOrder.BigEndian.ToInt32(buffer, index);
            index += 4;

            // Decode key source string
            if (length > 0)
            {
                source = Encoding.Unicode.GetString(buffer, index, length);
                index += length;
            }

            // Apply parsed key changes
            Key = new MeasurementKey(id, source);

            // Decode signal ID
            ID = EndianOrder.BigEndian.ToGuid(buffer, index);
            index += 16;

            // Decode tag name string length
            length = EndianOrder.BigEndian.ToInt32(buffer, index);
            index += 4;

            // Decode tag name string
            if (length > 0)
            {
                TagName = Encoding.Unicode.GetString(buffer, index, length);
                index += length;
            }
            else
                TagName = null;

            // Decode value
            Value = EndianOrder.BigEndian.ToDouble(buffer, index);
            index += 8;

            // Decode adder
            Adder = EndianOrder.BigEndian.ToDouble(buffer, index);
            index += 8;

            // Decode multiplier
            Multiplier = EndianOrder.BigEndian.ToDouble(buffer, index);
            index += 8;

            // Decode timestamp
            Timestamp = EndianOrder.BigEndian.ToInt64(buffer, index);
            index += 8;

            // Decode state flags
            StateFlags = (MeasurementStateFlags)EndianOrder.BigEndian.ToUInt32(buffer, index);
            index += 4;

            return (index - startIndex);
        }

        #endregion
    }
}