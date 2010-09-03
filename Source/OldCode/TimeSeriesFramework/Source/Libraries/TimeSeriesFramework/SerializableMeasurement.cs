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
    public class SerializableMeasurement : Measurement, ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Fixed byte length of a <see cref="SerializableMeasurement"/>.
        /// </summary>
        public const int FixedLength = 61;

        #endregion
        
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SerializableMeasurement"/>.
        /// </summary>
        public SerializableMeasurement()
        {
        }

        /// <summary>
        /// Creates a new <see cref="SerializableMeasurement"/> from source <see cref="IMeasurement"/> value.
        /// </summary>
        /// <param name="m">Source <see cref="IMeasurement"/> value.</param>
        public SerializableMeasurement(IMeasurement m) : base(m.ID, m.Source, m.SignalID, m.Value, m.Adder, m.Multiplier, m.Timestamp)
        {
            this.ValueQualityIsGood = m.ValueQualityIsGood;
            this.TimestampQualityIsGood = m.TimestampQualityIsGood;
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
        ///   ID            4    <br/>
        /// SourceLen       4    <br/>
        ///  Source     SourceLen<br/>
        /// SignalID       16    <br/>
        ///  TagLen         4    <br/>
        ///   Tag        TagLen  <br/>
        ///   Value         8    <br/>
        ///   Adder         8    <br/>
        /// Multipler       8    <br/>
        ///   Ticks         8    <br/>
        ///   Flags         1    <br/>
        /// </para>
        /// <para>
        /// Constant Length = 61<br/>
        /// Variable Length = SourceLen + TagLen
        /// </para>
        /// </remarks>
        public byte[] BinaryImage
        {
            get 
            {
                byte[] bytes, buffer;
                int length, index = 0;
                string source = Source.ToNonNullString();
                string tagName = TagName.ToNonNullString();

                // Allocate buffer to hold binary image
                buffer = new byte[FixedLength + source.Length + tagName.Length];

                // Encode ID
                EndianOrder.BigEndian.CopyBytes(ID, buffer, index);
                index += 4;

                // Encode source string length
                bytes = Encoding.Unicode.GetBytes(source);
                length = bytes.Length;
                EndianOrder.BigEndian.CopyBytes(length, buffer, index);
                index += 4;

                // Encode source string
                if (length > 0)
                {
                    Buffer.BlockCopy(bytes, 0, buffer, index, length);
                    index += length;
                }

                // Encode signal ID
                EndianOrder.BigEndian.CopyBytes(SignalID, buffer, index);
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

                // Encode flags
                buffer[index] = (byte)((ValueQualityIsGood ? Bits.Bit00 : Bits.Nil) | (TimestampQualityIsGood ? Bits.Bit01 : Bits.Nil) | (IsDiscarded ? Bits.Bit02 : Bits.Nil));

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
                return FixedLength + Source.ToNonNullString().Length + TagName.ToNonNullString().Length;
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
                throw new InvalidOperationException("Not enough buffer available to deserialized measurement.");

            int length, index = startIndex;

            // Decode ID
            ID = EndianOrder.BigEndian.ToUInt32(buffer, index);
            index += 4;

            // Decode source string length
            length = EndianOrder.BigEndian.ToInt32(buffer, index);
            index += 4;

            // Decode source string
            if (length > 0)
            {
                Source = Encoding.Unicode.GetString(buffer, index, length);
                index += length;
            }

            // Decode signal ID
            SignalID = EndianOrder.BigEndian.ToGuid(buffer, index);
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

            // Decode flags
            ValueQualityIsGood = ((buffer[index] & (byte)Bits.Bit00) > 0);
            TimestampQualityIsGood = ((buffer[index] & (byte)Bits.Bit01) > 0);
            IsDiscarded = ((buffer[index] & (byte)Bits.Bit02) > 0);
            index++;

            return (index - startIndex);
        }

        #endregion
    }
}