using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.DateTime.Common;

//*******************************************************************************************************
//  TVA.Measurements.TemporalMeasurement.vb - Time sensitive measurement implementation
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  This class represents a time constrained measured value
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/8/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace TVA
{
	namespace Measurements
	{
		
		public class TemporalMeasurement : Measurement
		{
			
			
			
			private double m_lagTime; // Allowed past time deviation tolerance
			private double m_leadTime; // Allowed future time deviation tolerance
			
			public TemporalMeasurement(double lagTime, double leadTime) : this(- 1, null, double.NaN, 0, lagTime, leadTime)
			{


			}
			
			public TemporalMeasurement(int id, string source, double value, DateTime timestamp, double lagTime, double leadTime) : this(id, source, value, timestamp.Ticks, lagTime, leadTime)
			{
				
				
			}
			
			public TemporalMeasurement(int id, string source, double value, long ticks, double lagTime, double leadTime) : base(id, source, value, ticks)
			{
				
				
				if (lagTime <= 0)
				{
					throw (new ArgumentOutOfRangeException("lagTime", "lagTime must be greater than zero, but it can be less than one"));
				}
				if (leadTime <= 0)
				{
					throw (new ArgumentOutOfRangeException("leadTime", "leadTime must be greater than zero, but it can be less than one"));
				}
				
				m_lagTime = lagTime;
				m_leadTime = leadTime;
				
			}
			
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
					{
						throw (new ArgumentOutOfRangeException("value", "LagTime must be greater than zero, but it can be less than one"));
					}
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
					{
						throw (new ArgumentOutOfRangeException("value", "LeadTime must be greater than zero, but it can be less than one"));
					}
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
					if (TVA.DateTime.Common.TimeIsValid(ticks, this.Ticks, m_lagTime, m_leadTime))
					{
						return base.AdjustedValue;
					}
					else
					{
						return double.NaN;
					}
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
			
			/// <summary>Gets or sets numeric value of this measurement, constrained within specified ticks</summary>
			/// <remarks>
			/// <para>Get operation will return NaN if ticks are outside of time deviation tolerances</para>
			/// <para>Set operation will only store a value that is newer than the cached value</para>
			/// </remarks>
			/// <returns>Raw value of this measurement (i.e., value that is not offset by adder and multiplier)</returns>
			public double Value(long ticks)
			{
				// We only return a measurement value that is up-to-date...
				if (TVA.DateTime.Common.TimeIsValid(ticks, this.Ticks, m_lagTime, m_leadTime))
				{
					return base.Value;
				}
				else
				{
					return double.NaN;
				}
			}
			public void SetValue(long ticks, double value)
			{
				// We only store a value that is is newer than the current value
				if (ticks > this.Ticks)
				{
					base.Value = value;
					this.Ticks = ticks;
				}
			}
			
			/// <summary>Gets or sets numeric value of this measurement, constrained within specified timestamp</summary>
			/// <remarks>
			/// <para>Get operation will return NaN if timestamp is outside of time deviation tolerances</para>
			/// <para>Set operation will only store a value that is newer than the cached value</para>
			/// </remarks>
			/// <returns>Raw value of this measurement (i.e., value that is not offset by adder and multiplier)</returns>
			public double Value(DateTime timestamp)
			{
				return this.Value(timestamp.Ticks);
			}
			public void SetValue(DateTime timestamp, double value)
			{
				this.SetValue(timestamp.Ticks, value);
			}
			
		}
		
	}
	
}
