using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;

//*******************************************************************************************************
//  TVA.Measurements.MeasurementKey.vb - Defines primary key elements for a measurement
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
//  12/8/2005 - J. Ritchie Carroll
//       Initial version of source generated
//
//*******************************************************************************************************

namespace TVA
{
	namespace Measurements
	{
		
		/// <summary>Defines a primary key for a measurement</summary>
		public struct MeasurementKey
		{
			
			
			private int m_id;
			private string m_source;
			private int m_hashCode;
			
			public MeasurementKey(int id, string source)
			{
				
				if (string.IsNullOrEmpty(source))
				{
					throw (new ArgumentNullException("source", "MeasurementKey source cannot be null"));
				}
				
				m_id = id;
				m_source = source.ToUpper();
				GenHashCode();
				
			}
			
			public int ID
			{
				get
				{
					return m_id;
				}
				set
				{
					if (m_id != value)
					{
						m_id = value;
						GenHashCode();
					}
				}
			}
			
			public string Source
			{
				get
				{
					return m_source;
				}
				set
				{
					if (string.Compare(m_source, value, true) != 0)
					{
						m_source = value.ToUpper();
						GenHashCode();
					}
				}
			}
			
			public override string ToString()
			{
				
				return string.Format("{0}:{1}", m_source, m_id);
				
			}
			
			public override int GetHashCode()
			{
				
				return m_hashCode;
				
			}
			
			public bool Equals(MeasurementKey other)
			{
				
				return (m_hashCode == other.GetHashCode());
				
			}
			
			public override bool Equals(object obj)
			{
				
				// Can't use TryCast on a structure...
				if (obj is MeasurementKey)
				{
					return Equals((MeasurementKey) obj);
				}
				throw (new ArgumentException("Object is not a MeasurementKey"));
				
			}
			
			public int CompareTo(MeasurementKey other)
			{
				
				int sourceCompare = string.Compare(m_source, other.Source);
				
				if (sourceCompare == 0)
				{
					if (m_id < other.ID)
					{
						return - 1;
					}
					else if (m_id > other.ID)
					{
						return 1;
					}
					else
					{
						return 0;
					}
				}
				else
				{
					return sourceCompare;
				}
				
			}
			
			public int CompareTo(object obj)
			{
				
				// Can't use TryCast on a structure...
				if (obj is MeasurementKey)
				{
					return CompareTo((MeasurementKey) obj);
				}
				throw (new ArgumentException("Object is not a MeasurementKey"));
				
			}
			
			private void GenHashCode()
			{
				
				// We cache hash code during construction or after element value change to speed structure usage
				m_hashCode = (m_source + m_id.ToString()).GetHashCode();
				
			}
			
			#region " MeasurementKey Operators "
			
			public static bool operator ==(MeasurementKey key1, MeasurementKey key2)
			{
				
				return key1.Equals(key2);
				
			}
			
			public static bool operator !=(MeasurementKey key1, MeasurementKey key2)
			{
				
				return ! key1.Equals(key2);
				
			}
			
			public static bool operator >(MeasurementKey key1, MeasurementKey key2)
			{
				
				return key1.CompareTo(key2) > 0;
				
			}
			
			public static bool operator >=(MeasurementKey key1, MeasurementKey key2)
			{
				
				return key1.CompareTo(key2) >= 0;
				
			}
			
			public static bool operator <(MeasurementKey key1, MeasurementKey key2)
			{
				
				return key1.CompareTo(key2) < 0;
				
			}
			
			public static bool operator <=(MeasurementKey key1, MeasurementKey key2)
			{
				
				return key1.CompareTo(key2) <= 0;
				
			}
			
			#endregion
			
		}
		
	}
}
