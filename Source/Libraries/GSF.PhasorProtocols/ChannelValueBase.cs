//******************************************************************************************************
//  ChannelValueBase.cs - Gbtc
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
//  03/07/2005 - J. Ritchie Carroll
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
    /// Represents the common implementation of the protocol independent representation of any kind of data value.
    /// </summary>
    /// <typeparam name="T">Generic type.</typeparam>
    [Serializable]
    public abstract class ChannelValueBase<T> : ChannelBase, IChannelValue<T> where T : IChannelDefinition
    {
        #region [ Members ]

        // Fields
        private IDataCell m_parent;
        private T m_definition;
        private IMeasurement[] m_measurements;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ChannelValueBase{T}"/> from specified parameters.
        /// </summary>
        /// <param name="parent">The <see cref="IDataCell"/> parent of this <see cref="ChannelValueBase{T}"/>.</param>
        /// <param name="channelDefinition">The <see cref="IChannelDefinition"/> associated with this <see cref="ChannelValueBase{T}"/>.</param>
        protected ChannelValueBase(IDataCell parent, T channelDefinition)
        {
            m_parent = parent;
            m_definition = channelDefinition;
        }

        /// <summary>
        /// Creates a new <see cref="ChannelValueBase{T}"/> from serialization parameters.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> with populated with data.</param>
        /// <param name="context">The source <see cref="StreamingContext"/> for this deserialization.</param>
        protected ChannelValueBase(SerializationInfo info, StreamingContext context)
        {
            // Deserialize channel value
            m_parent = (IDataCell)info.GetValue("parent", typeof(IDataCell));
            m_definition = (T)info.GetValue("definition", typeof(T));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="IDataCell"/> parent of this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        public virtual IDataCell Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                m_parent = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IChannelDefinition"/> associated with this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        public virtual T Definition
        {
            get
            {
                return m_definition;
            }
            set
            {
                m_definition = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="GSF.PhasorProtocols.DataFormat"/> of this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        public virtual DataFormat DataFormat => m_definition.DataFormat;

        /// <summary>
        /// Gets text based label of this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        public virtual string Label => m_definition.Label;

        /// <summary>
        /// Gets boolean value that determines if none of the composite values of <see cref="ChannelValueBase{T}"/> have been assigned a value.
        /// </summary>
        /// <returns><c>true</c>, if no composite values have been assigned a value; otherwise, <c>false</c>.</returns>
        public abstract bool IsEmpty { get; }

        /// <summary>
        /// Gets total number of composite values that this <see cref="ChannelValueBase{T}"/> provides.
        /// </summary>
        public abstract int CompositeValueCount { get; }

        /// <summary>
        /// Gets the composite values of this <see cref="ChannelValueBase{T}"/> as an array of <see cref="IMeasurement"/> values.
        /// </summary>
        public virtual IMeasurement[] Measurements
        {
            get
            {
                // Create a measurement instance for each composite value the derived channel value exposes
                if ((object)m_measurements == null)
                {
                    m_measurements = new IMeasurement[CompositeValueCount];

                    // ChannelValueMeasurement dynamically accesses CompositeValues[x] for its value
                    for (int x = 0; x < m_measurements.Length; x++)
                        m_measurements[x] = CreateMeasurement(x);
                }

                return m_measurements;
            }
        }

        /// <summary>
        /// <see cref="Dictionary{TKey,TValue}"/> of string based property names and values for the <see cref="ChannelValueBase{T}"/> object.
        /// </summary>
        public override Dictionary<string, string> Attributes
        {
            get
            {
                Dictionary<string, string> baseAttributes = base.Attributes;
                int compositeValues = CompositeValueCount;

                baseAttributes.Add("Label", Label);
                baseAttributes.Add("Data Format", (int)DataFormat + ": " + DataFormat);
                baseAttributes.Add("Is Empty", IsEmpty.ToString());
                baseAttributes.Add("Total Composite Values", compositeValues.ToString());

                for (int x = 0; x < compositeValues; x++)
                {
                    baseAttributes.Add("     Composite Value " + x, " => " + GetCompositeValue(x).ToString());
                }

                return baseAttributes;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the specified composite value of this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        /// <param name="index">Index of composite value to retrieve.</param>
        /// <remarks>
        /// Some <see cref="ChannelValueBase{T}"/> implementations can contain more than one value, this method is used to abstractly expose each value.
        /// </remarks>
        /// <returns>A <see cref="double"/> representing the composite value.</returns>
        public abstract double GetCompositeValue(int index);

        /// <summary>
        /// Creates a new <see cref="IMeasurement"/> value for specified composite value for this <see cref="ChannelValueBase{T}"/>.
        /// </summary>
        /// <param name="valueIndex">Composite value index for which to derive new <see cref="IMeasurement"/> value.</param>
        /// <returns>New <see cref="IMeasurement"/> value for specified composite value for this <see cref="ChannelValueBase{T}"/>.</returns>
        protected virtual IMeasurement CreateMeasurement(int valueIndex)
        {
            Measurement measurement = new Measurement
            {
                Timestamp = Parent.Parent.Timestamp,
                Value = GetCompositeValue(valueIndex)
            };

            measurement.StateFlags = (Parent.SynchronizationIsValid && measurement.Timestamp.Value != -1 ? MeasurementStateFlags.Normal : MeasurementStateFlags.BadTime) | (Parent.DataIsValid ? MeasurementStateFlags.Normal : MeasurementStateFlags.BadData);

            return measurement;
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination <see cref="StreamingContext"/> for this serialization.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Serialize channel value
            info.AddValue("parent", m_parent, typeof(IDataCell));
            info.AddValue("definition", m_definition, typeof(T));
        }

        #endregion
    }
}