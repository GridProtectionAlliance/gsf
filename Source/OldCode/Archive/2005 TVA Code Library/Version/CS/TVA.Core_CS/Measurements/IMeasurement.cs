using System.Diagnostics;
using System;
using System.Xml.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

//*******************************************************************************************************
//  TVA.Measurements.IMeasurement.vb - Abstract measurement interface
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  This interface abstractly represents a value measured at an exact time interval
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  12/8/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************


namespace ClassLibrary1
{
	namespace Measurements
	{
		
		/// <summary>Abstract measured value interface</summary>
		public interface IMeasurement : IEquatable<IMeasurement>,IComparable<IMeasurement>,IComparable
		{
			
			
			/// <summary>Gets or sets the numeric ID of this measurement</summary>
			/// <remarks>
			/// <para>In most implementations, this will be a required field</para>
			/// <para>Note that this field, in addition to Source, typically creates the primary key for a measurement</para>
			/// </remarks>
			int ID{
				get;
				set;
			}
			
			/// <summary>Gets or sets the source of this measurement</summary>
			/// <remarks>
			/// <para>In most implementations, this will be a required field</para>
			/// <para>Note that this field, in addition to ID, typically creates the primary key for a measurement</para>
			/// <para>This value is typically used to track the archive name in which measurement is stored</para>
			/// </remarks>
			string Source{
				get;
				set;
			}
			
			/// <summary>Returns the primary key of this measurement</summary>
			ClassLibrary1.Measurements.IMeasurement.ID.Source.Key Key{
				get;
			}
			
			/// <summary>Gets or sets the text based tag name of this measurement</summary>
			string TagName{
				get;
				set;
			}
			
			/// <summary>Gets or sets the raw value of this measurement (i.e., the numeric value that is not offset by adder and multiplier)</summary>
			double Value{
				get;
				set;
			}
			
			/// <summary>Returns the adjusted numeric value of this measurement, taking into account the specified adder and multiplier offsets</summary>
			/// <remarks>
			/// <para>Implementors need to account for adder and multiplier in return value, e.g.:</para>
			/// <code>Return Value * Multiplier + Adder</code>
			/// </remarks>
			double AdjustedValue{
				get;
			}
			
			/// <summary>Defines an offset to add to the measurement value</summary>
			/// <remarks>Implementors should make sure this value defaults to zero</remarks>
			[DefaultValue(0.0)]double Adder{
				get;
				set;
			}
			
			/// <summary>Defines a mulplicative offset to add to the measurement value</summary>
			/// <remarks>Implementors should make sure this value defaults to one</remarks>
			[DefaultValue(1.0)]double Multiplier{
				get;
				set;
			}
			
			/// <summary>Gets or sets exact timestamp of the data represented by this measurement</summary>
			/// <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
			long Ticks{
				get;
				set;
			}
			
			/// <summary>Date representation of ticks of this measurement</summary>
			DateTime Timestamp{
				get;
			}
			
			/// <summary>Determines if the quality of the numeric value of this measurement is good</summary>
			bool ValueQualityIsGood{
				get;
				set;
			}
			
			/// <summary>Determines if the quality of the timestamp of this measurement is good</summary>
			bool TimestampQualityIsGood{
				get;
				set;
			}
			
		}
		
	}
	
}
