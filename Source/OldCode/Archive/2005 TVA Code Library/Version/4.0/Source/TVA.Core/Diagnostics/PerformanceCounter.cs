//*******************************************************************************************************
//  PerformanceCounter.cs
//  Copyright © 2008 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: PSO TRAN & REL, CHATTANOOGA - MR 2W-C
//       Phone: 423/751-2250
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/04/2007 - Pinal C. Patel
//       Generated original version of source code.
//  09/22/2008 - James R Carroll
//       Converted to C#.
//  09/30/2008 - Pinal C. Patel
//       Entered code comments.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace TVA.Diagnostics
{
    /// <summary>
    /// A wrapper class to <see cref="System.Diagnostics.PerformanceCounter"/> with additional statistical logic.
    /// </summary>
    /// <example>
    /// This example shows how to create a performance counter for processor utilization:
    /// <code>
    /// using System;
    /// using System.Threading;
    /// using TVA.Diagnostics;
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
        public const float DefaultValueDivisor = 1;

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
        private List<float> m_samples;
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
        public PerformanceCounter(string categoryName, string counterName, string instanceName, string aliasName, string valueUnit, float valueDivisor)
        {
            this.AliasName = aliasName;
            this.ValueUnit = valueUnit;
            this.ValueDivisor = valueDivisor;
            m_samplingWindow = DefaultSamplingWindow;
            m_samples = new List<float>();
            m_counter = new System.Diagnostics.PerformanceCounter(categoryName, counterName, instanceName);
            Reset();
        }

        /// <summary>
        /// Releases the unmanaged resources before the <see cref="PerformanceCounter" /> object is reclaimed by <see cref="GC"/>.
        /// </summary>
        ~PerformanceCounter()
        {
            Dispose(false);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets an alias name for the <see cref="PerformanceCounter"/>.
        /// </summary>
        public string AliasName
        {
            get
            {
                return m_aliasName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();
                m_aliasName = value;
            }
        }

        /// <summary>
        /// Gets or sets the measurement unit of <see cref="LastValue"/>, <see cref="MinimumValue"/>, 
        /// <see cref="MaximumValue"/> and <see cref="AverageValue"/>
        /// </summary>
        /// <exception cref="ArgumentNullException">The value being set is a null or empty string.</exception>
        public string ValueUnit
        {
            get
            {
                return m_valueUnit;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();
                m_valueUnit = value;
            }
        }

        /// <summary>
        /// Gets or sets the divisor to be applied to the <see cref="LastValue"/>, <see cref="MinimumValue"/>, 
        /// <see cref="MaximumValue"/> and <see cref="AverageValue"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being set is not greater than 0.</exception>
        public float ValueDivisor
        {
            get
            {
                return m_valueDivisor;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("ValueDivisor", "Value must be greater than 0.");
                m_valueDivisor = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of samples to use to determine the <see cref="LastValue"/>, 
        /// <see cref="MinimumValue"/>, <see cref="MaximumValue"/> and <see cref="AverageValue"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value being set is not greater than 0.</exception>
        public int SamplingWindow
        {
            get
            {
                return m_samplingWindow;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("SamplingWindow", "Value must be greater than 0.");
                m_samplingWindow = value;
            }
        }

        /// <summary>
        /// Gets a list of sampled values from the <see cref="BaseCounter"/>
        /// </summary>
        /// <remarks>
        /// Thread-safety Warning: Due to the asynchronous nature of <see cref="PerformanceCounter"/>, a lock must be 
        /// obtained on <see cref="Samples"/> before accessing it.
        /// </remarks>
        public List<float> Samples
        {
            get
            {
                return m_samples;
            }
        }

        /// <summary>
        /// Gets the last sample value from the samples of the <see cref="BaseCounter"/>.
        /// </summary>
        public float LastValue
        {
            get
            {
                if (m_samples.Count <= 0)
                    return float.NaN;
                else
                    return m_samples[m_samples.Count - 1] / m_valueDivisor;
            }
        }

        /// <summary>
        /// Gets the minimum sample value from the samples of the <see cref="BaseCounter"/>.
        /// </summary>
        public float MinimumValue
        {
            get
            {
                if (m_samples.Count <= 0)
                    return float.NaN;
                else
                    return m_samples.Min() / m_valueDivisor;
            }
        }

        /// <summary>
        /// Gets the maximum sample value from the samples of the <see cref="BaseCounter"/>.
        /// </summary>
        public float MaximumValue
        {
            get
            {
                if (m_samples.Count <= 0)
                    return float.NaN;
                else
                    return m_samples.Max() / m_valueDivisor;
            }
        }

        /// <summary>
        /// Gets the average value from the samples of the <see cref="BaseCounter"/>.
        /// </summary>
        public float AverageValue
        {
            get
            {
                if (m_samples.Count <= 0)
                    return float.NaN;
                else
                    return m_samples.Average() / m_valueDivisor;
            }
        }

        /// <summary>
        /// Gets the <see cref="System.Diagnostics.PerformanceCounter"/> object that this <see cref="PerformanceCounter"/> objects wraps.
        /// </summary>
        public System.Diagnostics.PerformanceCounter BaseCounter
        {
            get
            {
                return m_counter;
            }
        }

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
            if (!m_disposed)
            {
                try
                {
                    // This will be done regardless of whether the object is finalized or disposed.
                    if (disposing)
                    {
                        // This will be done only when the object is disposed by calling Dispose().
                        if (m_counter != null)
                            m_counter.Dispose();
                    }
                }
                finally
                {
                    m_disposed = true;          // Prevent duplicate dispose.
                }
            }
        }

        /// <summary>
        /// Obtains a sample value from the <see cref="BaseCounter"/>.
        /// </summary>
        public void Sample()
        {
            try
            {
                m_samples.Add(m_counter.NextValue());         // Update counter samples.
                while (m_samples.Count > m_samplingWindow)
                {
                    m_samples.RemoveAt(0);                    // Keep the counter samples window rolling.
                }
            }
            catch (InvalidOperationException)
            {
                // If we're monitoring performance of an application that's not running (it was not running to begin 
                // with, or it was running but it no longer running), we'll encounter an InvalidOperationException 
                // exception. In this case we'll reset the values and absorb the exception.
                Reset();
            }
        }

        /// <summary>
        /// Resets the <see cref="PerformanceCounter"/> object to its initial state.
        /// </summary>
        public void Reset()
        {
            m_samples.Clear();
        }

        #endregion
    }
}