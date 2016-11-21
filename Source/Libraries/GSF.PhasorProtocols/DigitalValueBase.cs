//******************************************************************************************************
//  DigitalValueBase.cs - Gbtc
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
//  02/18/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/5/2012 - Gavin E. Holden
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GSF.TimeSeries;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents the common implementation of the protocol independent representation of a digital value.
    /// </summary>
    [Serializable]
    public abstract class DigitalValueBase : ChannelValueBase<IDigitalDefinition>, IDigitalValue
    {
        #region [ Members ]

        // Fields
        private ushort m_value;
        private bool m_valueAssigned;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="DigitalValueBase"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="DigitalValueBase"/>.</param>
        /// <param name="digitalDefinition">The <see cref="IDigitalDefinition"/> associated with this <see cref="DigitalValueBase"/>.</param>
        protected DigitalValueBase(IDataCell parent, IDigitalDefinition digitalDefinition)
            : base(parent, digitalDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="DigitalValueBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="DigitalValueBase"/>.</param>
        /// <param name="digitalDefinition">The <see cref="IDigitalDefinition"/> associated with this <see cref="DigitalValueBase"/>.</param>
        /// <param name="value">The unsigned 16-bit integer value (composed of digital bits) that represents this <see cref="DigitalValueBase"/>.</param>
        protected DigitalValueBase(IDataCell parent, IDigitalDefinition digitalDefinition, ushort value)
            : base(parent, digitalDefinition)
        {
            m_value = value;
            m_valueAssigned = (value != ushort.MaxValue);
        }

        /// <summary>
        /// Creates a new <see cref="DigitalValueBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected DigitalValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize digital value
            m_value = info.GetUInt16("value");
            m_valueAssigned = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the unsigned 16-bit integer value (composed of digital bits) that represents this <see cref="DigitalValueBase"/>.
        /// </summary>
        public virtual ushort Value
        {
            get
            {
                return m_value;
            }
            set
            {
                m_value = value;
                m_valueAssigned = true;
            }
        }

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="DigitalValueBase"/> have been assigned a value.
        /// </summary>
        /// <returns>True, if no composite values have been assigned a value; otherwise, false.</returns>
        public override bool IsEmpty
        {
            get
            {
                return !m_valueAssigned;
            }
        }

        /// <summary>
        /// Gets total number of composite values that this <see cref="DigitalValueBase"/> provides.
        /// </summary>
        public override int CompositeValueCount
        {
            get
            {
                return 1;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        protected override int BodyLength
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="DigitalValueBase"/> object.
        /// </summary>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];

                BigEndian.CopyBytes(m_value, buffer, 0);

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="DigitalValueBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;
                byte[] valueBytes = BitConverter.GetBytes(Value);

                baseAttributes.Add("Digital Value", Value.ToString());
                baseAttributes.Add("Digital Value (Big Endian Bits)", ByteEncoding.BigEndianBinary.GetString(valueBytes));
                baseAttributes.Add("Digital Value (Hexadecimal)", "0x" + ByteEncoding.Hexadecimal.GetString(valueBytes));

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the specified composite value of this <see cref="DigitalValueBase"/>.
        /// </summary>
        /// <param name="index">Index of composite value to retrieve.</param>
        /// <remarks>
        /// Some <see cref="ChannelValueBase{T}"/> implementations can contain more than one value, this method is used to abstractly expose each value.
        /// </remarks>
        /// <returns>A <see cref="double"/> representing the composite value.</returns>
        public override double GetCompositeValue(int index)
        {
            if (index == 0)
                return m_value;

            throw new ArgumentOutOfRangeException(nameof(index), "Invalid composite index requested");
        }

        /// <summary>
        /// Gets function used to apply a downsampling filter over a sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        /// <param name="index">Index of composite value for which to retrieve its filter function.</param>
        /// <returns>Majority value filter function since all values are digital in nature.</returns>
        public override MeasurementValueFilterFunction GetMeasurementValueFilterFunction(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid composite index requested");

            // Digital values shouldn't be averaged, so a majority value filter is applied when downsampling
            return Measurement.MajorityValueFilter;
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            // Length is validated at a frame level well in advance so that low level parsing routines do not have
            // to re-validate that enough length is available to parse needed information as an optimization...

            m_value = BigEndian.ToUInt16(buffer, startIndex);
            m_valueAssigned = true;

            return 2;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize digital value
            info.AddValue("value", m_value);
        }

        #endregion
    }
}