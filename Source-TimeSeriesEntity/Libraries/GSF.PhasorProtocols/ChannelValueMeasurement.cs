//******************************************************************************************************
//  ChannelValueMeasurement.cs - Gbtc
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
//  03/07/2005 - J. Ritchie Carroll
//       Generated original version of source code.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  10/17/2013 - Stephen C. Wills
//       Redefined measurement to fit into ITimeSeriesEntity hierarchy.
//
//******************************************************************************************************

using System;
using System.ComponentModel;
using GSF.TimeSeries;
using GSF.Units;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a <see cref="IMeasurement{Double}"/> implementation for composite values of a given <see cref="IChannelValue{T}"/>.
    /// </summary>
    /// <typeparam name="T">Generic type T.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public sealed class ChannelValueMeasurement<T> : IMeasurement<double> where T : IChannelDefinition
    {
        #region [ Members ]

        // Fields
        private IChannelValue<T> m_parent;
        private Guid m_id;
        private MeasurementStateFlags m_stateFlags;
        private Ticks m_timestamp;
        private int m_valueIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="ChannelValueMeasurement{T}"/> given the specified parameters.
        /// </summary>
        /// <param name="parent">The reference to the <see cref="IChannelValue{T}"/> that this measurement derives its values from.</param>
        /// <param name="id">The fundamental identifier of the <see cref="ChannelValueMeasurement{T}"/></param>
        /// <param name="valueIndex">The index into the <see cref="IChannelValue{T}.GetCompositeValue"/> that this measurement derives its value from.</param>
        public ChannelValueMeasurement(IChannelValue<T> parent, Guid id, int valueIndex)
            : this(parent, id, -1, valueIndex)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ChannelValueMeasurement{T}"/> given the specified parameters.
        /// </summary>
        /// <param name="parent">The reference to the <see cref="IChannelValue{T}"/> that this measurement derives its values from.</param>
        /// <param name="id">The fundamental identifier of the <see cref="ChannelValueMeasurement{T}"/></param>
        /// <param name="timestamp">The exact timestamp, in ticks, of the data represented by this <see cref="ChannelValueMeasurement{T}"/></param>
        /// <param name="valueIndex">The index into the <see cref="IChannelValue{T}.GetCompositeValue"/> that this measurement derives its value from.</param>
        public ChannelValueMeasurement(IChannelValue<T> parent, Guid id, Ticks timestamp, int valueIndex)
            : this(parent, id, timestamp, GetParentFlags(parent, timestamp), valueIndex)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ChannelValueMeasurement{T}"/> given the specified parameters.
        /// </summary>
        /// <param name="parent">The reference to the <see cref="IChannelValue{T}"/> that this measurement derives its values from.</param>
        /// <param name="id">The fundamental identifier of the <see cref="ChannelValueMeasurement{T}"/></param>
        /// <param name="stateFlags">The <see cref="MeasurementStateFlags"/> associated with this <see cref="ChannelValueMeasurement{T}"/></param>
        /// <param name="valueIndex">The index into the <see cref="IChannelValue{T}.GetCompositeValue"/> that this measurement derives its value from.</param>
        public ChannelValueMeasurement(IChannelValue<T> parent, Guid id, MeasurementStateFlags stateFlags, int valueIndex)
            : this(parent, id, -1, stateFlags, valueIndex)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ChannelValueMeasurement{T}"/> given the specified parameters.
        /// </summary>
        /// <param name="parent">The reference to the <see cref="IChannelValue{T}"/> that this measurement derives its values from.</param>
        /// <param name="id">The fundamental identifier of the <see cref="ChannelValueMeasurement{T}"/></param>
        /// <param name="timestamp">The exact timestamp, in ticks, of the data represented by this <see cref="ChannelValueMeasurement{T}"/></param>
        /// <param name="stateFlags">The <see cref="MeasurementStateFlags"/> associated with this <see cref="ChannelValueMeasurement{T}"/></param>
        /// <param name="valueIndex">The index into the <see cref="IChannelValue{T}.GetCompositeValue"/> that this measurement derives its value from.</param>
        public ChannelValueMeasurement(IChannelValue<T> parent, Guid id, Ticks timestamp, MeasurementStateFlags stateFlags, int valueIndex)
        {
            m_parent = parent;
            m_id = id;
            m_timestamp = timestamp;
            m_stateFlags = stateFlags;
            m_valueIndex = valueIndex;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets reference to the <see cref="IChannelValue{T}"/> that this measurement derives its values from.
        /// </summary>
        public IChannelValue<T> Parent
        {
            get
            {
                return m_parent;
            }
        }

        /// <summary>
        /// Gets the <see cref="Guid"/> based signal ID of this <see cref="ChannelValueMeasurement{T}"/>, if available.
        /// </summary>
        public Guid ID
        {
            get
            {
                return m_id;
            }
        }

        /// <summary>
        /// Gets exact timestamp, in ticks, of the data represented by this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <remarks>
        /// This value returns timestamp of parent data frame unless assigned an alternate value.<br/>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public Ticks Timestamp
        {
            get
            {
                if (m_timestamp == -1)
                    m_timestamp = m_parent.Parent.Parent.Timestamp;

                return m_timestamp;
            }
        }

        /// <summary>
        /// Gets <see cref="MeasurementStateFlags"/> associated with this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        public MeasurementStateFlags StateFlags
        {
            get
            {
                return m_stateFlags;
            }
        }

        /// <summary>
        /// Gets index into the <see cref="IChannelValue{T}.GetCompositeValue"/> that this measurement derives its value from.
        /// </summary>
        public int ValueIndex
        {
            get
            {
                return m_valueIndex;
            }
        }

        /// <summary>
        /// Gets the channel value.
        /// </summary>
        public double Value
        {
            get
            {
                double value = m_parent.GetCompositeValue(m_valueIndex);

                // Convert phase angles to the -180 degrees to 180 degrees range
                if (m_parent is PhasorValueBase && m_valueIndex == (int)CompositePhasorValue.Angle)
                    value = Angle.FromDegrees(value).ToRange(-Math.PI, false).ToDegrees();

                return value;
            }
        }

        #endregion

        #region [ Static ]

        // Static Methods
        private static MeasurementStateFlags GetParentFlags(IChannelValue<T> parent, Ticks timestamp)
        {
            MeasurementStateFlags timeQuality = (parent.Parent.SynchronizationIsValid && timestamp != -1) ? MeasurementStateFlags.Normal : MeasurementStateFlags.BadTime;
            MeasurementStateFlags valueQuality = parent.Parent.DataIsValid ? MeasurementStateFlags.Normal : MeasurementStateFlags.BadData;
            return timeQuality | valueQuality;
        }

        #endregion
    }
}