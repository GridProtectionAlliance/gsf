//*******************************************************************************************************
//  PerformanceMonitor.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcaptel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/01/2007 - Pinal C. Patel
//       Generated original version of source code.
//  08/08/2007 - James R Carroll
//       Added lock contention rate and datagram / sec performance counters.
//  09/04/2007 - Pinal C. Patel
//       Added Status property.
//  09/22/2008 - James R Carroll
//       Converted to C#.
//  10/01/2008 - Pinal C. Patel
//       Entered code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Timers;

namespace TVA.Diagnostics
{
    /// <summary>
    /// A class that can be used to monitor system and process performance similar to the Performance Monitor utility.
    /// </summary>
    /// <seealso cref="PerformaceCounter"/>
    /// <example>
    /// This sample shows how to use <see cref="PerformanceMonitor"/> for monitoring application performance:
    /// <code>
    /// using System;
    /// using System.Threading;
    /// using TVA.Diagnostics;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         PerformanceMonitor perfMon = new PerformanceMonitor();
    ///         while (true)
    ///         {
    ///             // Display process performance.
    ///             Thread.Sleep(5000);
    ///             Console.WriteLine("");
    ///             Console.Write(perfMon.Status);
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class PerformanceMonitor : IDisposable, IStatusProvider
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default interval of sampling the <see cref="Counters"/>.
        /// </summary>
        public const int DefaultSamplingInterval = 1000;

        // Fields
        private string m_processName;
        private List<PerformanceCounter> m_counters;
        private Timer m_samplingTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        public PerformanceMonitor()
            : this(DefaultSamplingInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="samplingInterval">Interval at which the <see cref="Counters"/> are to be sampled.</param>
        public PerformanceMonitor(double samplingInterval)
            : this(Process.GetCurrentProcess().ProcessName, samplingInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="processName">Name of the <see cref="Process"/> whose performance is to be monitored.</param>
        public PerformanceMonitor(string processName)
            : this(processName, DefaultSamplingInterval)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="processName">Name of the <see cref="Process"/> whose performance is to be monitored.</param>
        /// <param name="samplingInterval">Interval at which the <see cref="Counters"/> are to be sampled.</param>
        public PerformanceMonitor(string processName, double samplingInterval)
        {
            m_processName = processName;
            m_counters = new List<PerformanceCounter>();

            // Add default process counters.
            if (PerformanceCounterCategory.Exists("Process"))
            {
                AddCounter("Process", "% Processor Time", m_processName, "CPU Utilization", "Percent / CPU", Environment.ProcessorCount);
                AddCounter("Process", "IO Data Bytes/sec", m_processName, "I/O Data Rate", "Kilobytes / sec", 1024);
                AddCounter("Process", "IO Data Operations/sec", m_processName, "I/O Activity Rate", "Operations / sec", 1);
                AddCounter("Process", "Handle Count", m_processName, "Process Handle Count", "Total Handles", 1);
                AddCounter("Process", "Thread Count", m_processName, "Process Thread Count", "Total Threads", 1);
                AddCounter("Process", "Working Set", m_processName, "Process Memory Usage", "Megabytes", 1048576);
            }

            // Add default networking counters.
            if (PerformanceCounterCategory.Exists("IPv4"))
            {
                AddCounter("IPv4", "Datagrams Sent/sec", "", "Outgoing Packet Rate", "Datagrams / sec", 1);
                AddCounter("IPv4", "Datagrams Received/sec", "", "Incoming Packet Rate", "Datagrams / sec", 1);
            }
            else if (PerformanceCounterCategory.Exists("IP"))
            {
                AddCounter("IP", "Datagrams Sent/sec", "", "Outgoing Packet Rate", "Datagrams / sec", 1);
                AddCounter("IP", "Datagrams Received/sec", "", "Incoming Packet Rate", "Datagrams / sec", 1);
            }

            // Add default .NET counters.
            if (PerformanceCounterCategory.Exists(".NET CLR LocksAndThreads"))
            {
                AddCounter(".NET CLR LocksAndThreads", "Contention Rate / sec", m_processName, "Lock Contention Rate", "Attempts / sec", 1);
            }

            m_samplingTimer = new Timer(samplingInterval);
            m_samplingTimer.Elapsed += m_samplingTimer_Elapsed;
            m_samplingTimer.Start();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PerformanceMonitor"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PerformanceMonitor()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="Process"/> to be monitored.
        /// </summary>
        public string ProcessName
        {
            get
            {
                return m_processName;
            }
            set
            {
                m_processName = value;
                lock (m_counters)
                {
                    foreach (PerformanceCounter counter in m_counters)
                    {
                        // Only update the InstanceName for counters that had it set.
                        if (!string.IsNullOrEmpty(counter.BaseCounter.InstanceName))
                            counter.BaseCounter.InstanceName = m_processName;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the interval at which the <see cref="Counters"/> are to be sampled.
        /// </summary>
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

        /// <summary>
        /// Gets a list of <see cref="PerformanceCounter"/> objects monitored by the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public PerformanceCounter[] Counters
        {
            get 
            {
                lock (m_counters)
                {
                    // Return an array instead of the backing list to prevent it from being updated directly.
                    return m_counters.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the processor utilization of the monitored process.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter CPUUsage
        {
            get
            {
                return FindCounters("% Processor Time");
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the IP based datagrams sent / second of the system.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter DatagramSendRate
        {
            get
            {
                return FindCounters("Datagrams Sent/sec");
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the IP based datagrams received / second of the system.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter DatagramReceiveRate
        {
            get
            {
                return FindCounters("Datagrams Received/sec");
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the .NET threading contention rate / second of the process.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter ThreadingContentionRate
        {
            get
            {
                return FindCounters("Contention Rate / sec");
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the memory utilization of the monitored process.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter MemoryUsage
        {
            get
            {
                return FindCounters("Working Set");
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the rate at which the monitored process is 
        /// issuing bytes to I/O operations that do not involve data such as control operations.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter IOUsage
        {
            get
            {
                return FindCounters("IO Data Bytes/sec");
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the rate at which the monitored process is 
        /// issuing read and write I/O operations.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter IOActivity
        {
            get
            {
                return FindCounters("IO Data Operations/sec");
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the total number of handles currently open by 
        /// the monitored process.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter HandleCount
        {
            get
            {
                return FindCounters("Handle Count");
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the number of threads currently active in the 
        /// monitored process.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter ThreadCount
        {
            get
            {
                return FindCounters("Thread Count");
            }
        }

        /// <summary>
        /// Gets the friendly name of the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// Gets the current status of the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public string Status
        {
            get
            {
                StringBuilder status = new StringBuilder();

                // Status header.
                status.Append("Counter".PadRight(20));
                status.Append(' ');
                status.Append("Last".CenterText(13));
                status.Append(' ');
                status.Append("Average".CenterText(13));
                status.Append(' ');
                status.Append("Maximum".CenterText(13));
                status.Append(' ');
                status.Append("Units".CenterText(16));
                status.AppendLine();
                status.Append(new string('-', 20));
                status.Append(' ');
                status.Append(new string('-', 13));
                status.Append(' ');
                status.Append(new string('-', 13));
                status.Append(' ');
                status.Append(new string('-', 13));
                status.Append(' ');
                status.Append(new string('-', 16));
                status.AppendLine();

                lock (m_counters)
                {
                    foreach (PerformanceCounter counter in m_counters)
                    {
                        // Counter status.
                        status.Append(counter.AliasName.PadLeft(20));
                        status.Append(' ');
                        status.Append(counter.LastValue.ToString("0.00").CenterText(13));
                        status.Append(' ');
                        status.Append(counter.AverageValue.ToString("0.00").CenterText(13));
                        status.Append(' ');
                        status.Append(counter.MaximumValue.ToString("0.00").CenterText(13));
                        status.Append(' ');
                        status.Append(counter.ValueUnit);
                        status.AppendLine();
                    }
                }

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PerformanceMonitor"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if (m_samplingTimer != null)
                        {
                            m_samplingTimer.Elapsed -= m_samplingTimer_Elapsed;
                            m_samplingTimer.Dispose();
                        }
                        m_samplingTimer = null;

                        lock (m_counters)
                        {
                            foreach (PerformanceCounter counter in m_counters)
                            {
                                counter.Dispose();
                            }
                        }
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Adds a <see cref="PerformanceCounter"/> to be monitored.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        public void AddCounter(string categoryName, string counterName, string instanceName)
        {
            AddCounter(categoryName, counterName, instanceName, counterName);
        }

        /// <summary>
        /// Adds a <see cref="PerformanceCounter"/> to be monitored.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        /// <param name="aliasName">The alias name for the <see cref="PerformanceCounter"/> object.</param>
        public void AddCounter(string categoryName, string counterName, string instanceName, string aliasName)
        {
            AddCounter(categoryName, counterName, instanceName, aliasName, PerformanceCounter.DefaultValueUnit);
        }

        /// <summary>
        /// Adds a <see cref="PerformanceCounter"/> to be monitored.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        /// <param name="aliasName">The alias name for the <see cref="PerformanceCounter"/> object.</param>
        /// <param name="valueUnit">The measurement unit for the statistical values of the <see cref="PerformanceCounter"/> object.</param>
        public void AddCounter(string categoryName, string counterName, string instanceName, string aliasName, string valueUnit)
        {
            AddCounter(categoryName, counterName, instanceName, aliasName, valueUnit, PerformanceCounter.DefaultValueDivisor);
        }

        /// <summary>
        /// Adds a <see cref="PerformanceCounter"/> to be monitored.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        /// <param name="aliasName">The alias name for the <see cref="PerformanceCounter"/> object.</param>
        /// <param name="valueUnit">The measurement unit for the statistical values of the <see cref="PerformanceCounter"/> object.</param>
        /// <param name="valueDivisor">The divisor to be applied to the statistical values of the <see cref="PerformanceCounter"/> object.</param>
        public void AddCounter(string categoryName, string counterName, string instanceName, string aliasName, string valueUnit, float valueDivisor)
        {
            AddCounter(new PerformanceCounter(categoryName, counterName, instanceName, aliasName, valueUnit, valueDivisor));
        }

        /// <summary>
        /// Adds a <see cref="PerformanceCounter"/> to be monitored.
        /// </summary>
        /// <param name="counter">The <see cref="PerformanceCounter"/> object to be monitored.</param>
        public void AddCounter(PerformanceCounter counter)
        {
            lock (m_counters)
            {
                m_counters.Add(counter);
            }
        }

        /// <summary>
        /// Returns a <see cref="PerformanceCounter"/> object matching the specified counter name.
        /// </summary>
        /// <param name="counterName">Name of the <see cref="PerformanceCounter"/> to be retrieved.</param>
        /// <returns>A <see cref="PerformanceCounter"/> object if a match is found; otherwise null.</returns>
        public PerformanceCounter FindCounters(string counterName)
        {
            lock (m_counters)
            {
                foreach (PerformanceCounter counter in m_counters)
                {
                    if (string.Compare(counter.BaseCounter.CounterName, counterName, true) == 0)
                        return counter; // Return the match.
                }
            }

            return null;    // No match found.
        }

        private void m_samplingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (m_counters)
            {
                foreach (PerformanceCounter counter in m_counters)
                {
                    counter.Sample();
                }
            }
        }

        #endregion
    }
}