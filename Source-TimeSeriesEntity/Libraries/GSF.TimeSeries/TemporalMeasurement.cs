//******************************************************************************************************
//  TemporalMeasurement.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  10/17/2013 - Stephen C. Wills
//       Added generic type to handle values other than double, necessitating a change in functionality.
//       The GetTemporalStateFlags() method will now return MeasurementStateFlags to indicate whether
//       the timestamp of the temporal measurement has fallen out of range.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a time constrained measured value.
    /// </summary>
    public class TemporalMeasurement<T> : Measurement<T>
    {
        #region [ Members ]

        // Fields
        private double m_lagTime;   // Allowed past time deviation tolerance
        private double m_leadTime;  // Allowed future time deviation tolerance

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="TemporalMeasurement{T}"/> given the specified parameters.
        /// </summary>
        /// <param name="id">The fundamental identifier of the <see cref="TemporalMeasurement{T}"/>.</param>
        /// <param name="timestamp">The exact timestamp, in ticks, of the data represented by this <see cref="Measurement{T}"/>.</param>
        /// <param name="stateFlags">The <see cref="MeasurementStateFlags"/> associated with this <see cref="Measurement{T}"/>.</param>
        /// <param name="value">The raw value of this <see cref="TemporalMeasurement{T}"/>.</param>
        /// <param name="lagTime">Past time deviation tolerance, in seconds - this becomes the amount of time to wait before publishing begins.</param>
        /// <param name="leadTime">Future time deviation tolerance, in seconds - this becomes the tolerated +/- accuracy of the local clock to real-time.</param>
        public TemporalMeasurement(Guid id, Ticks timestamp, MeasurementStateFlags stateFlags, T value, double lagTime, double leadTime)
            : base(id, timestamp, stateFlags, value)
        {
            if (lagTime <= 0.0)
                throw new ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one");

            if (leadTime <= 0.0)
                throw new ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one");

            m_lagTime = lagTime;
            m_leadTime = leadTime;
        }

        /// <summary>
        /// Constructs a new <see cref="TemporalMeasurement{T}"/> given the specified parameters.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement{T}"/> value.</param>
        /// <param name="lagTime">Past time deviation tolerance, in seconds - this becomes the amount of time to wait before publishing begins.</param>
        /// <param name="leadTime">Future time deviation tolerance, in seconds - this becomes the tolerated +/- accuracy of the local clock to real-time.</param>
        /// <exception cref="NullReferenceException"><paramref name="measurement"/> is null</exception>
        public TemporalMeasurement(IMeasurement<T> measurement, double lagTime, double leadTime)
            : this(measurement.ID, measurement.Timestamp, measurement.StateFlags, measurement.Value, lagTime, leadTime)
        {
        }

        #endregion

        #region [ Properties ]

        /// <summary>Allowed past time deviation tolerance in seconds (can be sub-second).</summary>
        /// <remarks>
        /// <para>This value defines the time sensitivity to past measurement timestamps.</para>
        /// <para>Defined the number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one.</exception>
        public double LagTime
        {
            get
            {
                return m_lagTime;
            }
        }

        /// <summary>Allowed future time deviation tolerance in seconds (can be sub-second).</summary>
        /// <remarks>
        /// <para>This value defines the time sensitivity to future measurement timestamps.</para>
        /// <para>Defined the number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one.</exception>
        public double LeadTime
        {
            get
            {
                return m_leadTime;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Gets the <see cref="IMeasurement{T}.StateFlags"/> of this <see cref="TemporalMeasurement{T}"/>,
        /// adding the <see cref="MeasurementStateFlags.BadTime"/> flag if the timestamp is not valid based
        /// on the <see cref="LagTime"/> and <see cref="LeadTime"/>.
        /// </summary>
        /// <param name="currentTime">Timestamp used to constrain <see cref="TemporalMeasurement{T}"/> (typically set to real-time, i.e. "now").</param>
        /// <returns>State flags validated against the current time, lag time, and lead time.</returns>
        public MeasurementStateFlags GetTemporalStateFlags(Ticks currentTime)
        {
            return Timestamp.TimeIsValid(currentTime, m_lagTime, m_leadTime) ? StateFlags : (StateFlags | MeasurementStateFlags.BadTime);
        }

        #endregion
    }
}