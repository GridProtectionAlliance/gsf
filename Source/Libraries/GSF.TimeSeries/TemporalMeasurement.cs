//******************************************************************************************************
//  TemporalMeasurement.cs - Gbtc
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
//  09/02/2010 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/20/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;

namespace GSF.TimeSeries
{
    /// <summary>
    /// Represents a time constrained measured value.
    /// </summary>
    public class TemporalMeasurement : Measurement
    {
        #region [ Members ]

        // Fields
        private double m_lagTime;   // Allowed past time deviation tolerance
        private double m_leadTime;  // Allowed future time deviation tolerance

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Constructs a new <see cref="TemporalMeasurement"/> given the specified parameters.
        /// </summary>
        /// <param name="lagTime">Past time deviation tolerance, in seconds - this becomes the amount of time to wait before publishing begins.</param>
        /// <param name="leadTime">Future time deviation tolerance, in seconds - this becomes the tolerated +/- accuracy of the local clock to real-time.</param>
        public TemporalMeasurement(double lagTime, double leadTime)
            : this(null, lagTime, leadTime)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="TemporalMeasurement"/> given the specified parameters.
        /// </summary>
        /// <param name="measurement">Source <see cref="IMeasurement"/> value.</param>
        /// <param name="lagTime">Past time deviation tolerance, in seconds - this becomes the amount of time to wait before publishing begins.</param>
        /// <param name="leadTime">Future time deviation tolerance, in seconds - this becomes the tolerated +/- accuracy of the local clock to real-time.</param>
        public TemporalMeasurement(IMeasurement measurement, double lagTime, double leadTime)
        {
            if (measurement != null)
            {
                Key = measurement.Key;
                Value = measurement.Value;
                Adder = measurement.Adder;
                Multiplier = measurement.Multiplier;
                Timestamp = measurement.Timestamp;
                StateFlags = measurement.StateFlags;
            }

            if (lagTime <= 0)
                throw new ArgumentOutOfRangeException(nameof(lagTime), "lagTime must be greater than zero, but it can be less than one");

            if (leadTime <= 0)
                throw new ArgumentOutOfRangeException(nameof(leadTime), "leadTime must be greater than zero, but it can be less than one");

            m_lagTime = lagTime;
            m_leadTime = leadTime;
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
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "LagTime must be greater than zero, but it can be less than one");

                m_lagTime = value;
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
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "LeadTime must be greater than zero, but it can be less than one");

                m_leadTime = value;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>Gets numeric adjusted value of this <see cref="TemporalMeasurement"/>, constrained within specified ticks.</summary>
        /// <remarks>
        /// <para>Operation will return NaN if ticks are outside of time deviation tolerances.</para>
        /// <para>Note that returned value will be offset by adder and multiplier.</para>
        /// </remarks>
        /// <param name="timestamp">Timestamp used to constrain <see cref="TemporalMeasurement"/> (typically set to real-time, i.e. "now").</param>
        /// <returns>Value offset by adder and multiplier (i.e., Value * Multiplier + Adder).</returns>
        public double GetAdjustedValue(Ticks timestamp)
        {
            // We only return a measurement value that is up-to-date...
            return Timestamp.TimeIsValid(timestamp, m_lagTime, m_leadTime) ? base.AdjustedValue : double.NaN;
        }

        /// <summary>Gets numeric value of this <see cref="TemporalMeasurement"/>, constrained within specified ticks.</summary>
        /// <remarks>
        /// <para>Operation will return NaN if ticks are outside of time deviation tolerances.</para>
        /// </remarks>
        /// <param name="timestamp">Timestamp, in ticks, used to constrain <see cref="TemporalMeasurement"/> (typically set to real-time, i.e. "now").</param>
        /// <returns>Raw value of this measurement (i.e., value that is not offset by adder and multiplier).</returns>
        public double GetValue(Ticks timestamp)
        {
            // We only return a measurement value that is up-to-date...
            return Timestamp.TimeIsValid(timestamp, m_lagTime, m_leadTime) ? base.Value : double.NaN;
        }

        /// <summary>Sets numeric value and timestamp, as ticks, of this <see cref="TemporalMeasurement"/>.</summary>
        /// <remarks>
        /// <para>Operation will only store a value that is newer than the cached value.</para>
        /// </remarks>
        /// <param name="timestamp">New timestamp, in ticks, for <see cref="TemporalMeasurement"/>.</param>
        /// <param name="value">New value for <see cref="TemporalMeasurement"/>, only stored if <paramref name="timestamp"/> are newer than current <see cref="Ticks"/>.</param>
        /// <returns><c>true</c> if value was updated; otherwise <c>false</c>.</returns>
        public bool SetValue(Ticks timestamp, double value)
        {
            // We only store a value that is is newer than the current value
            if (timestamp > Timestamp)
            {
                base.Value = value;
                Timestamp = timestamp;
                return true;
            }

            return false;
        }

        #endregion
    }
}