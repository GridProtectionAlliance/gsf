//******************************************************************************************************
//  FrequencyValueBase.cs - Gbtc
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  12/17/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GSF.PhasorProtocols
{
    #region [ Enumerations ]

    /// <summary>
    /// Composite frequency value indices enumeration.
    /// </summary>
    public enum CompositeFrequencyValue
    {
        /// <summary>
        /// Composite frequency value index.
        /// </summary>
        Frequency,
        /// <summary>
        /// Composite dF/dt value index.
        /// </summary>
        DfDt
    }

    #endregion

    /// <summary>
    /// Represents the common implementation of the protocol independent representation of a frequency and dF/dt value.
    /// </summary>
    [Serializable]
    public abstract class FrequencyValueBase : ChannelValueBase<IFrequencyDefinition>, IFrequencyValue
    {
        #region [ Members ]

        // Fields
        private double m_frequency;
        private double m_dfdt;
        private bool m_frequencyAssigned;
        private bool m_dfdtAssigned;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="FrequencyValueBase"/>.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="FrequencyValueBase"/>.</param>
        /// <param name="frequencyDefinition">The <see cref="IFrequencyDefinition"/> associated with this <see cref="FrequencyValueBase"/>.</param>
        protected FrequencyValueBase(IDataCell parent, IFrequencyDefinition frequencyDefinition)
            : base(parent, frequencyDefinition)
        {
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyValueBase"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="FrequencyValueBase"/>.</param>
        /// <param name="frequencyDefinition">The <see cref="IFrequencyDefinition"/> associated with this <see cref="FrequencyValueBase"/>.</param>
        /// <param name="frequency">The floating point value that represents this <see cref="FrequencyValueBase"/>.</param>
        /// <param name="dfdt">The floating point value that represents the change in this <see cref="FrequencyValueBase"/> over time.</param>
        protected FrequencyValueBase(IDataCell parent, IFrequencyDefinition frequencyDefinition, double frequency, double dfdt)
            : base(parent, frequencyDefinition)
        {
            m_frequency = frequency;
            m_dfdt = dfdt;

            m_frequencyAssigned = !double.IsNaN(frequency);
            m_dfdtAssigned = !double.IsNaN(dfdt);
        }

        /// <summary>
        /// Creates a new <see cref="FrequencyValueBase"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected FrequencyValueBase(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            // Deserialize frequency value
            m_frequency = info.GetDouble("frequency");
            m_dfdt = info.GetDouble("dfdt");

            m_frequencyAssigned = true;
            m_dfdtAssigned = true;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the floating point value that represents this <see cref="FrequencyValueBase"/>.
        /// </summary>
        public virtual double Frequency
        {
            get => m_frequency;
            set
            {
                m_frequency = value;
                m_frequencyAssigned = true;
            }
        }

        /// <summary>
        /// Gets or sets the floating point value that represents the change in this <see cref="FrequencyValueBase"/> over time.
        /// </summary>
        public virtual double DfDt
        {
            get => m_dfdt;
            set
            {
                m_dfdt = value;
                m_dfdtAssigned = true;
            }
        }

        /// <summary>
        /// Gets or sets the unscaled integer representation of this <see cref="FrequencyValueBase"/>.
        /// </summary>
        public virtual int UnscaledFrequency
        {
            get
            {
                unchecked
                {
                    return (int)((m_frequency - Definition.Offset) * Definition.ScalingValue);
                }
            }
            set
            {
                m_frequency = value / (double)Definition.ScalingValue + Definition.Offset;
                m_frequencyAssigned = true;
            }
        }

        /// <summary>
        /// Gets or sets the unscaled integer representation of the change in this <see cref="FrequencyValueBase"/> over time.
        /// </summary>
        public virtual int UnscaledDfDt
        {
            get
            {
                unchecked
                {
                    return (int)((m_dfdt - Definition.DfDtOffset) * Definition.DfDtScalingValue);
                }
            }
            set
            {
                m_dfdt = value / (double)Definition.DfDtScalingValue + Definition.DfDtOffset;
                m_dfdtAssigned = true;
            }
        }

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="FrequencyValueBase"/> have been assigned a value.
        /// </summary>
        /// <returns>True, if no composite values have been assigned a value; otherwise, false.</returns>
        public override bool IsEmpty => !m_frequencyAssigned || !m_dfdtAssigned;

        /// <summary>
        /// Gets total number of composite values that this <see cref="FrequencyValueBase"/> provides.
        /// </summary>
        public override int CompositeValueCount => 2;

        /// <summary>
        /// Gets the length of the <see cref="BodyImage"/>.
        /// </summary>
        /// <remarks>
        /// The base implementation assumes fixed integer values are represented as 16-bit signed
        /// integers and floating point values are represented as 32-bit single-precision floating-point
        /// values (i.e., short and float data types respectively).
        /// </remarks>
        protected override int BodyLength
        {
            get
            {
                if (DataFormat == DataFormat.FixedInteger)
                    return 4;
                return 8;
            }
        }

        /// <summary>
        /// Gets the binary body image of the <see cref="FrequencyValueBase"/> object.
        /// </summary>
        /// <remarks>
        /// The base implementation assumes fixed integer values are represented as 16-bit signed
        /// integers and floating point values are represented as 32-bit single-precision floating-point
        /// values (i.e., short and float data types respectively).
        /// </remarks>
        protected override byte[] BodyImage
        {
            get
            {
                byte[] buffer = new byte[BodyLength];

                // Had to make a decision on usage versus typical protocol implementation when
                // exposing values as double / int when protocols typically use float / short for
                // transmission. Exposing values as double / int makes class more versatile by
                // allowing future protocol implementations to support higher resolution values
                // simply by overriding BodyLength, BodyImage and ParseBodyImage. However, exposing
                // the double / int values runs the risk of providing values that are outside the
                // data type limitations, hence the unchecked section below. Risk should generally
                // be low in typical usage scenarios since values being transmitted via a generated
                // image were likely parsed previously from a binary image with the same constraints.
                unchecked
                {
                    if (DataFormat == DataFormat.FixedInteger)
                    {
                        BigEndian.CopyBytes((short)UnscaledFrequency, buffer, 0);
                        BigEndian.CopyBytes((short)UnscaledDfDt, buffer, 2);
                    }
                    else
                    {
                        BigEndian.CopyBytes((float)m_frequency, buffer, 0);
                        BigEndian.CopyBytes((float)m_dfdt, buffer, 4);
                    }
                }

                return buffer;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="FrequencyValueBase"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;

                baseAttributes.Add("Frequency Value", Frequency.ToString());
                baseAttributes.Add("dF/dt Value", DfDt.ToString());
                baseAttributes.Add("Unscaled Frequency Value", UnscaledFrequency.ToString());
                baseAttributes.Add("Unscaled dF/dt Value", UnscaledDfDt.ToString());

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the specified composite value of this <see cref="FrequencyValueBase"/>.
        /// </summary>
        /// <param name="index">Index of composite value to retrieve.</param>
        /// <remarks>
        /// Some <see cref="ChannelValueBase{T}"/> implementations can contain more than one value, this method is used to abstractly expose each value.
        /// </remarks>
        /// <returns>A <see cref="double"/> representing the composite value.</returns>
        public override double GetCompositeValue(int index)
        {
            switch (index)
            {
                case (int)CompositeFrequencyValue.Frequency:
                    return m_frequency;
                case (int)CompositeFrequencyValue.DfDt:
                    return m_dfdt;
                default:
                    throw new ArgumentOutOfRangeException(nameof(index), "Invalid composite index requested");
            }
        }

        /// <summary>
        /// Parses the binary body image.
        /// </summary>
        /// <param name="buffer">Binary image to parse.</param>
        /// <param name="startIndex">Start index into <paramref name="buffer"/> to begin parsing.</param>
        /// <param name="length">Length of valid data within <paramref name="buffer"/>.</param>
        /// <returns>The length of the data that was parsed.</returns>
        /// <remarks>
        /// The base implementation assumes fixed integer values are represented as 16-bit signed
        /// integers and floating point values are represented as 32-bit single-precision floating-point
        /// values (i.e., short and float data types respectively).
        /// </remarks>
        protected override int ParseBodyImage(byte[] buffer, int startIndex, int length)
        {
            // Length is validated at a frame level well in advance so that low level parsing routines do not have
            // to re-validate that enough length is available to parse needed information as an optimization...

            if (DataFormat == DataFormat.FixedInteger)
            {
                UnscaledFrequency = BigEndian.ToInt16(buffer, startIndex);
                UnscaledDfDt = BigEndian.ToInt16(buffer, startIndex + 2);

                return 4;
            }

            m_frequency = BigEndian.ToSingle(buffer, startIndex);
            m_dfdt = BigEndian.ToSingle(buffer, startIndex + 4);

            m_frequencyAssigned = true;
            m_dfdtAssigned = true;

            return 8;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            // Serialize frequency value
            info.AddValue("frequency", m_frequency);
            info.AddValue("dfdt", m_dfdt);
        }

        #endregion
    }
}