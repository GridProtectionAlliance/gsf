//******************************************************************************************************
//  ChannelValueMeasurement.cs - Gbtc
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
using System.ComponentModel;
using GSF.TimeSeries;
using GSF.Units;

namespace GSF.PhasorProtocols
{
    /// <summary>
    /// Represents a <see cref="IMeasurement"/> implementation for composite values of a given <see cref="IChannelValue{T}"/>.
    /// </summary>
    /// <typeparam name="T">Generic type T.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class ChannelValueMeasurement<T> : MeasurementBase, IMeasurement where T : IChannelDefinition
    {
        #region [ Members ]

        // Fields
        private IChannelValue<T> m_parent;
        private MeasurementStateFlags m_stateFlags;
        private bool m_stateFlagsAssigned;
        private Ticks m_timestamp;
        private Ticks m_receivedTimestamp;
        private Ticks m_publishedTimestamp;
        private int m_valueIndex;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="ChannelValueMeasurement{T}"/> given the specified parameters.
        /// </summary>
        /// <param name="parent">The reference to the <see cref="IChannelValue{T}"/> that this measurement derives its values from.</param>
        /// <param name="valueIndex">The index into the <see cref="IChannelValue{T}.GetCompositeValue"/> that this measurement derives its value from.</param>
        public ChannelValueMeasurement(IChannelValue<T> parent, int valueIndex)
        {

            m_parent = parent;
            m_valueIndex = valueIndex;
            m_timestamp = -1;
            m_receivedTimestamp = DateTime.UtcNow.Ticks;

            //Note: This might not be compatible code. Please review.

            ///// <summary>
            ///// Gets or sets function used to apply a down-sampling filter over a sequence of <see cref="IMeasurement"/> values.
            ///// </summary>
            //public virtual MeasurementValueFilterFunction MeasurementValueFilter
            //{
            //    get
            //    {
            //        // If measurement user has assigned a specific filter for this measurement, we use it
            //        if (m_measurementValueFilter != null)
            //            return m_measurementValueFilter;

            //        // Otherwise we use default filter algorithm as specified by the parent channel value
            //        return m_parent.GetMeasurementValueFilterFunction(m_valueIndex);
            //    }
            //    set
            //    {
            //        m_measurementValueFilter = value;
            //    }
            //}

            CommonMeasurementFields = CommonMeasurementFields.ChangeMeasurementValueFilter(m_parent.GetMeasurementValueFilterFunction(m_valueIndex));
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets reference to the <see cref="IChannelValue{T}"/> that this measurement derives its values from.
        /// </summary>
        public IChannelValue<T> Parent
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
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="ChannelValueMeasurement{T}"/>.
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
            set
            {
                m_timestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of when this <see cref="ChannelValueMeasurement{T}"/> was received (i.e., created).
        /// </summary>
        /// <remarks>
        /// <para>In the default implementation, this timestamp will simply be the ticks of <see cref="DateTime.UtcNow"/> of when this class was created.</para>
        /// <para>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.</para>
        /// </remarks>
        public Ticks ReceivedTimestamp
        {
            get
            {
                return m_receivedTimestamp;
            }
            set
            {
                m_receivedTimestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of when this <see cref="ChannelValueMeasurement{T}"/> was published (post-processing).
        /// </summary>
        /// <remarks>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public Ticks PublishedTimestamp
        {
            get
            {
                return m_publishedTimestamp;
            }
            set
            {
                m_publishedTimestamp = value;
            }
        }

        /// <summary>
        /// Gets or sets index into the <see cref="IChannelValue{T}.GetCompositeValue"/> that this measurement derives its value from.
        /// </summary>
        public int ValueIndex
        {
            get
            {
                return m_valueIndex;
            }
            set
            {
                m_valueIndex = value;
            }
        }

        /// <summary>
        /// Gets or sets the raw measurement value that is not offset by <see cref="MeasurementBase.Adder"/> and <see cref="MeasurementBase.Multiplier"/>.
        /// </summary>
        /// <returns>Raw value of this <see cref="ChannelValueMeasurement{T}"/> (i.e., value that is not offset by <see cref="MeasurementBase.Adder"/> and <see cref="MeasurementBase.Multiplier"/>).</returns>
        public double Value
        {
            get
            {
                return m_parent.GetCompositeValue(m_valueIndex);
            }
            set
            {
                throw new NotImplementedException("Cannot update derived phasor measurement, composite values are read-only");
            }
        }

        /// <summary>
        /// Gets the adjusted numeric value of this measurement, taking into account the specified <see cref="MeasurementBase.Adder"/> and <see cref="MeasurementBase.Multiplier"/> offsets.
        /// </summary>
        /// <remarks>
        /// Note that returned value will be offset by <see cref="MeasurementBase.Adder"/> and <see cref="MeasurementBase.Multiplier"/>.
        /// </remarks>
        /// <returns><see cref="Value"/> offset by <see cref="MeasurementBase.Adder"/> and <see cref="MeasurementBase.Multiplier"/> (i.e., <c><see cref="Value"/> * <see cref="MeasurementBase.Multiplier"/> + <see cref="MeasurementBase.Adder"/></c>).</returns>
        public double AdjustedValue
        {
            get
            {
                double adjustedValue = m_parent.GetCompositeValue(m_valueIndex) * Multiplier + Adder;

                // Convert phase angles to the -180 degrees to 180 degrees range
                if (m_parent is PhasorValueBase && m_valueIndex == (int)CompositePhasorValue.Angle)
                    adjustedValue = Angle.FromDegrees(adjustedValue).ToRange(-Math.PI, false).ToDegrees();

                return adjustedValue;
            }
        }

        /// <summary>
        /// Gets or sets <see cref="MeasurementStateFlags"/> associated with this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        public MeasurementStateFlags StateFlags
        {
            get
            {
                if (m_stateFlagsAssigned)
                    return m_stateFlags;

                return (m_parent.Parent.SynchronizationIsValid && Timestamp != -1 ? MeasurementStateFlags.Normal : MeasurementStateFlags.BadTime) | (m_parent.Parent.DataIsValid ? MeasurementStateFlags.Normal : MeasurementStateFlags.BadData);
            }
            set
            {
                m_stateFlags = value;
                m_stateFlagsAssigned = true;
            }
        }

        BigBinaryValue ITimeSeriesValue.Value
        {
            get
            {
                return Value;
            }
            set
            {
                throw new NotImplementedException("Cannot update derived phasor measurement, composite values are read-only");
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Returns a <see cref="String"/> that represents the current <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <returns>A <see cref="String"/> that represents the current <see cref="ChannelValueMeasurement{T}"/>.</returns>
        public override string ToString()
        {
            return Measurement.ToString(this);
        }

        /// <summary>
        /// Determines whether the specified <see cref="ITimeSeriesValue"/> is equal to the current <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <param name="other">The <see cref="ITimeSeriesValue"/> to compare with the current <see cref="ChannelValueMeasurement{T}"/>.</param>
        /// <returns>
        /// true if the specified <see cref="ITimeSeriesValue"/> is equal to the current <see cref="ChannelValueMeasurement{T}"/>;
        /// otherwise, false.
        /// </returns>
        public bool Equals(ITimeSeriesValue other)
        {
            return (CompareTo(other) == 0);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="ChannelValueMeasurement{T}"/>.</param>
        /// <returns>
        /// true if the specified <see cref="Object"/> is equal to the current <see cref="ChannelValueMeasurement{T}"/>;
        /// otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        public override bool Equals(object obj)
        {
            IMeasurement other = obj as IMeasurement;

            if (other != null)
                return Equals(other);

            return false;
        }

        /// <summary>
        /// Compares the <see cref="ChannelValueMeasurement{T}"/> with an <see cref="ITimeSeriesValue"/>.
        /// </summary>
        /// <param name="other">The <see cref="ITimeSeriesValue"/> to compare with the current <see cref="ChannelValueMeasurement{T}"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <remarks>Measurement implementations should compare by hash code.</remarks>
        public int CompareTo(ITimeSeriesValue other)
        {
            return GetHashCode().CompareTo(other.GetHashCode());
        }

        /// <summary>
        /// Compares the <see cref="ChannelValueMeasurement{T}"/> with the specified <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current <see cref="ChannelValueMeasurement{T}"/>.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj"/> is not an <see cref="IMeasurement"/>.</exception>
        /// <remarks>Measurement implementations should compare by hash code.</remarks>
        public int CompareTo(object obj)
        {
            ITimeSeriesValue other = obj as ITimeSeriesValue;

            if (other != null)
                return CompareTo(other);

            throw new ArgumentException("Measurement can only be compared with other ITimeSeriesValues...");
        }

        /// <summary>
        /// Get the hash code for the <see cref="ChannelValueMeasurement{T}"/>.<see cref="MeasurementKey"/>.
        /// </summary>
        /// <returns>Hash code for the <see cref="ChannelValueMeasurement{T}"/>.<see cref="MeasurementKey"/>.</returns>
        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        #endregion
    }
}