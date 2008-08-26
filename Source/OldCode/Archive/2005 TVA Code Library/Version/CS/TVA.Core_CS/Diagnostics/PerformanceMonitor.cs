using System.Diagnostics;
using System.Linq;
using System.Data;
using System.Collections;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System;
using System.Text;
//using TVA.Text.Common;
using System.Threading;

// 06/01/2007
// JRC: 08/08/2007 - Added lock contention rate and datagram / sec performance counters...
// PCP: 09/04/2007 - Added Status property


namespace TVA
{
	namespace Diagnostics
	{
		
		public class PerformanceMonitor : IDisposable
		{
			
			
			#region " Member Declaration "
			
			private string m_processName;
			private List<PerformanceCounter> m_counters;
			
			private System.Timers.Timer m_samplingTimer;
			
			#endregion
			
			#region " Code Scope: Public "
			
			public const int DefaultSamplingInterval = 1000;
			
			public PerformanceMonitor() : this(DefaultSamplingInterval)
			{
				
				
			}
			
			public PerformanceMonitor(double samplingInterval) : this(Process.GetCurrentProcess().ProcessName, samplingInterval)
			{
				
				
			}
			
			public PerformanceMonitor(string processName) : this(processName, DefaultSamplingInterval)
			{
				
				
			}
			
			public PerformanceMonitor(string processName, double samplingInterval)
			{
				
				m_processName = processName;
				m_counters = new List<PerformanceCounter>;
				
				if (System.Diagnostics.PerformanceCounterCategory.Exists("Process"))
				{
					m_counters.Add(new PerformanceCounter("Process", "% Processor Time", m_processName));
					m_counters.Add(new PerformanceCounter("Process", "IO Data Bytes/sec", m_processName));
					m_counters.Add(new PerformanceCounter("Process", "IO Data Operations/sec", m_processName));
					m_counters.Add(new PerformanceCounter("Process", "Handle Count", m_processName));
					m_counters.Add(new PerformanceCounter("Process", "Thread Count", m_processName));
					m_counters.Add(new PerformanceCounter("Process", "Working Set", m_processName));
				}
				
				if (System.Diagnostics.PerformanceCounterCategory.Exists("IPv4"))
				{
					m_counters.Add(new PerformanceCounter("IPv4", "Datagrams Sent/sec", ""));
					m_counters.Add(new PerformanceCounter("IPv4", "Datagrams Received/sec", ""));
				}
				else if (System.Diagnostics.PerformanceCounterCategory.Exists("IP"))
				{
					m_counters.Add(new PerformanceCounter("IP", "Datagrams Sent/sec", ""));
					m_counters.Add(new PerformanceCounter("IP", "Datagrams Received/sec", ""));
				}
				
				if (System.Diagnostics.PerformanceCounterCategory.Exists(".NET CLR LocksAndThreads"))
				{
					m_counters.Add(new PerformanceCounter(".NET CLR LocksAndThreads", "Contention Rate / sec", m_processName));
				}
				
				m_samplingTimer = new System.Timers.Timer(samplingInterval);
				m_samplingTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_samplingTimer_Elapsed);
				m_samplingTimer.Start();
				
			}
			
			public string ProcessName
			{
				get
				{
					return m_processName;
				}
				set
				{
					m_processName = value;
					lock(m_counters)
					{
						foreach (PerformanceCounter counter in m_counters)
						{
							lock(counter.BaseCounter)
							{
								counter.BaseCounter.InstanceName = m_processName;
							}
						}
					}
				}
			}
			
			public double SamplingInterval
			{
				get
				{
					return m_samplingTimer.Interval;
				}
				set
				{
					m_samplingTimer.Interval = value;
				}
			}
			
			public List<PerformanceCounter> Counters
			{
				get
				{
					return m_counters;
				}
			}
			
			public PerformanceCounter Counters(string counterName)
			{
				PerformanceCounter match = null;
				lock(m_counters)
				{
					foreach (PerformanceCounter counter in m_counters)
					{
						lock(counter.BaseCounter)
						{
							if (string.Compare(counter.BaseCounter.CounterName, counterName, true) == 0)
							{
								match = counter;
								break;
							}
						}
					}
				}
				return match;
			}
			
			/// <summary>
			/// Gets the counter that monitors the processor utilization of the process.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The TVA.Diagnostics.PerformanceCounter instance that monitors the processor utilization of the process.
			/// </returns>
			public PerformanceCounter CPUUsage
			{
				get
				{
					return Counters("% Processor Time");
				}
			}
			
			/// <summary>
			/// Gets the counter that monitors the IP based datagrams sent / second of the system.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The TVA.Diagnostics.PerformanceCounter instance that monitors the IP based datagrams sent / second of the system.
			/// </returns>
			public PerformanceCounter DatagramSendRate
			{
				get
				{
					return Counters("Datagrams Sent/sec");
				}
			}
			
			/// <summary>
			/// Gets the counter that monitors the IP based datagrams received / second of the system.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The TVA.Diagnostics.PerformanceCounter instance that monitors the IP based datagrams received / second of the system.
			/// </returns>
			public PerformanceCounter DatagramReceiveRate
			{
				get
				{
					return Counters("Datagrams Received/sec");
				}
			}
			
			/// <summary>
			/// Gets the counter that monitors the .NET threading contention rate / second of the process.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The TVA.Diagnostics.PerformanceCounter instance that monitors the .NET threading contention rate / second of the process.
			/// </returns>
			public PerformanceCounter ThreadingContentionRate
			{
				get
				{
					return Counters("Contention Rate / sec");
				}
			}
			
			/// <summary>
			/// Gets the counter that monitors the memory utilization of the process.
			/// </summary>
			/// <value></value>
			/// <returns>
			/// The TVA.Diagnostics.PerformanceCounter instance that monitors the memory utilization of the process.
			/// </returns>
			public PerformanceCounter MemoryUsage
			{
				get
				{
					return Counters("Working Set");
				}
			}
			
			public PerformanceCounter IOUsage
			{
				get
				{
					return Counters("IO Data Bytes/sec");
				}
			}
			
			public PerformanceCounter IOActivity
			{
				get
				{
					return Counters("IO Data Operations/sec");
				}
			}
			
			public PerformanceCounter HandleCount
			{
				get
				{
					return Counters("Handle Count");
				}
			}
			
			public PerformanceCounter ThreadCount
			{
				get
				{
					return Counters("Thread Count");
				}
			}
			
			public string Status
			{
				get
				{
					int processorCount = System.Environment.ProcessorCount;
					PerformanceCounter counter;
					
					System.Text.StringBuilder with_1 = new StringBuilder();
					with_1.Append("Counter".PadRight(20));
					with_1.Append(' ');
					with_1.Append(TVA.Text.Common.CenterText("Last", 13));
					with_1.Append(' ');
					with_1.Append(TVA.Text.Common.CenterText("Average", 13));
					with_1.Append(' ');
					with_1.Append(TVA.Text.Common.CenterText("Maximum", 13));
					with_1.Append(' ');
					with_1.Append(TVA.Text.Common.CenterText("Units", 16));
					with_1.AppendLine();
					with_1.Append(new string('-', 20));
					with_1.Append(' ');
					with_1.Append(new string('-', 13));
					with_1.Append(' ');
					with_1.Append(new string('-', 13));
					with_1.Append(' ');
					with_1.Append(new string('-', 13));
					with_1.Append(' ');
					with_1.Append(new string('-', 16));
					with_1.AppendLine();
					
					counter = CPUUsage;
					
					if (counter != null)
					{
						//        12345678901234567890
						with_1.Append("CPU Utilization".PadLeft(20));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText((counter.LastValue / processorCount).ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText((counter.AverageValue / processorCount).ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText((counter.MaximumValue / processorCount).ToString("0.00"), 13));
						with_1.Append(' ');
						//        1234567890123456
						with_1.Append("Percent / CPU");
						with_1.AppendLine();
					}
					
					counter = MemoryUsage;
					
					if (counter != null)
					{
						//        12345678901234567890
						with_1.Append("Process Memory Usage".PadLeft(20));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText((counter.LastValue / 1048576).ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText((counter.AverageValue / 1048576).ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText((counter.MaximumValue / 1048576).ToString("0.00"), 13));
						with_1.Append(' ');
						//        1234567890123456
						with_1.Append("Megabytes");
						with_1.AppendLine();
					}
					
					counter = HandleCount;
					
					if (counter != null)
					{
						//        12345678901234567890
						with_1.Append("Process Handle Count".PadLeft(20));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.LastValue.ToString(), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.AverageValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.MaximumValue.ToString(), 13));
						with_1.Append(' ');
						//        1234567890123456
						with_1.Append("Total Handles");
						with_1.AppendLine();
					}
					
					counter = ThreadCount;
					
					if (counter != null)
					{
						//        12345678901234567890
						with_1.Append("Process Thread Count".PadLeft(20));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.LastValue.ToString(), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.AverageValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.MaximumValue.ToString(), 13));
						with_1.Append(' ');
						//        1234567890123456
						with_1.Append("Total Threads");
						with_1.AppendLine();
					}
					
					counter = IOUsage;
					
					if (counter != null)
					{
						//        12345678901234567890
						with_1.Append("I/O Data Rate".PadLeft(20));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText((counter.LastValue / 1024).ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText((counter.AverageValue / 1024).ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText((counter.MaximumValue / 1024).ToString("0.00"), 13));
						with_1.Append(' ');
						//        1234567890123456
						with_1.Append("Kilobytes / sec");
						with_1.AppendLine();
					}
					
					counter = IOActivity;
					
					if (counter != null)
					{
						//        12345678901234567890
						with_1.Append("I/O Activity Rate".PadLeft(20));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.LastValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.AverageValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.MaximumValue.ToString("0.00"), 13));
						with_1.Append(' ');
						//        1234567890123456
						with_1.Append("Operations / sec");
						with_1.AppendLine();
					}
					
					counter = DatagramReceiveRate;
					
					if (counter != null)
					{
						//        12345678901234567890
						with_1.Append("Incoming Packet Rate".PadLeft(20));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.LastValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.AverageValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.MaximumValue.ToString("0.00"), 13));
						with_1.Append(' ');
						//        1234567890123456
						with_1.Append("Datagrams / sec");
						with_1.AppendLine();
					}
					
					counter = DatagramSendRate;
					
					if (counter != null)
					{
						//        12345678901234567890
						with_1.Append("Outgoing Packet Rate".PadLeft(20));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.LastValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.AverageValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.MaximumValue.ToString("0.00"), 13));
						with_1.Append(' ');
						//        1234567890123456
						with_1.Append("Datagrams / sec");
						with_1.AppendLine();
					}
					
					counter = ThreadingContentionRate;
					
					if (counter != null)
					{
						//        12345678901234567890
						with_1.Append("Lock Contention Rate".PadLeft(20));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.LastValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.AverageValue.ToString("0.00"), 13));
						with_1.Append(' ');
						with_1.Append(TVA.Text.Common.CenterText(counter.MaximumValue.ToString("0.00"), 13));
						with_1.Append(' ');
						//        1234567890123456
						with_1.Append("Attempts / sec");
						with_1.AppendLine();
					}
					
					return with_1.ToString();
				}
			}
			
			#endregion
			
			#region " Code Scope: Private "
			
			private void m_samplingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
			{
				
				lock(m_counters)
				{
					foreach (PerformanceCounter counter in m_counters)
					{
						counter.Sample();
					}
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
						m_samplingTimer.Dispose();
						lock(m_counters)
						{
							foreach (PerformanceCounter counter in m_counters)
							{
								counter.Dispose();
							}
							m_counters.Clear();
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
