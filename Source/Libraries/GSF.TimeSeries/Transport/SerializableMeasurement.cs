//******************************************************************************************************
//  SerializableMeasurement.cs - Gbtc
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
//  06/07/2011 - J. Ritchie Carroll
//       Implemented binary image issue fix as found and proposed by Luc Cezard.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Text;
using GSF.Parsing;

#pragma warning disable 618

namespace GSF.TimeSeries.Transport
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
    public class SerializableMeasurement : Measurement, IBinaryMeasurement
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Fixed byte length of a <see cref="SerializableMeasurement"/>.
        /// </summary>
        public const int FixedLength = 64;

        // Fields
        private readonly Encoding m_encoding;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SerializableMeasurement"/>.
        /// </summary>
        /// <param name="encoding">Character encoding used to convert strings to binary.</param>
        public SerializableMeasurement(Encoding encoding)
        {
            if ((object)encoding == null)
                throw new ArgumentNullException(nameof(encoding), "Cannot create serializable measurement with no encoding.");

            m_encoding = encoding;
        }

        /// <summary>
        /// Creates a new <see cref="SerializableMeasurement"/> from an existing <see cref="IMeasurement"/> value.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement"/> value.</param>
        /// <param name="encoding">Character encoding used to convert strings to binary.</param>
        public SerializableMeasurement(IMeasurement measurement, Encoding encoding)
            : this(encoding)
        {
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
        /// Gets the length of the <see cref="SerializableMeasurement"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                int sourceLength = m_encoding.GetByteCount(Key.Source.ToNonNullString());
                int tagLength = m_encoding.GetByteCount(TagName.ToNonNullString());
                return FixedLength + sourceLength + tagLength;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="SerializableMeasurement"/> from the specified binary image.
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

            if (length < FixedLength)
                throw new InvalidOperationException("Not enough buffer available to deserialize measurement");

            int size, index = startIndex;
            uint keyID;
            string keySource = "";

            // Decode key ID
            keyID = BigEndian.ToUInt32(buffer, index);
            index += 4;

            // Decode key source string length
            size = BigEndian.ToInt32(buffer, index);
            index += 4;

            // Decode key source string
            if (size > 0)
            {
                keySource = m_encoding.GetString(buffer, index, size);
                index += size;
            }

            // Decode signal ID
            Guid signalID = EndianOrder.BigEndian.ToGuid(buffer, index);
            index += 16;

            // Apply parsed key changes
            Key = MeasurementKey.LookUpOrCreate(signalID, keySource, keyID);

            // Decode tag name string length
            size = BigEndian.ToInt32(buffer, index);
            index += 4;

            // Decode tag name string
            if (size > 0)
            {
                TagName = m_encoding.GetString(buffer, index, size);
                index += size;
            }
            else
                TagName = null;

            // Decode value
            Value = BigEndian.ToDouble(buffer, index);
            index += 8;

            // Decode adder
            Adder = BigEndian.ToDouble(buffer, index);
            index += 8;

            // Decode multiplier
            Multiplier = BigEndian.ToDouble(buffer, index);
            index += 8;

            // Decode timestamp
            Timestamp = BigEndian.ToInt64(buffer, index);
            index += 8;

            // Decode state flags
            StateFlags = (MeasurementStateFlags)BigEndian.ToUInt32(buffer, index);
            index += 4;

            return (index - startIndex);
        }

        /// <summary>
        /// Generates binary image of the <see cref="SerializableMeasurement"/> and copies it into the given buffer, for <see cref="ISupportBinaryImage.BinaryLength"/> bytes.
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
        /// Multiplier      8    <br/>
        ///   Ticks         8    <br/>
        ///   Flags         4    <br/>
        /// </para>
        /// <para>
        /// Constant Length = 64<br/>
        /// Variable Length = SourceLen + TagLen
        /// </para>
        /// </remarks>
        public int GenerateBinaryImage(byte[] buffer, int startIndex)
        {
            int length = BinaryLength;

            buffer.ValidateParameters(startIndex, length);

            byte[] bytes;
            int size, index = startIndex;
            string source = Key.Source.ToNonNullString();
            string tagName = TagName.ToNonNullString();

            // Encode key ID
            BigEndian.CopyBytes(Key.ID, buffer, index);
            index += 4;

            // Encode key source string length
            bytes = m_encoding.GetBytes(source);
            size = bytes.Length;
            BigEndian.CopyBytes(size, buffer, index);
            index += 4;

            // Encode key source string
            if (size > 0)
            {
                Buffer.BlockCopy(bytes, 0, buffer, index, size);
                index += size;
            }

            // Encode signal ID
            EndianOrder.BigEndian.CopyBytes(ID, buffer, index);
            index += 16;

            // Encode tag name string length
            bytes = m_encoding.GetBytes(tagName);
            size = bytes.Length;
            BigEndian.CopyBytes(size, buffer, index);
            index += 4;

            // Encode tag name string
            if (size > 0)
            {
                Buffer.BlockCopy(bytes, 0, buffer, index, size);
                index += size;
            }

            // Encode value
            BigEndian.CopyBytes(Value, buffer, index);
            index += 8;

            // Encode adder
            BigEndian.CopyBytes(Adder, buffer, index);
            index += 8;

            // Encode multiplier
            BigEndian.CopyBytes(Multiplier, buffer, index);
            index += 8;

            // Encode timestamp
            BigEndian.CopyBytes((long)Timestamp, buffer, index);
            index += 8;

            // Encode state flags
            BigEndian.CopyBytes((uint)StateFlags, buffer, index);

            return length;
        }

        #endregion
    }
}