//******************************************************************************************************
//  SerializableMeasurement.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
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
//       Implemented binary image issue fix as found and proposed by Luc Cezard.
//  10/17/2013 - Stephen C. Wills
//       Removed SerializableMeasurement from the ITimeSeriesEntity hierarchy, which is read-only by
//       design, so that the CompactMeasurement can read/write for deserialization. ToMeasurement()
//       method was provided to allow for simple conversion to immutable time-series measurements.
//
//******************************************************************************************************

using System;
using System.Text;
using GSF.Parsing;

namespace GSF.TimeSeries.Transport
{
    /// <summary>
    /// Represents a measurement that can be serialized.
    /// </summary>
    /// <remarks>
    /// This measurement implementation is serialized through <see cref="ISupportBinaryImage"/>
    /// to allow complete control of binary format. All measurement properties are serialized
    /// at their full resolution and no attempt is made to optimize the binary image for
    /// purposes of size reduction.
    /// </remarks>
    public class SerializableMeasurement : ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Fixed byte length of a <see cref="SerializableMeasurement"/>.
        /// </summary>
        public const int FixedLength = 64;

        // Fields
        private Guid m_id;
        private Ticks m_timestamp;
        private MeasurementStateFlags m_stateFlags;
        private double m_value;

        private string m_source;
        private uint m_pointID;
        private string m_tagName;
        private double m_adder;
        private double m_multiplier;

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
                throw new ArgumentNullException("encoding", "Cannot create serializable measurement with no encoding.");

            m_encoding = encoding;
        }

        /// <summary>
        /// Creates a new <see cref="SerializableMeasurement"/> from an existing <see cref="IMeasurement{Double}"/> value.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement{Double}"/> value.</param>
        /// <param name="encoding">Character encoding used to convert strings to binary.</param>
        public SerializableMeasurement(IMeasurement<double> measurement, Encoding encoding)
            : this(encoding)
        {
            ID = measurement.ID;
            Timestamp = measurement.Timestamp;
            StateFlags = measurement.StateFlags;
            Value = measurement.Value;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="Guid"/> based signal ID of this <see cref="SerializableMeasurement"/>.
        /// </summary>
        /// <remarks>
        /// This is the fundamental identifier of the measurement.
        /// </remarks>
        public Guid ID
        {
            get
            {
                return m_id;
            }
            set
            {
                m_id = value;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="SerializableMeasurement"/>.
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public Ticks Timestamp
        {
            get
            {
                return m_timestamp;
            }
            set
            {
                m_timestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementStateFlags"/> associated with this <see cref="SerializableMeasurement"/>.
        /// </summary>
        public MeasurementStateFlags StateFlags
        {
            get
            {
                return m_stateFlags;
            }
            set
            {
                m_stateFlags = value;
            }
        }

        /// <summary>
        /// Gets or sets the raw value of this <see cref="SerializableMeasurement"/>.
        /// </summary>
        public double Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
            }
        }

        /// <summary>
        /// Gets or sets the source of this <see cref="SerializableMeasurement"/>.
        /// </summary>
        public string Source
        {
            get
            {
                return m_source;
            }
            set
            {
                m_source = value;
            }
        }

        /// <summary>
        /// Gets or sets the integer point ID of this <see cref="SerializableMeasurement"/>.
        /// </summary>
        public uint PointID
        {
            get
            {
                return m_pointID;
            }
            set
            {
                m_pointID = value;
            }
        }

        /// <summary>
        /// Gets or sets the human-readable tag name of this <see cref="SerializableMeasurement"/>.
        /// </summary>
        public string TagName
        {
            get
            {
                return m_tagName;
            }
            set
            {
                m_tagName = value;
            }
        }

        /// <summary>
        /// Gets or sets the adder applied to the value of this <see cref="SerializableMeasurement"/>.
        /// </summary>
        public double Adder
        {
            get
            {
                return m_adder;
            }
            set
            {
                m_adder = value;
            }
        }

        /// <summary>
        /// Gets or sets the multiplier applied to the value of this <see cref="SerializableMeasurement"/>.
        /// </summary>
        public double Multiplier
        {
            get
            {
                return m_multiplier;
            }
            set
            {
                m_multiplier = value;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="SerializableMeasurement"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                int sourceLength = m_encoding.GetByteCount(m_source.ToNonNullString());
                int tagLength = m_encoding.GetByteCount(m_tagName.ToNonNullString());
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
            m_pointID = EndianOrder.BigEndian.ToUInt32(buffer, index);
            index += 4;

            // Decode key source string length
            size = EndianOrder.BigEndian.ToInt32(buffer, index);
            index += 4;

            // Decode key source string
            if (size > 0)
            {
                m_source = m_encoding.GetString(buffer, index, size);
                index += size;
            }

            // Decode signal ID
            m_id = EndianOrder.BigEndian.ToGuid(buffer, index);
            index += 16;

            // Decode tag name string length
            size = EndianOrder.BigEndian.ToInt32(buffer, index);
            index += 4;

            // Decode tag name string
            if (size > 0)
            {
                m_tagName = m_encoding.GetString(buffer, index, size);
                index += size;
            }
            else
                m_tagName = null;

            // Decode value
            m_value = EndianOrder.BigEndian.ToDouble(buffer, index);
            index += 8;

            // Decode adder
            m_adder = EndianOrder.BigEndian.ToDouble(buffer, index);
            index += 8;

            // Decode multiplier
            m_multiplier = EndianOrder.BigEndian.ToDouble(buffer, index);
            index += 8;

            // Decode timestamp
            m_timestamp = EndianOrder.BigEndian.ToInt64(buffer, index);
            index += 8;

            // Decode state flags
            m_stateFlags = (MeasurementStateFlags)EndianOrder.BigEndian.ToUInt32(buffer, index);
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
            string source = m_source.ToNonNullString();
            string tagName = m_tagName.ToNonNullString();

            // Encode key ID
            EndianOrder.BigEndian.CopyBytes(m_pointID, buffer, index);
            index += 4;

            // Encode key source string length
            bytes = m_encoding.GetBytes(source);
            size = bytes.Length;
            EndianOrder.BigEndian.CopyBytes(size, buffer, index);
            index += 4;

            // Encode key source string
            if (size > 0)
            {
                Buffer.BlockCopy(bytes, 0, buffer, index, size);
                index += size;
            }

            // Encode signal ID
            EndianOrder.BigEndian.CopyBytes(m_id, buffer, index);
            index += 16;

            // Encode tag name string length
            bytes = m_encoding.GetBytes(tagName);
            size = bytes.Length;
            EndianOrder.BigEndian.CopyBytes(size, buffer, index);
            index += 4;

            // Encode tag name string
            if (size > 0)
            {
                Buffer.BlockCopy(bytes, 0, buffer, index, size);
                index += size;
            }

            // Encode value
            EndianOrder.BigEndian.CopyBytes(m_value, buffer, index);
            index += 8;

            // Encode adder
            EndianOrder.BigEndian.CopyBytes(m_adder, buffer, index);
            index += 8;

            // Encode multiplier
            EndianOrder.BigEndian.CopyBytes(m_multiplier, buffer, index);
            index += 8;

            // Encode timestamp
            EndianOrder.BigEndian.CopyBytes((long)m_timestamp, buffer, index);
            index += 8;

            // Encode state flags
            EndianOrder.BigEndian.CopyBytes((uint)m_stateFlags, buffer, index);

            return length;
        }

        /// <summary>
        /// Converts the <see cref="SerializableMeasurement"/> to <see cref="IMeasurement{Double}"/>.
        /// </summary>
        /// <returns>This measurement converted to <see cref="IMeasurement{Double}"/>.</returns>
        public IMeasurement<double> ToMeasurement()
        {
            return new Measurement<double>(m_id, m_timestamp, m_stateFlags, m_value);
        }

        #endregion
    }
}