//******************************************************************************************************
//  PerformanceMonitorBase.cs - Gbtc
//
//  Copyright © 2021, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  04/29/2021 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Represents a base class for performance monitors, e.g., the process level <see cref="PerformanceMonitor"/>
    /// and the system level <see cref="SystemPerformanceMonitor"/>.
    /// </summary>
    public abstract class PerformanceMonitorBase : IDisposable, IProvideStatus
    {
        #region [ Members ]

        /// <summary>
        /// Default interval for sampling the <see cref="Counters"/>.
        /// </summary>
        public const double DefaultSamplingInterval = 1000.0D;

        // Fields
        private readonly List<PerformanceCounter> m_counters;
        private readonly Timer m_samplingTimer;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceMonitorBase"/> class.
        /// </summary>
        /// <param name="samplingInterval">Interval, in milliseconds, at which the <see cref="Counters"/> are to be sampled.</param>
        protected PerformanceMonitorBase(double samplingInterval = DefaultSamplingInterval)
        {
            m_counters = new List<PerformanceCounter>();
            m_samplingTimer = new Timer(samplingInterval);
            m_samplingTimer.Elapsed += SamplingTimer_Elapsed;
            m_samplingTimer.Start();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PerformanceMonitor"/> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PerformanceMonitorBase() => 
            Dispose(false);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the interval, in milliseconds, at which the <see cref="Counters"/> are to be sampled.
        /// </summary>
        public double SamplingInterval
        {
            get => m_samplingTimer.Interval;
            set => m_samplingTimer.Interval = value;
        }

        /// <summary>
        /// Gets a read-only list of the <see cref="PerformanceCounter"/> objects monitored by the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public ReadOnlyCollection<PerformanceCounter> Counters
        {
            get
            {
                lock (m_counters)
                    return new ReadOnlyCollection<PerformanceCounter>(m_counters);
            }
        }

        /// <summary>
        /// Gets the friendly name of the <see cref="PerformanceMonitorBase"/> object.
        /// </summary>
        public virtual string Name => 
            $"{GetType().Name}";

        /// <summary>
        /// Gets the current status of the <see cref="PerformanceMonitorBase"/> object.
        /// </summary>
        public virtual string Status
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

                status.AppendLine($"{Environment.NewLine}Statistics calculated using last {PerformanceCounter.DefaultSamplingWindow} counter values sampled every {samplingInterval}.");

                return status.ToString();
            }
        }

        /// <summary>
        /// Gets the lifetime status statistics of the <see cref="PerformanceMonitor"/> object.
        /// </summary>
        public virtual string LifetimeStatus
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

                status.AppendLine($"{Environment.NewLine}Statistics calculated using {sampleCount} counter values sampled every {samplingInterval}.");

                return status.ToString();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PerformanceMonitorBase"/> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PerformanceMonitorBase"/> object and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                if (!disposing)
                    return;

                if (!(m_samplingTimer is null))
                {
                    m_samplingTimer.Elapsed -= SamplingTimer_Elapsed;
                    m_samplingTimer.Dispose();
                }

                lock (m_counters)
                {
                    foreach (PerformanceCounter counter in m_counters)
                        counter.Dispose();

                    m_counters.Clear();
                }
            }
            finally
            {
                m_disposed = true; // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Adds a <see cref="PerformanceCounter"/> to be monitored.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        public void AddCounter(string categoryName, string counterName, string instanceName) =>
            AddCounter(categoryName, counterName, instanceName, counterName);

        /// <summary>
        /// Adds a <see cref="PerformanceCounter"/> to be monitored.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        /// <param name="aliasName">The alias name for the <see cref="PerformanceCounter"/> object.</param>
        public void AddCounter(string categoryName, string counterName, string instanceName, string aliasName) =>
            AddCounter(categoryName, counterName, instanceName, aliasName, PerformanceCounter.DefaultValueUnit);

        /// <summary>
        /// Adds a <see cref="PerformanceCounter"/> to be monitored.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        /// <param name="aliasName">The alias name for the <see cref="PerformanceCounter"/> object.</param>
        /// <param name="valueUnit">The measurement unit for the statistical values of the <see cref="PerformanceCounter"/> object.</param>
        public void AddCounter(string categoryName, string counterName, string instanceName, string aliasName, string valueUnit) =>
            AddCounter(categoryName, counterName, instanceName, aliasName, valueUnit, PerformanceCounter.DefaultValueDivisor);

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
        /// <param name="sampleAdjuster">Defines a custom sample adjustment function for the counter.</param>
        /// <param name="sampleFilter">Defines a custom sample filter function for the counter.</param>
        public void AddCounter(string categoryName, string counterName, string instanceName, string aliasName, string valueUnit, float valueDivisor, bool readOnly = true, Func<float, float> sampleAdjuster = null, Func<float, bool> sampleFilter = null)
        {
            try
            {
                AddCounter(new PerformanceCounter(categoryName, counterName, instanceName, aliasName, valueUnit, valueDivisor, readOnly)
                { 
                    SampleAdjuster = sampleAdjuster,
                    SampleFilter = sampleFilter
                });
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
                m_counters.Add(counter);
        }

        /// <summary>
        /// Removes a <see cref="PerformanceCounter"/> being monitored.
        /// </summary>
        /// <param name="counter">The <see cref="PerformanceCounter"/> object to be unmonitored.</param>
        public void RemoveCounter(PerformanceCounter counter)
        {
            lock (m_counters)
                m_counters.Remove(counter);
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

            return null; // No match found.
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
                    if (string.Compare(counter.BaseCounter.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase) == 0 && 
                        string.Compare(counter.BaseCounter.CounterName, counterName, StringComparison.OrdinalIgnoreCase) == 0)
                        return counter; // Return the match.
                }
            }

            return null; // No match found.
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
                counters.AddRange(m_counters.Where(counter => 
                    string.Compare(counter.BaseCounter.CounterName, counterName, StringComparison.OrdinalIgnoreCase) == 0));
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
                counters.AddRange(m_counters.Where(counter => 
                    string.Compare(counter.BaseCounter.CategoryName, categoryName, StringComparison.OrdinalIgnoreCase) == 0 && 
                    string.Compare(counter.BaseCounter.CounterName, counterName, StringComparison.OrdinalIgnoreCase) == 0));
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
                foreach (PerformanceCounter counter in m_counters)
                    counter.Sample();

                SampleCustomCounters();
            }
        }

        /// <summary>
        /// Derived class should override this method to sample any custom counters.
        /// </summary>
        protected virtual void SampleCustomCounters()
        {
        }

        private void SamplingTimer_Elapsed(object sender, ElapsedEventArgs e) => 
            SampleCounters();

        #endregion
    }
}