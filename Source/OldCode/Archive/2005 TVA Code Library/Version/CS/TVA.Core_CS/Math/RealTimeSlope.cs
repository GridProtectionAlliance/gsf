using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Threading;
//using System.Math;
//using TVA.Math.Common;
//using TVA.DateTime.Common;

//*******************************************************************************************************
//  TVA.Math.RealTimeSlope.vb - Calculates slope of real-time data stream using linear regression
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
//  12/8/2004 - J. Ritchie Carroll
//       Generated initial version of source for Real-Time Frequency Monitor.
//  01/24/2006 - J. Ritchie Carroll
//       Integrated into code library.
//  08/23/2007 - Darrell Zuercher
//       Edited code comments.
//
//*******************************************************************************************************


namespace TVA
{
	namespace Math
	{
		
		/// <summary>Calculates slope for a real-time continuous data stream.</summary>
		public class RealTimeSlope
		{
			
			
			private int m_regressionInterval;
			private int m_pointCount;
			private List<double> m_xValues;
			private List<double> m_yValues;
			private double m_slope;
			private double m_lastSlope;
			private DateTime m_slopeRun;
			private bool m_calculating;
			
			public delegate void StatusEventHandler(string message);
			private StatusEventHandler StatusEvent;
			
			public event StatusEventHandler Status
			{
				add
				{
					StatusEvent = (StatusEventHandler) System.Delegate.Combine(StatusEvent, value);
				}
				remove
				{
					StatusEvent = (StatusEventHandler) System.Delegate.Remove(StatusEvent, value);
				}
			}
			
			public delegate void RecalculatedEventHandler();
			private RecalculatedEventHandler RecalculatedEvent;
			
			public event RecalculatedEventHandler Recalculated
			{
				add
				{
					RecalculatedEvent = (RecalculatedEventHandler) System.Delegate.Combine(RecalculatedEvent, value);
				}
				remove
				{
					RecalculatedEvent = (RecalculatedEventHandler) System.Delegate.Remove(RecalculatedEvent, value);
				}
			}
			
			
			/// <summary>
			/// Creates a default instance of the real-time slope calculation class. Must call Initialize before using.
			/// </summary>
			public RealTimeSlope()
			{
				
				
			}
			
			/// <summary>Creates a new instance of the real-time slope calculation class.</summary>
			/// <param name="regressionInterval">Time span over which to calculate slope.</param>
			/// <param name="estimatedRefreshInterval">Estimated data points per second.</param>
			public RealTimeSlope(int regressionInterval, double estimatedRefreshInterval) : this()
			{
				
				Initialize(regressionInterval, estimatedRefreshInterval);
				
			}
			
			/// <summary>Adds a new x, y data pair to continuous data set.</summary>
			/// <param name="x">New x-axis value.</param>
			/// <param name="y">New y-axis value.</param>
			public void Calculate(double x, double y)
			{
				
				lock(m_xValues)
				{
					// Adds latest values to regression data set.
					m_xValues.Add(x);
					m_yValues.Add(y);
					
					// Keeps a constant number of points by removing them from the left.
					while (m_xValues.Count > m_pointCount)
					{
						m_xValues.RemoveAt(0);
					}
					
					while (m_yValues.Count > m_pointCount)
					{
						m_yValues.RemoveAt(0);
					}
				}
				
				if (m_xValues.Count >= m_pointCount && ! m_calculating)
				{
					// Performs curve fit calculation on seperate thread, since it could be time consuming.
					#if ThreadTracking
					object with_1 = TVA.Threading.ManagedThreadPool.QueueUserWorkItem(new System.EventHandler(PerformCalculation));
					with_1.Name = "TVA.Math.RealTimeSlope.PerformCalculation()";
					#else
					ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(PerformCalculation));
					#endif
				}
				
			}
			
			public void Initialize(int regressionInterval, double estimatedRefreshInterval)
			{
				
				m_slopeRun = DateTime.Now;
				m_regressionInterval = regressionInterval;
				m_pointCount = m_regressionInterval * (1 / estimatedRefreshInterval);
				if (m_xValues == null)
				{
					m_xValues = new List<double>;
				}
				else
				{
					lock(m_xValues)
					{
						m_xValues.Clear();
					}
				}
				if (m_yValues == null)
				{
					m_yValues = new List<double>;
				}
				else
				{
					lock(m_yValues)
					{
						m_yValues.Clear();
					}
				}
				
			}
			
			private void PerformCalculation(object state)
			{
				
				try
				{
					m_calculating = true;
					
					double[] xValues;
					double[] yValues;
					
					// Calculations are made against a copy of the current data set to keep lock time on
					// data values down to a minimum. This allows data to be added with minimal delay.
					lock(m_xValues)
					{
						xValues = m_xValues.ToArray();
						yValues = m_yValues.ToArray();
					}
					
					// Takes new values and calculates slope (curve fit for 1st order polynomial).
					m_slope = TVA.Math.Common.CurveFit(1, m_pointCount, xValues, yValues)[1];
				}
				catch (Exception ex)
				{
					if (StatusEvent != null)
						StatusEvent("CurveFit failed: " + ex.Message);
				}
				finally
				{
					m_calculating = false;
				}
				
				if (System.Math.Sign(m_slope) != System.Math.Sign(m_lastSlope))
				{
					m_slopeRun = DateTime.Now;
				}
				m_lastSlope = m_slope;
				
				// Notifies consumer of new calculated slope.
				if (RecalculatedEvent != null)
					RecalculatedEvent();
				
			}
			
			/// <summary>Gets current calculated slope for data set.</summary>
			public double Slope
			{
				get
				{
					return m_slope;
				}
			}
			
			/// <summary>Gets run-time, in seconds, for which slope has maintained a continuous positive or negative
			/// trend.</summary>
			public double RunTime
			{
				get
				{
					return (m_regressionInterval + TVA.DateTime.Common.TicksToSeconds(DateTime.Now.Ticks - m_slopeRun.Ticks));
				}
			}
			
		}
		
	}
	
}
