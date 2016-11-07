//******************************************************************************************************
//  PerformanceMonitor.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  06/01/2007 - Pinal C. Patel
//       Generated original version of source code.
//  08/08/2007 - J. Ritchie Carroll
//       Added lock contention rate and datagram / sec performance counters.
//  09/04/2007 - Pinal C. Patel
//       Added Status property.
//  09/22/2008 - J. Ritchie Carroll
//       Converted to C#.
//  10/01/2008 - Pinal C. Patel
//       Entered code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  06/21/2010 - Stephen C. Wills
//       Fixed issue with monitor not disposing of counters properly.
//  01/03/2011 - J. Ritchie Carroll
//       Added counters for CLR memory consumption, IPv6 incoming / outgoing rates and lifetime status.
//  01/04/2011 - J. Ritchie Carroll
//       Made addition of default counters optional in case user wants a custom monitor.
//       Added new and reorganized default counters.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
#if !MONO
using System.Threading;
#endif
using System.Timers;
using GSF.Units;
using Timer = System.Timers.Timer;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Represents a process performance monitor that operates similar to the Windows Performance Monitor utility
    /// that can be used to monitor system performance.
    /// </summary>
    /// <example>
    /// This example shows how to use <see cref="PerformanceMonitor"/> for monitoring application performance:
    /// <code>
    /// using System;
    /// using System.Threading;
    /// using GSF.Diagnostics;
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
    /// <seealso cref="PerformanceCounter"/>
    public class PerformanceMonitor : IDisposable, IProvideStatus
    {
        #region [ Members ]

        // Constants
#if !MONO
        /// <summary>
        /// Name of the custom thread pool counters category.
        /// </summary>
        public const string ThreadPoolCountersCategoryName = "GSF Thread Pool Counters";
#endif

        /// <summary>
        /// Default interval for sampling the <see cref="Counters"/>.
        /// </summary>
        public const double DefaultSamplingInterval = 1000.0D;

        // Fields
        private string m_processName;
        private readonly List<PerformanceCounter> m_counters;
        private readonly Timer m_samplingTimer;
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

#if MONO
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="samplingInterval">Interval, in milliseconds, at which the <see cref="Counters"/> are to be sampled.</param>
        /// <param name="addDefaultCounters">Set to <c>true</c> to add default counters; otherwise <c>false</c>.</param>
        // Process based performance counters on Mono are initialized via current process ID
        public PerformanceMonitor(double samplingInterval, bool addDefaultCounters = true)
            : this(Process.GetCurrentProcess().Id.ToString(), samplingInterval, addDefaultCounters)
        {
        }
#else
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="samplingInterval">Interval, in milliseconds, at which the <see cref="Counters"/> are to be sampled.</param>
        /// <param name="addDefaultCounters">Set to <c>true</c> to add default counters; otherwise <c>false</c>.</param>
        public PerformanceMonitor(double samplingInterval, bool addDefaultCounters = true)
            : this(Process.GetCurrentProcess().ProcessName, samplingInterval, addDefaultCounters)
        {
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="processName">Name of the <see cref="Process"/> whose performance is to be monitored.</param>
        /// <param name="addDefaultCounters">Set to <c>true</c> to add default counters; otherwise <c>false</c>.</param>
        public PerformanceMonitor(string processName, bool addDefaultCounters = true)
            : this(processName, DefaultSamplingInterval, addDefaultCounters)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitor"/> class.
        /// </summary>
        /// <param name="processName">Name of the <see cref="Process"/> whose performance is to be monitored.</param>
        /// <param name="samplingInterval">Interval, in milliseconds, at which the <see cref="Counters"/> are to be sampled.</param>
        /// <param name="addDefaultCounters">Set to <c>true</c> to add default counters; otherwise <c>false</c>.</param>
        public PerformanceMonitor(string processName, double samplingInterval, bool addDefaultCounters = true)
        {
            if ((object)processName == null)
                throw new ArgumentNullException(nameof(processName));

            m_processName = processName;
            m_counters = new List<PerformanceCounter>();

            if (addDefaultCounters)
            {
                // Add default process and .NET counters
                AddCounter("Process", "% Processor Time", m_processName, "CPU Utilization", "Average % / CPU", Environment.ProcessorCount);
#if !MONO
                AddCounter("Process", "IO Data Bytes/sec", m_processName, "I/O Data Rate", "Kilobytes / sec", SI2.Kilo);
                AddCounter("Process", "IO Data Operations/sec", m_processName, "I/O Activity Rate", "Operations / sec", 1);
                AddCounter("Process", "Handle Count", m_processName, "Process Handle Count", "Total Handles", 1);
#endif
                AddCounter("Process", "Thread Count", m_processName, "Process Thread Count", "System Threads", 1);
                AddCounter(".NET CLR LocksAndThreads", "# of current logical Threads", m_processName, "CLR Thread Count", "Managed Threads", 1);

#if MONO
                // Add Mono thread pool counters
                AddCounter("Mono Threadpool", "# of Threads", m_processName, "Worker Threads", "Active in Pool", 1);
                AddCounter("Mono Threadpool", "# of IO Threads", m_processName, "I/O Port Threads", "Active in Pool", 1);
#else
                if (PerformanceCounterCategory.Exists(ThreadPoolCountersCategoryName))
                {
                    // Add custom thread pool counters                                                             1234567890123456
                    AddCounter(ThreadPoolCountersCategoryName, "Worker Threads", m_processName, "Worker Threads", "Active in Pool", 1, false);
                    //                                                                                    12345678901234567890
                    AddCounter(ThreadPoolCountersCategoryName, "Completion Port Threads", m_processName, "I/O Port Threads", "Active in Pool", 1, false);
                }
#endif

                AddCounter(".NET CLR LocksAndThreads", "Current Queue Length", m_processName, "Thread Queue Size", "Waiting Threads", 1);
                AddCounter(".NET CLR LocksAndThreads", "Contention Rate / sec", m_processName, "Lock Contention Rate", "Attempts / sec", 1);

                AddCounter("Process", "Working Set", m_processName, "Process Memory Usage", "Megabytes", SI2.Mega);

                AddCounter(".NET CLR Memory", "# Bytes in all Heaps", m_processName, "CLR Memory Usage", "Megabytes", SI2.Mega);
                AddCounter(".NET CLR Memory", "Large Object Heap size", m_processName, "Large Object Heap", "Megabytes", SI2.Mega);

                //                                                                                         1234567890123456
                AddCounter(".NET CLR Exceptions", "# of Exceps Thrown", m_processName, "Exception Count", "Total Exceptions", 1);

#if MONO
                AddCounter(".NET CLR Exceptions", "# of Exceps Thrown/Sec", m_processName, "Exception Rate", "Exceptions / sec", 1);
#else
                AddCounter(".NET CLR Exceptions", "# of Exceps Thrown / sec", m_processName, "Exception Rate", "Exceptions / sec", 1);
#endif

                // Add default networking counters
#if MONO
                PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");

                foreach (string instance in category.GetInstanceNames())
                {
                    //  12345678901234567890
                    // "IP Outgoing (eth0)"
                    // "IP Incoming (eth0)"
                    AddCounter("Network Interface", "Bytes Sent/sec", instance, string.Format("IP Outgoing ({0})", instance).TruncateRight(20), "Bytes / sec", 1);
                    AddCounter("Network Interface", "Bytes Received/sec", instance, string.Format("IP Incoming ({0})", instance).TruncateRight(20), "Bytes / sec", 1);
                }
#else
                if (PerformanceCounterCategory.Exists("IPv4"))
                {
                    //                                            12345678901234567890
                    AddCounter("IPv4", "Datagrams Sent/sec", "", "IPv4 Outgoing Rate", "Datagrams / sec", 1);
                    //                                                12345678901234567890
                    AddCounter("IPv4", "Datagrams Received/sec", "", "IPv4 Incoming Rate", "Datagrams / sec", 1);
                }
                else if (PerformanceCounterCategory.Exists("IP"))
                {
                    AddCounter("IP", "Datagrams Sent/sec", "", "IP Outgoing Rate", "Datagrams / sec", 1);
                    AddCounter("IP", "Datagrams Received/sec", "", "IP Incoming Rate", "Datagrams / sec", 1);
                }

                if (PerformanceCounterCategory.Exists("IPv6"))
                {
                    AddCounter("IPv6", "Datagrams Sent/sec", "", "IPv6 Outgoing Rate", "Datagrams / sec", 1);
                    AddCounter("IPv6", "Datagrams Received/sec", "", "IPv6 Incoming Rate", "Datagrams / sec", 1);
                }
#endif
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
        /// Gets or sets the interval, in milliseconds, at which the <see cref="Counters"/> are to be sampled.
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
        /// Gets a read-only list of the <see cref="PerformanceCounter"/> objects monitored by the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public ReadOnlyCollection<PerformanceCounter> Counters
        {
            get
            {
                lock (m_counters)
                {
                    return new ReadOnlyCollection<PerformanceCounter>(m_counters);
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
                return FindCounter("% Processor Time");
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the IP based datagrams sent / second of the system.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter IPDataSendRate
        {
            get
            {
                try
                {
                    PerformanceCounter[] sources;
#if MONO
                    sources = FindCounters("Bytes Sent/sec");
#else
                    sources = FindCounters("Datagrams Sent/sec");
#endif
                    if ((object)sources != null && sources.Length > 0)
                        return new PerformanceCounter(sources);

                    return null;
                }
                catch
                {
                    // Not failing if performance counter cannot be created
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the IP based datagrams received / second of the system.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter IPDataReceiveRate
        {
            get
            {
                try
                {
                    PerformanceCounter[] sources;
#if MONO
                    sources = FindCounters("Bytes Received/sec");
#else
                    sources = FindCounters("Datagrams Received/sec");
#endif
                    if ((object)sources != null && sources.Length > 0)
                        return new PerformanceCounter(sources);

                    return null;
                }
                catch
                {
                    // Not failing if performance counter cannot be created
                    return null;
                }
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
                return FindCounter("Contention Rate / sec");
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
                return FindCounter("Working Set");
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
                return FindCounter("IO Data Bytes/sec");
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
                return FindCounter("IO Data Operations/sec");
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
                return FindCounter("Handle Count");
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
                return FindCounter("Thread Count");
            }
        }

        /// <summary>
        /// Gets the friendly name of the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public string Name
        {
            get
            {
                return string.Format("{0}.{1}", this.GetType().Name, m_processName);
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

                //          1         2         3         4         5         6         7         8
                // 12345678901234567890123456789012345678901234567890123456789012345678901234567890
                // Statistics calculated using last 500 counter values sampled every 12.5 seconds.

                string samplingInterval = "second";

                if (m_samplingTimer.Interval != 1000.0D)
                    samplingInterval = (m_samplingTimer.Interval / 1000.0D).ToString("0.0") + " seconds";

                status.AppendFormat("{0}Statistics calculated using last {1} counter values sampled every {2}.{3}", Environment.NewLine, PerformanceCounter.DefaultSamplingWindow, samplingInterval, Environment.NewLine);

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the lifetime status statistics of the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public string LifetimeStatus
        {
            get
            {
                StringBuilder status = new StringBuilder();
                long sampleCount = 0;

                // Status header.
                status.Append("Counter".PadRight(20));
                status.Append(' ');
                //             1234567890123
                status.Append("Lifetime Max.".CenterText(13));
                status.Append(' ');
                //             1234567890123
                status.Append("Lifetime Avg.".CenterText(13));
                status.Append(' ');
                //             1234567890123
                status.Append("Inv(Scalar)".CenterText(13));
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
                        status.Append(counter.LifetimeMaximumValue.ToString("0.00").CenterText(13));
                        status.Append(' ');
                        status.Append(counter.LifetimeAverageValue.ToString("0.00").CenterText(13));
                        status.Append(' ');
                        status.Append(counter.ValueDivisor.ToString().CenterText(13));
                        status.Append(' ');
                        status.Append(counter.ValueUnit);
                        status.AppendLine();

                        if (sampleCount == 0)
                            sampleCount = counter.LifetimeSampleCount;
                    }
                }

                //          1         2         3         4         5         6         7         8
                // 12345678901234567890123456789012345678901234567890123456789012345678901234567890
                // Statistics calculated using 121878905 counter values sampled every 5.0 seconds.

                string samplingInterval = "second";

                if (m_samplingTimer.Interval != 1000.0D)
                    samplingInterval = (m_samplingTimer.Interval / 1000.0D).ToString("0.0") + " seconds";

                status.AppendFormat("{0}Statistics calculated using {1} counter values sampled every {2}.{3}", Environment.NewLine, sampleCount, samplingInterval, Environment.NewLine);

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
                        if ((object)m_samplingTimer != null)
                        {
                            m_samplingTimer.Elapsed -= m_samplingTimer_Elapsed;
                            m_samplingTimer.Dispose();
                        }

                        lock (m_counters)
                        {
                            foreach (PerformanceCounter counter in m_counters)
                            {
                                counter.Dispose();
                            }

                            m_counters.Clear();
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
        /// <param name="readOnly">Flag that determines if this counter is read-only.</param>
        public void AddCounter(string categoryName, string counterName, string instanceName, string aliasName, string valueUnit, float valueDivisor, bool readOnly = true)
        {
            try
            {
                AddCounter(new PerformanceCounter(categoryName, counterName, instanceName, aliasName, valueUnit, valueDivisor, readOnly));
            }
            catch
            {
                // Performance counter may not exist...
            }
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
        /// Removes a <see cref="PerformanceCounter"/> being monitored.
        /// </summary>
        /// <param name="counter">The <see cref="PerformanceCounter"/> object to be unmonitored.</param>
        public void RemoveCounter(PerformanceCounter counter)
        {
            lock (m_counters)
            {
                m_counters.Remove(counter);
            }
        }

        /// <summary>
        /// Returns a <see cref="PerformanceCounter"/> object matching the specified counter name.
        /// </summary>
        /// <param name="counterName">Name of the <see cref="PerformanceCounter"/> to be retrieved.</param>
        /// <returns>A <see cref="PerformanceCounter"/> object if a match is found; otherwise null.</returns>
        /// <remarks>
        /// First <see cref="PerformanceCounter"/> with matching name is returned. If same name exists within
        /// multiple monitored categories, use <see cref="FindCounter(string,string)"/> overload instead.
        /// </remarks>
        public PerformanceCounter FindCounter(string counterName)
        {
            lock (m_counters)
            {
                foreach (PerformanceCounter counter in m_counters)
                {
                    if (string.Compare(counter.BaseCounter.CounterName, counterName, StringComparison.OrdinalIgnoreCase) == 0)
                        return counter; // Return the match.
                }
            }

            return null;    // No match found.
        }

        /// <summary>
        /// Returns a <see cref="PerformanceCounter"/> object matching the specified counter name.
        /// </summary>
        /// <param name="categoryName">Category of the <see cref="PerformanceCounter"/> to be retrieved.</param>
        /// <param name="counterName">Name of the <see cref="PerformanceCounter"/> to be retrieved.</param>
        /// <returns>A <see cref="PerformanceCounter"/> object if a match is found; otherwise null.</returns>
        public PerformanceCounter FindCounter(string categoryName, string counterName)
        {
            lock (m_counters)
            {
                foreach (PerformanceCounter counter in m_counters)
                {
                    if (string.Compare(counter.BaseCounter.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(counter.BaseCounter.CounterName, counterName, StringComparison.OrdinalIgnoreCase) == 0)
                        return counter; // Return the match.
                }
            }

            return null;    // No match found.
        }

        /// <summary>
        /// Returns <see cref="PerformanceCounter"/> array matching the specified counter name.
        /// </summary>
        /// <param name="counterName">Name of the <see cref="PerformanceCounter"/> to be retrieved.</param>
        /// <returns>A <see cref="PerformanceCounter"/> array of found matches, if any.</returns>
        public PerformanceCounter[] FindCounters(string counterName)
        {
            List<PerformanceCounter> counters = new List<PerformanceCounter>();

            lock (m_counters)
            {
                foreach (PerformanceCounter counter in m_counters)
                {
                    if (string.Compare(counter.BaseCounter.CounterName, counterName, StringComparison.OrdinalIgnoreCase) == 0)
                        counters.Add(counter);
                }
            }

            return counters.ToArray();
        }

        /// <summary>
        /// Returns <see cref="PerformanceCounter"/> array matching the specified counter name.
        /// </summary>
        /// <param name="categoryName">Category of the <see cref="PerformanceCounter"/> to be retrieved.</param>
        /// <param name="counterName">Name of the <see cref="PerformanceCounter"/> to be retrieved.</param>
        /// <returns>A <see cref="PerformanceCounter"/> array of found matches, if any.</returns>
        public PerformanceCounter[] FindCounters(string categoryName, string counterName)
        {
            List<PerformanceCounter> counters = new List<PerformanceCounter>();

            lock (m_counters)
            {
                foreach (PerformanceCounter counter in m_counters)
                {
                    if (string.Compare(counter.BaseCounter.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase) == 0 && string.Compare(counter.BaseCounter.CounterName, counterName, StringComparison.OrdinalIgnoreCase) == 0)
                        counters.Add(counter);
                }
            }

            return counters.ToArray();
        }

        /// <summary>
        /// Sample all defined counters.
        /// </summary>
        public void SampleCounters()
        {
            lock (m_counters)
            {
#if !MONO
                // Sample custom thread pool counters (these already exist in Mono)
                PerformanceCounter workerThreadsCounter = FindCounter(ThreadPoolCountersCategoryName, "Worker Threads");
                PerformanceCounter completionPortThreadsCounter = FindCounter(ThreadPoolCountersCategoryName, "Completion Port Threads");

                if ((object)workerThreadsCounter != null && (object)completionPortThreadsCounter != null)
                {
                    System.Diagnostics.PerformanceCounter workerThreads = workerThreadsCounter.BaseCounter;
                    System.Diagnostics.PerformanceCounter completionPortThreads = completionPortThreadsCounter.BaseCounter;

                    if ((object)workerThreads != null && (object)completionPortThreads != null)
                    {
                        int maximumWorkerThreads, maximumCompletionPortThreads, availableWorkerThreads, availableCompletionPortThreads;

                        ThreadPool.GetMaxThreads(out maximumWorkerThreads, out maximumCompletionPortThreads);
                        ThreadPool.GetAvailableThreads(out availableWorkerThreads, out availableCompletionPortThreads);

                        workerThreads.RawValue = maximumWorkerThreads - availableWorkerThreads;
                        completionPortThreads.RawValue = maximumCompletionPortThreads - availableCompletionPortThreads;
                    }
                }
#endif

                foreach (PerformanceCounter counter in m_counters)
                {
                    counter.Sample();
                }
            }
        }

        private void m_samplingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SampleCounters();
        }

        #endregion

        #region [ Static ]

#if !MONO
        // Static Constructor
        static PerformanceMonitor()
        {
            try
            {
                if (!PerformanceCounterCategory.Exists(ThreadPoolCountersCategoryName))
                {
                    CounterCreationDataCollection customPerformanceCounters = new CounterCreationDataCollection();

                    // Create custom counter objects for thread pool monitoring
                    CounterCreationData workerThreadCounter = new CounterCreationData();
                    workerThreadCounter.CounterName = "Worker Threads";
                    workerThreadCounter.CounterHelp = "Active worker threads in the thread pool";
                    workerThreadCounter.CounterType = PerformanceCounterType.NumberOfItems32;

                    CounterCreationData completionPortThreadCounter = new CounterCreationData();
                    completionPortThreadCounter.CounterName = "Completion Port Threads";
                    completionPortThreadCounter.CounterHelp = "Active completion port threads in the thread pool";
                    completionPortThreadCounter.CounterType = PerformanceCounterType.NumberOfItems32;

                    // Add custom counter objects to CounterCreationDataCollection
                    customPerformanceCounters.Add(workerThreadCounter);
                    customPerformanceCounters.Add(completionPortThreadCounter);

                    // Bind the counters to the PerformanceCounterCategory
                    PerformanceCounterCategory.Create(ThreadPoolCountersCategoryName, "Application thread pool counters", PerformanceCounterCategoryType.MultiInstance, customPerformanceCounters);
                }
            }
            catch
            {
                // Not failing if custom counters cannot be created
            }
        }
#endif

        #endregion
    }
}