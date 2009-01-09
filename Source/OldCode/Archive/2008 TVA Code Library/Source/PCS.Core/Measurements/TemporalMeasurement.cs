//*******************************************************************************************************
//  TemporalMeasurement.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/8/2005 - J. Ritchie Carroll
//       Initial version of source generated.
//  09/17/2008 - James R Carroll
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using PCS.Units;

namespace PCS.Measurements
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
            : this(-1, null, double.NaN, 0, lagTime, leadTime)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="TemporalMeasurement"/> given the specified parameters.
        /// </summary>
        /// <param name="id">Numeric ID of the <see cref="TemporalMeasurement"/>.</param>
        /// <param name="source">Source of the <see cref="TemporalMeasurement"/>(e.g., name of archive).</param>
        /// <param name="value">Value of the <see cref="TemporalMeasurement"/>.</param>
        /// <param name="timestamp">Timestamp of the <see cref="TemporalMeasurement"/>.</param>
        /// <param name="lagTime">Past time deviation tolerance, in seconds - this becomes the amount of time to wait before publishing begins.</param>
        /// <param name="leadTime">Future time deviation tolerance, in seconds - this becomes the tolerated +/- accuracy of the local clock to real-time.</param>
        public TemporalMeasurement(int id, string source, double value, Time timestamp, double lagTime, double leadTime)
            : base(id, source, value, timestamp)
        {
            if (lagTime <= 0)
                throw new ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one");

            if (leadTime <= 0)
                throw new ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one");

            m_lagTime = lagTime;
            m_leadTime = leadTime;
        }

        #endregion

        #region [ Properties ]

        /// <summary>Allowed past time deviation tolerance in seconds (can be subsecond).</summary>
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
                    throw new ArgumentOutOfRangeException("value", "LagTime must be greater than zero, but it can be less than one");

                m_lagTime = value;
            }
        }

        /// <summary>Allowed future time deviation tolerance in seconds (can be subsecond).</summary>
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
                    throw new ArgumentOutOfRangeException("value", "LeadTime must be greater than zero, but it can be less than one");

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
        /// <returns>Value offset by adder and multipler (i.e., Value * Multiplier + Adder).</returns>
        public double GetAdjustedValue(Time timestamp)
        {
            // We only return a measurement value that is up-to-date...
            if (Timestamp.TimeIsValid(timestamp, m_lagTime, m_leadTime))
                return base.AdjustedValue;
            else
                return double.NaN;
        }

        /// <summary>Gets numeric adjusted value of this <see cref="TemporalMeasurement"/>, constrained within specified timestamp.</summary>
        /// <remarks>
        /// <para>Operation will return NaN if ticks are outside of time deviation tolerances.</para>
        /// <para>Note that returned value will be offset by adder and multiplier.</para>
        /// </remarks>
        /// <param name="timestamp">Timestamp used to constrain <see cref="TemporalMeasurement"/> (typically set to real-time, i.e. "now").</param>
        /// <returns>Value offset by adder and multipler (i.e., Value * Multiplier + Adder).</returns>
        public double GetAdjustedValue(DateTime timestamp)
        {
            return GetAdjustedValue(timestamp.Ticks);
        }

        /// <summary>Gets numeric value of this <see cref="TemporalMeasurement"/>, constrained within specified ticks.</summary>
        /// <remarks>
        /// <para>Operation will return NaN if ticks are outside of time deviation tolerances.</para>
        /// </remarks>
        /// <param name="timestamp">Timestamp, in ticks, used to constrain <see cref="TemporalMeasurement"/> (typically set to real-time, i.e. "now").</param>
        /// <returns>Raw value of this measurement (i.e., value that is not offset by adder and multiplier).</returns>
        public double GetValue(Time timestamp)
        {
            // We only return a measurement value that is up-to-date...
            if (Timestamp.TimeIsValid(timestamp, m_lagTime, m_leadTime))
                return base.Value;
            else
                return double.NaN;
        }

        /// <summary>Sets numeric value and timestamp, as ticks, of this <see cref="TemporalMeasurement"/>.</summary>
        /// <remarks>
        /// <para>Operation will only store a value that is newer than the cached value.</para>
        /// </remarks>
        /// <param name="timestamp">New timestamp, in ticks, for <see cref="TemporalMeasurement"/>.</param>
        /// <param name="value">New value for <see cref="TemporalMeasurement"/>, only stored if <paramref name="ticks"/> are newer than current <see cref="Ticks"/>.</param>
        public void SetValue(Time timestamp, double value)
        {
            // We only store a value that is is newer than the current value
            if (timestamp > Timestamp)
            {
                base.Value = value;
                Timestamp = timestamp;
            }
        }
        
        #endregion
    }
}