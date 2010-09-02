//******************************************************************************************************
//  SerializableMeasurementSlim.cs - Gbtc
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVA;

using TVA.Parsing;

namespace TimeSeriesFramework
{
    /// <summary>
    /// Represents a <see cref="IMeasurement"/> that can be serialized with minimal size.
    /// </summary>
    public class SerializableMeasurementSlim : Measurement, ISupportBinaryImage
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Fixed byte length of a <see cref="SerializableMeasurementSlim"/>.
        /// </summary>
        public const int FixedLength = 25;

        // Members
        private bool m_includeTime;

        #endregion
        
        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="SerializableMeasurementSlim"/>.
        /// </summary>
        /// <param name="includeTime">Determines if time is included in binary images.</param>
        public SerializableMeasurementSlim(bool includeTime)
        {
            m_includeTime = includeTime;
        }

        /// <summary>
        /// Creates a new <see cref="SerializableMeasurementSlim"/> from source <see cref="IMeasurement"/> value.
        /// </summary>
        /// <param name="m">Source <see cref="IMeasurement"/> value.</param>
        /// <param name="includeTime">Determines if time is included in binary images.</param>
        public SerializableMeasurementSlim(IMeasurement m, bool includeTime) : base(m.ID, m.Source, m.SignalID, m.Value, m.Adder, m.Multiplier, m.Timestamp)
        {
            this.ValueQualityIsGood = m.ValueQualityIsGood;
            this.TimestampQualityIsGood = m.TimestampQualityIsGood;
            m_includeTime = includeTime;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets flags that determines if time is included in binary images.
        /// </summary>
        public bool IncludeTime
        {
            get
            {
                return m_includeTime;
            }
            set
            {
                m_includeTime = value;
            }
        }

        /// <summary>
        /// Gets the binary image of the <see cref="SerializableMeasurementSlim"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Field:     Bytes: <br/>
        /// --------   -------<br/>
        /// SignalID     16   <br/>
        ///  Value        8   <br/>
        ///  [Time]       8?  <br/>
        ///  Flags        1   <br/>
        /// </para>
        /// <para>
        /// Constant Length = 25<br/>
        /// Variable Length = 8 (time is optional)
        /// </para>
        /// </remarks>
        public byte[] BinaryImage
        {
            get 
            {
                byte[] buffer;
                int index = 0;

                // Allocate buffer to hold binary image
                buffer = new byte[FixedLength];

                // Encode signal ID
                EndianOrder.BigEndian.CopyBytes(SignalID, buffer, index);
                index += 16;

                // Encode adjusted value (accounts for adder and multipler)
                EndianOrder.BigEndian.CopyBytes(AdjustedValue, buffer, index);
                index += 8;

                if (m_includeTime)
                {
                    // Encode timestamp
                    EndianOrder.BigEndian.CopyBytes((long)Timestamp, buffer, index);
                    index += 8;
                }

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
                return FixedLength + (m_includeTime ? 8 : 0);
            }
        }

        #endregion        

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="SerializableMeasurementSlim"/> from the specified binary image.
        /// </summary>
        /// <param name="buffer">Binary image to be used for initialization.</param>
        /// <param name="startIndex">0-based starting index in the <paramref name="buffer"/> to be used for initialization.</param>
        /// <param name="count">Valid number of bytes within binary image.</param>
        /// <returns>The number of bytes used for initialization in the <paramref name="buffer"/> (i.e., the number of bytes parsed).</returns>
        public int Initialize(byte[] buffer, int startIndex, int count)
        {
            if (count < BinaryLength)
                throw new InvalidOperationException("Not enough buffer available to deserialized measurement.");

            int index = startIndex;

            // Decode signal ID
            SignalID = EndianOrder.BigEndian.ToGuid(buffer, index);
            index += 16;

            // Decode value
            Value = EndianOrder.BigEndian.ToDouble(buffer, index);
            index += 8;

            if (m_includeTime)
            {
                // Decode timestamp
                Timestamp = EndianOrder.BigEndian.ToInt64(buffer, index);
                index += 8;
            }

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