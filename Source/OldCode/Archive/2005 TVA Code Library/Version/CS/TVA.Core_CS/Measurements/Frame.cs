using System.Diagnostics;
using System;
using System.Xml.Linq;
using System.Collections;
using Microsoft.VisualBasic;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

//*******************************************************************************************************
//  TVA.Measurements.Frame.vb - Basic frame implementation
//  Copyright © 2006 - TVA, all rights reserved - Gbtc
//
//  Build Environment: VB.NET, Visual Studio 2005
//  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
//      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
//       Phone: 423/751-2827
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  6/22/2006 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace ClassLibrary1
{
	namespace Measurements
	{
		
		/// <summary>Implementation of a basic frame</summary>
		public class Frame : IFrame
		{
			public override int GetHashCode()
			{
				return base.GetHashCode ();
			}
			
			
			
			private long m_ticks;
			private bool m_published;
			private int m_publishedMeasurements;
			private Dictionary<ClassLibrary1.Measurements.MeasurementKey, IMeasurement> m_measurements;
			private long m_startSortTime;
			private long m_lastSortTime;
			private IMeasurement m_lastSortedMeasurement;
			
			public Frame(long ticks)
			{
				
				m_ticks = ticks;
				m_measurements = new Dictionary<MeasurementKey, IMeasurement>(100);
				m_publishedMeasurements = - 1;
				
			}
			
			public Frame(long ticks, Dictionary<New, IMeasurement> measurements, long startSortTime, long lastSortTime)
			{
				
				m_ticks = ticks;
				m_measurements = new Dictionary<MeasurementKey, IMeasurement>(measurements);
				m_startSortTime = startSortTime;
				m_lastSortTime = lastSortTime;
				m_publishedMeasurements = - 1;
				
			}
			
			/// <summary>Create a copy of this frame and its measurements</summary>
			/// <remarks>This frame's measurement dictionary is synclocked during copy</remarks>
			public IFrame Clone()
			{
				
				lock(m_measurements)
				{
					return new Frame(m_ticks, m_measurements, m_startSortTime, m_lastSortTime);
				}
				
			}
			
			/// <summary>Keyed measurements in this frame</summary>
			public IDictionary<Measurements, IMeasurement> Measurements
			{
				get
				{
					return m_measurements;
				}
			}
			
			/// <summary>Gets or sets published state of this frame</summary>
			public bool Published
			{
				get
				{
					return m_published;
				}
				set
				{
					m_published = value;
				}
			}
			
			/// <summary>Gets or sets total number of measurements that have been published for this frame</summary>
			/// <remarks>If this property has not been assigned a value, the property will return measurement count</remarks>
			public int PublishedMeasurements
			{
				get
				{
					if (m_publishedMeasurements == - 1)
					{
						return m_measurements.Count;
					}
					return m_publishedMeasurements;
				}
				set
				{
					m_publishedMeasurements = value;
				}
			}
			
			/// <summary>Exact timestamp of the data represented in this frame</summary>
			/// <remarks>The value of this property represents the number of 100-nanosecond intervals that have elapsed since 12:00:00 midnight, January 1, 0001</remarks>
			public long Ticks
			{
				get
				{
					return m_ticks;
				}
				set
				{
					m_ticks = value;
				}
			}
			
			/// <summary>Date representation of ticks of this frame</summary>
			PublicDateTime Timestamp
			{
				get
				{
					return new DateTime(m_ticks);
				}
			}
			
			/// <summary>Last measurement that was sorted into this frame</summary>
			public IMeasurement LastSortedMeasurement
			{
				get
				{
					return m_lastSortedMeasurement;
				}
				set
				{
					m_lastSortedMeasurement = value;
				}
			}
			
			/// <summary>Returns True if the timestamp of this frame equals the timestamp of the specified other frame</summary>
			public bool Equals(IFrame other)
			{
				
				return (CompareTo(other) == 0);
				
			}
			
			/// <summary>Returns True if the timestamp of this frame equals the timestamp of the specified other frame</summary>
			public override bool Equals(object obj)
			{
				
				IFrame other = obj as IFrame;
				if (other != null)
				{
					return Equals(other);
				}
				throw (new ArgumentException("Object is not an IFrame"));
				
			}
			
			/// <summary>This implementation of a basic measurement compares itself by timestamp</summary>
			public int CompareTo(IFrame other)
			{
				
				return m_ticks.CompareTo(other.Ticks);
				
			}
			
			/// <summary>This implementation of a basic frame compares itself by timestamp</summary>
			public int CompareTo(object obj)
			{
				
				IFrame other = obj as IFrame;
				if (other != null)
				{
					return CompareTo(other);
				}
				throw (new ArgumentException("Frame can only be compared with other IFrames..."));
				
			}
			
			#region " Frame Operators "
			
			public static bool operator ==(Frame frame1, Frame frame2)
			{
				
				return frame1.Equals(frame2);
				
			}
			
			public static bool operator !=(Frame frame1, Frame frame2)
			{
				
				return ! frame1.Equals(frame2);
				
			}
			
			public static bool operator >(Frame frame1, Frame frame2)
			{
				
				return frame1.CompareTo(frame2) > 0;
				
			}
			
			public static bool operator >=(Frame frame1, Frame frame2)
			{
				
				return frame1.CompareTo(frame2) >= 0;
				
			}
			
			public static bool operator <(Frame frame1, Frame frame2)
			{
				
				return frame1.CompareTo(frame2) < 0;
				
			}
			
			public static bool operator <=(Frame frame1, Frame frame2)
			{
				
				return frame1.CompareTo(frame2) <= 0;
				
			}
			
			#endregion
			
		}
		
	}
}
