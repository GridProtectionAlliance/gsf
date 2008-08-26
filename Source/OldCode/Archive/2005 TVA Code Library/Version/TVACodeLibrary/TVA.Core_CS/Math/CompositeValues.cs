using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
//using TVA.Common;

//*******************************************************************************************************
//  TVA.Math.CompositeValues.vb - Composite values class
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
//  11/12/2004 - J. Ritchie Carroll
//       Generated original version of source code.
//  12/29/2005 - Pinal C. Patel
//       Migrated 2.0 version of source code from 1.1 source (TVA.Shared.Math).
//  08/23/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
	namespace Math
	{
		
		/// <summary>Temporarily caches composite values until all values been received, so that a compound value
		/// can be created.</summary>
		public class CompositeValues
		{
			
			
			private struct CompositeValue
			{
				
				public double Value;
				public bool Received;
				
			}
			
			private CompositeValue[] m_compositeValues;
			private bool m_allReceived;
			
			/// <summary>Creates a new instance of the CompositeValues class, specifing the total number of
			/// composite values to track.</summary>
			/// <param name="count">Total number of composite values to track.</param>
			public CompositeValues(int count)
			{
				
				m_compositeValues = TVA.Common.CreateArray<CompositeValue>(count);
				
			}
			
			/// <summary>Gets or sets the composite value at the specified index in composite value collection.</summary>
			/// <param name="index">The zero-based index of the composite value to get or set.</param>
			/// <returns>The composite value at the specified index in composite value collection.</returns>
			public double this[int index]
			{
				get
				{
					return m_compositeValues[index].Value;
				}
				set
				{
					CompositeValue with_1 = m_compositeValues[index];
					with_1.Value = value;
					with_1.Received = true;
				}
			}
			
			/// <summary>Gets a boolean value indicating if composite value at the specified index is received.</summary>
			/// <param name="index">The zero-based index of the composite value.</param>
			/// <returns>True, if composite value at the specified index is received; otherwise, false.</returns>
			public bool Received(int index)
			{
				return m_compositeValues[index].Received;
			}
			
			/// <summary>Gets the number of composite values in the composite value collection.</summary>
			/// <returns>To be provided.</returns>
			public int Count
			{
				get
				{
					return m_compositeValues.Length;
				}
			}
			
			/// <summary>Gets a boolean value indicating if all composite values are received.</summary>
			/// <returns>True, if all composite values are received; otherwise, false.</returns>
			public bool AllReceived
			{
				get
				{
					if (m_allReceived)
					{
						return true;
					}
					else
					{
						bool allValuesReceived = true;
						
						for (int x = 0; x <= m_compositeValues.Length - 1; x++)
						{
							if (! m_compositeValues[x].Received)
							{
								allValuesReceived = false;
								break;
							}
						}
						
						if (allValuesReceived)
						{
							m_allReceived = true;
						}
						return allValuesReceived;
					}
				}
			}
			
		}
		
	}
	
}
