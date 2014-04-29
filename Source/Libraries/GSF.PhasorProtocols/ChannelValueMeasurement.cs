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
    public class ChannelValueMeasurement<T> : IMeasurement where T : IChannelDefinition
    {
        #region [ Members ]

        // Fields
        private IChannelValue<T> m_parent;
        private MeasurementKey m_key;
        private MeasurementStateFlags m_stateFlags;
        private bool m_stateFlagsAssigned;
        private string m_tagName;
        private Ticks m_timestamp;
        private Ticks m_receivedTimestamp;
        private Ticks m_publishedTimestamp;
        private int m_valueIndex;
        private double m_adder;
        private double m_multiplier;
        private MeasurementValueFilterFunction m_measurementValueFilter;

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
            m_key = MeasurementKey.Undefined;
            m_timestamp = -1;
            m_receivedTimestamp = DateTime.UtcNow.Ticks;
            m_multiplier = 1.0D;
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
        /// Gets or sets the <see cref="Guid"/> based signal ID of this <see cref="ChannelValueMeasurement{T}"/>, if available.
        /// </summary>
        public Guid ID
        {
            get
            {
                return m_key.SignalID;
            }
        }

        /// <summary>
        /// Gets the primary key (a <see cref="MeasurementKey"/>, of this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        public MeasurementKey Key
        {
            get
            {
                return m_key;
            }
            set
            {
                if ((object)value == null)
                {
                    m_key = MeasurementKey.Undefined;
                }
                else
                {
                    m_key = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets exact timestamp, in ticks, of the data represented by this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        /// <remarks>
        /// This value returns timestamp of parent data frame unless assigned an alternate value.<br/>
        /// The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001.
        /// </remarks>
        public virtual Ticks Timestamp
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
        public virtual Ticks ReceivedTimestamp
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
        public virtual Ticks PublishedTimestamp
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
        /// Gets or sets the text based tag name of this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        public virtual string TagName
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
        /// Gets or sets index into the <see cref="IChannelValue{T}.GetCompositeValue"/> that this measurement derives its value from.
        /// </summary>
        public virtual int ValueIndex
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
        /// Gets or sets the raw measurement value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </summary>
        /// <returns>Raw value of this <see cref="ChannelValueMeasurement{T}"/> (i.e., value that is not offset by <see cref="Adder"/> and <see cref="Multiplier"/>).</returns>
        public virtual double Value
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
        /// Gets the adjusted numeric value of this measurement, taking into account the specified <see cref="Adder"/> and <see cref="Multiplier"/> offsets.
        /// </summary>
        /// <remarks>
        /// Note that returned value will be offset by <see cref="Adder"/> and <see cref="Multiplier"/>.
        /// </remarks>
        /// <returns><see cref="Value"/> offset by <see cref="Adder"/> and <see cref="Multiplier"/> (i.e., <c><see cref="Value"/> * <see cref="Multiplier"/> + <see cref="Adder"/></c>).</returns>
        public virtual double AdjustedValue
        {
            get
            {
                double adjustedValue = m_parent.GetCompositeValue(m_valueIndex) * m_multiplier + m_adder;

                // Convert phase angles to the -180 degrees to 180 degrees range
                if (m_parent is PhasorValueBase && m_valueIndex == (int)CompositePhasorValue.Angle)
                    adjustedValue = Angle.FromDegrees(adjustedValue).ToRange(-Math.PI, false).ToDegrees();

                return adjustedValue;
            }
        }

        /// <summary>
        /// Gets or sets an offset to add to the measurement value. This defaults to 0.0.
        /// </summary>
        public virtual double Adder
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
        /// Defines a multiplicative offset to apply to the measurement value. This defaults to 1.0.
        /// </summary>
        public virtual double Multiplier
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
        /// Gets or sets <see cref="MeasurementStateFlags"/> associated with this <see cref="ChannelValueMeasurement{T}"/>.
        /// </summary>
        public virtual MeasurementStateFlags StateFlags
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

        /// <summary>
        /// Gets or sets function used to apply a down-sampling filter over a sequence of <see cref="IMeasurement"/> values.
        /// </summary>
        public virtual MeasurementValueFilterFunction MeasurementValueFilter
        {
            get
            {
                // If measurement user has assigned a specific filter for this measurement, we use it
                if (m_measurementValueFilter != null)
                    return m_measurementValueFilter;

                // Otherwise we use default filter algorithm as specified by the parent channel value
                return m_parent.GetMeasurementValueFilterFunction(m_valueIndex);
            }
            set
            {
                m_measurementValueFilter = value;
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