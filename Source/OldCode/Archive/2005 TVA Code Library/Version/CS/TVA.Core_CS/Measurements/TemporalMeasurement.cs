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
using System.Collections.Generic;

namespace TVA.Measurements
{
    /// <summary>This class represents a time constrained measured value.</summary>
    public class TemporalMeasurement : Measurement
    {
        #region [ Members ]

        // Fields
        private double m_lagTime;   // Allowed past time deviation tolerance
        private double m_leadTime;  // Allowed future time deviation tolerance

        #endregion

        #region [ Constructors ]

        public TemporalMeasurement(double lagTime, double leadTime)
            : this(-1, null, double.NaN, 0, lagTime, leadTime)
        {
        }

        public TemporalMeasurement(int id, string source, double value, DateTime timestamp, double lagTime, double leadTime)
            : this(id, source, value, timestamp.Ticks, lagTime, leadTime)
        {
        }

        public TemporalMeasurement(int id, string source, double value, long ticks, double lagTime, double leadTime)
            : base(id, source, value, ticks)
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

        /// <summary>Allowed past time deviation tolerance in seconds (can be subsecond)</summary>
        /// <remarks>
        /// <para>This value defines the time sensitivity to past measurement timestamps.</para>
        /// <para>Defined the number of seconds allowed before assuming a measurement timestamp is too old.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LagTime must be greater than zero, but it can be less than one</exception>
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

        /// <summary>Allowed future time deviation tolerance in seconds (can be subsecond)</summary>
        /// <remarks>
        /// <para>This value defines the time sensitivity to future measurement timestamps.</para>
        /// <para>Defined the number of seconds allowed before assuming a measurement timestamp is too advanced.</para>
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">LeadTime must be greater than zero, but it can be less than one</exception>
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

        /// <summary>Returns numeric adjusted value of this measurement, constrained within specified ticks</summary>
        /// <remarks>
        /// <para>Operation will return NaN if ticks are outside of time deviation tolerances</para>
        /// <para>Note that returned value will be offset by adder and multiplier</para>
        /// </remarks>
        /// <returns>Value offset by adder and multipler (i.e., Value * Multiplier + Adder)</returns>
        public double this[long ticks]
        {
            get
            {
                // We only return a measurement value that is up-to-date...
                if (ticks.TimeIsValid(this.Ticks, m_lagTime, m_leadTime))
                    return base.AdjustedValue;
                else
                    return double.NaN;
            }
        }

        /// <summary>Returns numeric adjusted value of this measurement, constrained within specified timestamp</summary>
        /// <remarks>
        /// <para>Operation will return NaN if ticks are outside of time deviation tolerances</para>
        /// <para>Note that returned value will be offset by adder and multiplier</para>
        /// </remarks>
        /// <returns>Value offset by adder and multipler (i.e., Value * Multiplier + Adder)</returns>
        public double this[DateTime timestamp]
        {
            get
            {
                return this[timestamp.Ticks];
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>Gets numeric value of this measurement, constrained within specified ticks</summary>
        /// <remarks>
        /// <para>Operation will return NaN if ticks are outside of time deviation tolerances</para>
        /// </remarks>
        /// <returns>Raw value of this measurement (i.e., value that is not offset by adder and multiplier)</returns>
        public double GetValue(long ticks)
        {
            // We only return a measurement value that is up-to-date...
            if (ticks.TimeIsValid(this.Ticks, m_lagTime, m_leadTime))
                return base.Value;
            else
                return double.NaN;
        }

        /// <summary>Sets numeric value of this measurement, constrained within specified ticks</summary>
        /// <remarks>
        /// <para>Operation will only store a value that is newer than the cached value</para>
        /// </remarks>
        public void SetValue(long ticks, double value)
        {
            // We only store a value that is is newer than the current value
            if (ticks > this.Ticks)
            {
                base.Value = value;
                this.Ticks = ticks;
            }
        }

        /// <summary>Gets numeric value of this measurement, constrained within specified timestamp</summary>
        /// <remarks>
        /// <para>Operation will return NaN if timestamp is outside of time deviation tolerances</para>
        /// </remarks>
        /// <returns>Raw value of this measurement (i.e., value that is not offset by adder and multiplier)</returns>
        public double GetValue(DateTime timestamp)
        {
            return GetValue(timestamp.Ticks);
        }

        /// <summary>Sets numeric value of this measurement, constrained within specified timestamp</summary>
        /// <remarks>
        /// <para>Operation will only store a value that is newer than the cached value</para>
        /// </remarks>
        public void SetValue(DateTime timestamp, double value)
        {
            SetValue(timestamp.Ticks, value);
        }
        
        #endregion
    }
}