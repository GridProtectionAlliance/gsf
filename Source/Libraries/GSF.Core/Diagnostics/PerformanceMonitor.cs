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
using System.Diagnostics;
#if !MONO
using System.Threading;
#endif
using GSF.Units;

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
    public class PerformanceMonitor : PerformanceMonitorBase
    {
        #region [ Members ]

        // Constants
    #if !MONO
        /// <summary>
        /// Name of the custom thread pool counters category.
        /// </summary>
        public const string ThreadPoolCountersCategoryName = "GSF Thread Pool Counters";
    #endif

        // Fields
        private string m_processName;

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
        /// <param name="samplingInterval">Interval, in milliseconds, at which the <see cref="PerformanceMonitorBase.Counters"/> are to be sampled.</param>
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
        /// <param name="samplingInterval">Interval, in milliseconds, at which the <see cref="PerformanceMonitorBase.Counters"/> are to be sampled.</param>
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
        /// <param name="samplingInterval">Interval, in milliseconds, at which the <see cref="PerformanceMonitorBase.Counters"/> are to be sampled.</param>
        /// <param name="addDefaultCounters">Set to <c>true</c> to add default counters; otherwise <c>false</c>.</param>
        public PerformanceMonitor(string processName, double samplingInterval, bool addDefaultCounters = true)
            : base(samplingInterval)
        {
            m_processName = processName ?? throw new ArgumentNullException(nameof(processName));

            if (addDefaultCounters)
            {
                // Add default process and .NET counters
                AddCounter("Process", "% Processor Time", m_processName, "Process CPU Usage", "Average % / CPU", Environment.ProcessorCount);
            #if !MONO
                AddCounter("Process", "IO Data Bytes/sec", m_processName, "I/O Data Rate", "Kilobytes / sec", SI2.Kilo);
                AddCounter("Process", "IO Data Operations/sec", m_processName, "I/O Activity Rate", "Operations / sec", 1);
                AddCounter("Process", "Handle Count", m_processName, "Process Handle Count", "Total Handles", 1);
            #endif
                AddCounter("Process", "Thread Count", m_processName, "Process Thread Count", "System Threads", 1);
                AddCounter(".NET CLR LocksAndThreads", "# of current logical Threads", m_processName, "CLR Thread Count", "Managed Threads", 1, true, 
                           sample => sample > int.MaxValue ? uint.MaxValue - sample : sample);

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
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PerformanceMonitor"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PerformanceMonitor() => 
            Dispose(false);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the name of the <see cref="Process"/> to be monitored.
        /// </summary>
        public string ProcessName
        {
            get => m_processName;
            set
            {
                m_processName = value;

                foreach (PerformanceCounter counter in Counters)
                {
                    // Only update the InstanceName for counters that had it set.
                    if (!string.IsNullOrEmpty(counter.BaseCounter.InstanceName))
                        counter.BaseCounter.InstanceName = m_processName;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the processor utilization of the monitored process.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter CPUUsage =>
            FindCounter("% Processor Time");

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
                #if MONO
                    PerformanceCounter[] sources = FindCounters("Bytes Sent/sec");
                #else
                    PerformanceCounter[] sources = FindCounters("Datagrams Sent/sec");
                #endif

                    return sources is null || sources.Length == 0 ? null : new PerformanceCounter(sources);
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
                #if MONO
                    PerformanceCounter[] sources = FindCounters("Bytes Received/sec");
                #else
                    PerformanceCounter[] sources = FindCounters("Datagrams Received/sec");
                #endif

                    return sources is null || sources.Length <= 0 ? null : new PerformanceCounter(sources);
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
        public PerformanceCounter ThreadingContentionRate => 
            FindCounter("Contention Rate / sec");

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the memory utilization of the monitored process.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter MemoryUsage => 
            FindCounter("Working Set");

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the rate at which the monitored process is 
        /// issuing bytes to I/O operations that do not involve data such as control operations.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter IOUsage => 
            FindCounter("IO Data Bytes/sec");

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the rate at which the monitored process is 
        /// issuing read and write I/O operations.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter IOActivity => 
            FindCounter("IO Data Operations/sec");

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the total number of handles currently open by 
        /// the monitored process.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter HandleCount => 
            FindCounter("Handle Count");

        /// <summary>
        /// Gets the <see cref="PerformanceCounter"/> that monitors the number of threads currently active in the 
        /// monitored process.
        /// </summary>
        /// <remarks>This <see cref="PerformanceCounter"/> is added by default.</remarks>
        public PerformanceCounter ThreadCount => 
            FindCounter("Thread Count");

        /// <summary>
        /// Gets the friendly name of the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public override string Name => 
            $"{base.Name}.{m_processName}";

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handle sampling of custom counters.
        /// </summary>
        protected override void SampleCustomCounters()
        {

        #if !MONO
            // Sample custom thread pool counters (these already exist in Mono)
            PerformanceCounter workerThreadsCounter = FindCounter(ThreadPoolCountersCategoryName, "Worker Threads");
            PerformanceCounter completionPortThreadsCounter = FindCounter(ThreadPoolCountersCategoryName, "Completion Port Threads");

            if (!(workerThreadsCounter is null) && !(completionPortThreadsCounter is null))
            {
                System.Diagnostics.PerformanceCounter workerThreads = workerThreadsCounter.BaseCounter;
                System.Diagnostics.PerformanceCounter completionPortThreads = completionPortThreadsCounter.BaseCounter;

                if (!(workerThreads is null) && !(completionPortThreads is null))
                {
                    ThreadPool.GetMaxThreads(out int maximumWorkerThreads, out int maximumCompletionPortThreads);
                    ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out int availableCompletionPortThreads);

                    workerThreads.RawValue = maximumWorkerThreads - availableWorkerThreads;
                    completionPortThreads.RawValue = maximumCompletionPortThreads - availableCompletionPortThreads;
                }
            }
        #endif
        }

        #endregion

        #region [ Static ]

    #if !MONO
        // Static Constructor
        static PerformanceMonitor()
        {
            try
            {
                if (PerformanceCounterCategory.Exists(ThreadPoolCountersCategoryName))
                    return;

                CounterCreationDataCollection customPerformanceCounters = new CounterCreationDataCollection();

                // Create custom counter objects for thread pool monitoring
                CounterCreationData workerThreadCounter = new CounterCreationData
                {
                    CounterName = "Worker Threads",
                    CounterHelp = "Active worker threads in the thread pool",
                    CounterType = PerformanceCounterType.NumberOfItems32
                };

                CounterCreationData completionPortThreadCounter = new CounterCreationData
                {
                    CounterName = "Completion Port Threads",
                    CounterHelp = "Active completion port threads in the thread pool", 
                    CounterType = PerformanceCounterType.NumberOfItems32
                };

                // Add custom counter objects to CounterCreationDataCollection
                customPerformanceCounters.Add(workerThreadCounter);
                customPerformanceCounters.Add(completionPortThreadCounter);

                // Bind the counters to the PerformanceCounterCategory
                PerformanceCounterCategory.Create
                (
                    ThreadPoolCountersCategoryName, 
                    "Application thread pool counters", 
                    PerformanceCounterCategoryType.MultiInstance, 
                    customPerformanceCounters
                );
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