//******************************************************************************************************
//  PerformanceCounter.cs - Gbtc
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
//  06/04/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/22/2008 - J. Ritchie Carroll
//       Converted to C#.
//  09/30/2008 - Pinal C. Patel
//       Entered code comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  06/21/2010 - Stephen C. Wills
//       Fixed issue with counters not disposing properly.
//  01/03/2011 - J. Ritchie Carroll
//       Added total lifetime counter statistics.
//  01/04/2011 - J. Ritchie Carroll
//       Fixed issue with application of value divisor on lifetime statistics.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Represents an extension of the basic <see cref="System.Diagnostics.PerformanceCounter"/> providing additional statistical logic.
    /// </summary>
    /// <example>
    /// This example shows how to create a performance counter for processor utilization:
    /// <code>
    /// using System;
    /// using System.Threading;
    /// using GSF.Diagnostics;
    ///
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         PerformanceCounter counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
    ///         while (true)
    ///         {
    ///             Thread.Sleep(1000);
    ///             counter.Sample();
    ///             Console.WriteLine(string.Format("Last value: {0}", counter.LastValue));
    ///             Console.WriteLine(string.Format("Minimum value: {0}", counter.MinimumValue));
    ///             Console.WriteLine(string.Format("Maximum value: {0}", counter.MaximumValue));
    ///             Console.WriteLine(string.Format("Average value: {0}", counter.AverageValue));
    ///             Console.WriteLine(new string('-', 30));
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class PerformanceCounter : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default measurement unit of the statistical values.
        /// </summary>
        public const string DefaultValueUnit = "Unknown";

        /// <summary>
        /// Default divisor to be applied to the statistical value.
        /// </summary>
        public const float DefaultValueDivisor = 1.0F;

        /// <summary>
        /// Default number of samples over which statistical values are to be calculated.
        /// </summary>
        public const int DefaultSamplingWindow = 120;

        // Fields
        private string m_aliasName;
        private string m_valueUnit;
        private float m_valueDivisor;
        private int m_samplingWindow;
        private System.Diagnostics.PerformanceCounter m_counter;
        private readonly object m_samplesLock;
        private readonly List<float> m_samples;
        private float m_lifetimeMaximum;
        private decimal m_lifetimeTotal;
        private long m_lifetimeSampleCount;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCounter"/> class.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        public PerformanceCounter(string categoryName, string counterName, string instanceName)
            : this(categoryName, counterName, instanceName, counterName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCounter"/> class.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        /// <param name="aliasName">The alias name for the <see cref="PerformanceCounter"/> object.</param>
        public PerformanceCounter(string categoryName, string counterName, string instanceName, string aliasName)
            : this(categoryName, counterName, instanceName, aliasName, DefaultValueUnit)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCounter"/> class.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        /// <param name="aliasName">The alias name for the <see cref="PerformanceCounter"/> object.</param>
        /// <param name="valueUnit">The measurement unit for the statistical values of the <see cref="PerformanceCounter"/> object.</param>
        public PerformanceCounter(string categoryName, string counterName, string instanceName, string aliasName, string valueUnit)
            : this(categoryName, counterName, instanceName, aliasName, valueUnit, DefaultValueDivisor)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceCounter"/> class.
        /// </summary>
        /// <param name="categoryName">The name of the performance counter category (performance object) with which this performance counter is associated.</param>
        /// <param name="counterName">The name of the performance counter.</param>
        /// <param name="instanceName">The name of the performance counter category instance, or an empty string (""), if the category contains a single instance.</param>
        /// <param name="aliasName">The alias name for the <see cref="PerformanceCounter"/> object.</param>
        /// <param name="valueUnit">The measurement unit for the statistical values of the <see cref="PerformanceCounter"/> object.</param>
        /// <param name="valueDivisor">The divisor to be applied to the statistical values of the <see cref="PerformanceCounter"/> object.</param>
        /// <param name="readOnly">Flag that determines if this counter is read-only.</param>
        public PerformanceCounter(string categoryName, string counterName, string instanceName, string aliasName, string valueUnit, float valueDivisor, bool readOnly = true)
        {
            AliasName = aliasName;
            ValueUnit = valueUnit;
            ValueDivisor = valueDivisor;

            m_samplingWindow = DefaultSamplingWindow;
            m_samplesLock = new object();
            m_samples = new List<float>();
            m_counter = new System.Diagnostics.PerformanceCounter(categoryName, counterName, instanceName);

            if (!readOnly)
                m_counter.ReadOnly = false;

            Reset();
        }

        // Create a combined performance counter from multiple similar sources
        internal PerformanceCounter(PerformanceCounter[] sources)
        {
            if (sources is null)
                throw new ArgumentNullException(nameof(sources));

            if (sources.Length < 1)
                throw new ArgumentOutOfRangeException(nameof(sources));

            PerformanceCounter initialCounter = sources[0];

            if (initialCounter is null)
                throw new InvalidOperationException("No valid performance counters available");

            m_aliasName = initialCounter.m_aliasName;
            m_valueUnit = initialCounter.m_valueUnit;
            m_valueDivisor = initialCounter.m_valueDivisor;
            m_samplingWindow = initialCounter.m_samplingWindow;
            m_counter = initialCounter.m_counter;

            m_samplesLock = new object();
            m_samples = new List<float>(Enumerable.Repeat(0.0F, sources.Max(c => c.m_samples.Count)));

            for (int i = 0; i < m_samples.Count; i++)
            {
                foreach (PerformanceCounter source in sources)
                {
                    if (source.m_samples.Count > i)
                        m_samples[i] += source.m_samples[i];
                }
            }

            m_lifetimeMaximum = sources.Max(c => c.m_lifetimeMaximum);
            m_lifetimeTotal = sources.Sum(c => c.m_lifetimeTotal);
            m_lifetimeSampleCount = sources.Max(c => c.m_lifetimeSampleCount);
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PerformanceCounter" /> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PerformanceCounter() =>
            Dispose(false);

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets an alias name for the <see cref="PerformanceCounter"/>.
        /// </summary>
        public string AliasName
        {
            get => m_aliasName;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_aliasName = value;
            }
        }

        /// <summary>
        /// Gets or sets the measurement unit of <see cref="LastValue"/>, <see cref="MinimumValue"/>, 
        /// <see cref="MaximumValue"/> and <see cref="AverageValue"/>
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being assigned is a null or empty string.</exception>
        public string ValueUnit
        {
            get => m_valueUnit;
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException(nameof(value));

                m_valueUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the divisor to be applied to the <see cref="LastValue"/>, <see cref="MinimumValue"/>, 
        /// <see cref="MaximumValue"/> and <see cref="AverageValue"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is not greater than 0.</exception>
        public float ValueDivisor
        {
            get => m_valueDivisor;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than 0");

                m_valueDivisor = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of samples to use to determine the <see cref="LastValue"/>, 
        /// <see cref="MinimumValue"/>, <see cref="MaximumValue"/> and <see cref="AverageValue"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being assigned is not greater than 0.</exception>
        public int SamplingWindow
        {
            get => m_samplingWindow;
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than 0");

                m_samplingWindow = value;
            }
        }

        /// <summary>
        /// Gets a list of sampled values from the <see cref="BaseCounter"/>
        /// </summary>
        public List<float> Samples
        {
            get
            {
                lock (m_samplesLock)
                    return new List<float>(m_samples);
            }
        }

        /// <summary>
        /// Gets the last sample value from the samples of the <see cref="BaseCounter"/>.
        /// </summary>
        public float LastValue
        {
            get
            {
                lock (m_samplesLock)
                {
                    if (m_samples.Count <= 0)
                        return float.NaN;

                    return m_samples[m_samples.Count - 1] / m_valueDivisor;
                }
            }
        }

        /// <summary>
        /// Gets the minimum sample value from the samples of the <see cref="BaseCounter"/>.
        /// </summary>
        public float MinimumValue
        {
            get
            {
                lock (m_samplesLock)
                {
                    if (m_samples.Count <= 0)
                        return float.NaN;

                    return m_samples.Min() / m_valueDivisor;
                }
            }
        }

        /// <summary>
        /// Gets the maximum sample value from the samples of the <see cref="BaseCounter"/>.
        /// </summary>
        public float MaximumValue
        {
            get
            {
                lock (m_samplesLock)
                {
                    if (m_samples.Count <= 0)
                        return float.NaN;

                    return m_samples.Max() / m_valueDivisor;
                }
            }
        }

        /// <summary>
        /// Gets the average value from the samples of the <see cref="BaseCounter"/>.
        /// </summary>
        public float AverageValue
        {
            get
            {
                lock (m_samplesLock)
                {
                    if (m_samples.Count <= 0)
                        return float.NaN;

                    return m_samples.Average() / m_valueDivisor;
                }
            }
        }

        /// <summary>
        /// Gets the maximum sample value over the entire lifetime of the <see cref="BaseCounter"/>.
        /// </summary>
        public float LifetimeMaximumValue => m_lifetimeMaximum / m_valueDivisor;

        /// <summary>
        /// Gets the average sample value over the entire lifetime of the <see cref="BaseCounter"/>.
        /// </summary>
        public float LifetimeAverageValue
        {
            get
            {
                if (m_lifetimeSampleCount > 0)
                    return (float)(m_lifetimeTotal / m_lifetimeSampleCount) / m_valueDivisor;

                return float.NaN;
            }
        }

        /// <summary>
        /// Gets the total values sampled over the entire lifetime of the <see cref="BaseCounter"/>.
        /// </summary>
        public long LifetimeSampleCount => m_lifetimeSampleCount;

        /// <summary>
        /// Gets the <see cref="System.Diagnostics.PerformanceCounter"/> object that this <see cref="PerformanceCounter"/> objects wraps.
        /// </summary>
        public System.Diagnostics.PerformanceCounter BaseCounter => m_counter;

        /// <summary>
        /// Gets or sets an optional custom sample adjustment function. Can be used to apply linear adjustments to sampled values.
        /// </summary>
        public Func<float, float> SampleAdjuster { get; set; }

        /// <summary>
        /// Gets or sets an optional custom sample filter function. Can be used to skip sampled values that are unreasonable.
        /// </summary>
        /// <remarks>
        /// Return <c>true</c> if sample should be filtered; otherwise, <c>false</c>.
        /// </remarks>
        public Func<float, bool> SampleFilter { get; set; }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="PerformanceCounter" /> object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="PerformanceCounter" /> object and optionally 
        /// releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            try
            {
                // This will be done regardless of whether the object is finalized or disposed.
                if (disposing)
                {
                    // This will be done only when the object is disposed by calling Dispose().
                    if (!(m_counter is null))
                        m_counter.Dispose();

                    m_counter = null;
                }

                Reset();
            }
            finally
            {
                m_disposed = true; // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Obtains a sample value from the <see cref="BaseCounter"/>.
        /// </summary>
        public void Sample()
        {
            try
            {
                if (m_counter is null)
                    return;

                float currentSample = m_counter.NextValue();

                if (SampleFilter?.Invoke(currentSample) ?? false)
                    return;

                if (!(SampleAdjuster is null))
                    currentSample = SampleAdjuster(currentSample);

                lock (m_samplesLock)
                {
                    // Update counter sample set
                    m_samples.Add(currentSample);

                    // Maintain counter samples rolling window size
                    while (m_samples.Count > m_samplingWindow)
                        m_samples.RemoveAt(0);
                }

                // Track lifetime maximum value
                if (currentSample > m_lifetimeMaximum)
                    m_lifetimeMaximum = currentSample;

                // Track lifetime average components
                checked
                {
                    try
                    {
                        m_lifetimeSampleCount++;
                        m_lifetimeTotal += (decimal)currentSample;
                    }
                    catch (OverflowException)
                    {
                        // If we overflow lifetime total, we restart total with current sample
                        m_lifetimeSampleCount = 1;
                        m_lifetimeTotal = (decimal)currentSample;
                    }
                }
            }
            catch
            {
                // Not failing if counters cannot be sampled
            }
        }

        /// <summary>
        /// Resets the <see cref="PerformanceCounter"/> object to its initial state.
        /// </summary>
        public void Reset()
        {
            lock (m_samplesLock)
                m_samples.Clear();
        }

        #endregion
    }
}