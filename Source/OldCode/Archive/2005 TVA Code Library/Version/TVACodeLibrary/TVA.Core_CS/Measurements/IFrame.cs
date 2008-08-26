using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Measurements.IFrame.vb - Abstract frame interface
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  This interface represents a keyed collection of measurements for a given timestamp
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
		
		public interface IFrame : IEquatable<IFrame>,IComparable<IFrame>
		{
			
			
			/// <summary>Handy instance reference to self</summary>
			IFrame This{
				get;
			}
			
			/// <summary>Create a copy of this frame and its measurements</summary>
			/// <remarks>Implementors should synclock frame's measurement dictionary during copy</remarks>
			IFrame Clone();
			
			/// <summary>Keyed measurements in this frame</summary>
			/// <remarks>Represents a dictionary of measurements, keyed by measurement key</remarks>
			IDictionary<MeasurementKey, IMeasurement> Measurements_Renamed{
				get;
			}
			
			/// <summary>Gets or sets published state of this frame</summary>
			bool Published{
				get;
				set;
			}
			
			/// <summary>Gets or sets total number of measurements that have been pubilshed for this frame</summary>
			/// <remarks>If this property has not been assigned a value, implementors should return measurement count</remarks>
			int PublishedMeasurements{
				get;
				set;
			}
			
			/// <summary>Exact timestamp of the data represented in this frame</summary>
			/// <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
			long Ticks{
				get;
				set;
			}
			
			/// <summary>Date representation of ticks of this frame</summary>
			DateTime Timestamp{
				get;
			}
			
			/// <summary>Ticks of when first measurement was sorted into this frame</summary>
			/// <remarks>
			/// <para>This value is used to calculate total required sort time for the frame</para>
			/// <para>Implementors need only track the value</para>
			/// </remarks>
			long StartSortTime{
				get;
				set;
			}
			
			/// <summary>Ticks of when last measurement was sorted into this frame</summary>
			/// <remarks>
			/// <para>This value is used to calculate total required sort time for the frame</para>
			/// <para>Implementors need only track the value</para>
			/// </remarks>
			long LastSortTime{
				get;
				set;
			}
			
			/// <summary>Last measurement that was sorted into this frame</summary>
			/// <remarks>
			/// <para>This value is used to help monitor slow moving measurements that are being sorted into the frame</para>
			/// <para>Implementors need only track the value</para>
			/// </remarks>
			IMeasurement LastSortedMeasurement{
				get;
				set;
			}
			
		}
		
	}
	
}
