using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Threading;

// 06/04/2007


namespace TVA
{
	namespace Diagnostics
	{
		
		public class PerformanceCounter : IDisposable
		{
			
			
			#region " Member Declaration "
			
			private float m_lastValue;
			private float m_minimumValue;
			private float m_maximumValue;
			private int m_averagingWindow;
			
			private System.Diagnostics.PerformanceCounter m_counter;
			private List<double> m_counterValues;
			
			#endregion
			
			#region " Code Scope: Public "
			
			public const int DefaultAveragingWindow = 120;
			
			public PerformanceCounter(string categoryName, string counterName, string instanceName)
			{
				
				m_minimumValue = float.MaxValue;
				m_maximumValue = float.MinValue;
				m_averagingWindow = DefaultAveragingWindow;
				m_counter = new System.Diagnostics.PerformanceCounter(categoryName, counterName, instanceName);
				m_counterValues = new List<double>();
				
			}
			
			public int AveragingWindow
			{
				get
				{
					return m_averagingWindow;
				}
				set
				{
					if (value > 0)
					{
						Interlocked.Exchange(ref m_averagingWindow, value);
					}
					else
					{
						throw (new ArgumentOutOfRangeException("AveragingWindow", "Value must be greater than 0."));
					}
				}
			}
			
			public float LastValue
			{
				get
				{
					return m_lastValue;
				}
			}
			
			public float MinimumValue
			{
				get
				{
					return m_minimumValue;
				}
			}
			
			public float MaximumValue
			{
				get
				{
					return m_maximumValue;
				}
			}
			
			public float AverageValue
			{
				get
				{
					lock(m_counterValues)
					{
						return Convert.ToSingle(Math.Common.Average(m_counterValues));
					}
				}
			}
			
			public System.Diagnostics.PerformanceCounter BaseCounter
			{
				get
				{
					return m_counter;
				}
			}
			
			public void Sample()
			{
				
				try
				{
					lock(m_counter)
					{
						Interlocked.Exchange(ref m_lastValue, m_counter.NextValue());
					}
					
					if (m_lastValue < m_minimumValue)
					{
						Interlocked.Exchange(ref m_minimumValue, m_lastValue);
					}
					if (m_lastValue > m_maximumValue)
					{
						Interlocked.Exchange(ref m_maximumValue, m_lastValue);
					}
					
					lock(m_counterValues)
					{
						m_counterValues.Add(m_lastValue);
						while (m_counterValues.Count > m_averagingWindow)
						{
							m_counterValues.RemoveAt(0);
						}
					}
				}
				catch (InvalidOperationException)
				{
					// If we're monitoring performance of an application that's not running (it was not running to begin with,
					// or it was running but it no longer running), we'll encounter an InvalidOperationException exception.
					// In this case we'll reset the values and absorb the exception.
					Reset();
				}
				
			}
			
			public void Reset()
			{
				
				Interlocked.Exchange(ref m_lastValue, 0);
				Interlocked.Exchange(ref m_minimumValue, 0);
				Interlocked.Exchange(ref m_maximumValue, 0);
				
				lock(m_counterValues)
				{
					m_counterValues.Clear();
				}
				
			}
			
			#endregion
			
			private bool disposedValue = false; // To detect redundant calls
			
			protected virtual void Dispose(bool disposing)
			{
				if (! this.disposedValue)
				{
					if (disposing)
					{
						Reset();
						lock(m_counter)
						{
							m_counter.Dispose();
						}
					}
				}
				this.disposedValue = true;
			}
			
			#region " IDisposable Support "
			// This code added by Visual Basic to correctly implement the disposable pattern.
			public void Dispose()
			{
				// Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
				Dispose(true);
				GC.SuppressFinalize(this);
			}
			#endregion
			
		}
		
	}
}
