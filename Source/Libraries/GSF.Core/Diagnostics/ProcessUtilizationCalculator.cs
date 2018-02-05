//******************************************************************************************************
//  ProcessUtilizationCalculator.cs - Gbtc
//
//  Copyright © 2018, Grid Protection Alliance.  All Rights Reserved.
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
//  02/05/2018 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.Diagnostics;
using System.Timers;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Represents a utilization calculator for a <see cref="Process"/>.
    /// </summary>
    public sealed class ProcessUtilizationCalculator : IDisposable
    {
        #region [ Members ]

        // Constants

        /// <summary>
        /// Default value for the <see cref="UpdateInterval"/> property.
        /// </summary>
        public const int DefaultUpdateInterval = 5000;

        // Fields
        private readonly Timer m_updateUtilizationTimer;
        private Process m_process;
        private int m_updateInterval;
        private TimeSpan m_startTime;
        private TimeSpan m_lastProcessorTime;
        private DateTime m_lastMonitorTime;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ProcessUtilizationCalculator"/>.
        /// </summary>
        public ProcessUtilizationCalculator()
        {
            m_updateInterval = DefaultUpdateInterval;
            m_lastProcessorTime = new TimeSpan(0L);

            m_updateUtilizationTimer = new Timer(m_updateInterval)
            {
                AutoReset = true,
                Enabled = false
            };

            m_updateUtilizationTimer.Elapsed += UpdateUtilizationTimerElapsed;
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the current processor utilization of the associated <see cref="Process"/>.
        /// </summary>
        public double Utilization { get; private set; }

        /// <summary>
        /// Gets or sets the interval, in milliseconds, over which <see cref="Utilization"/> will be calculated.
        /// </summary>
        public int UpdateInterval
        {
            get
            {
                return m_updateInterval;
            }
            set
            {
                m_updateInterval = value;

                if (m_updateInterval > 0)
                    m_updateUtilizationTimer.Interval = m_updateInterval;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Releases all the resources used by the <see cref="ProcessUtilizationCalculator"/> object.
        /// </summary>
        public void Dispose()
        {
            if (m_disposed)
                return;

            try
            {
                m_updateUtilizationTimer.Stop();
                m_updateUtilizationTimer.Elapsed -= UpdateUtilizationTimerElapsed;
                m_updateUtilizationTimer.Dispose();
            }
            finally
            {
                m_disposed = true;  // Prevent duplicate dispose.
            }
        }

        /// <summary>
        /// Starts calculating the processor utilization of the specified <paramref name="process"/>.
        /// </summary>
        /// <param name="process">The <see cref="Process"/> to monitor for processor utilization.</param>
        public void Initialize(Process process)
        {
            m_process = process;
            m_startTime = m_process.TotalProcessorTime;
            m_lastMonitorTime = DateTime.UtcNow;

            if (m_updateInterval > 0)
                m_updateUtilizationTimer.Start();
        }

        private void UpdateUtilizationTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                DateTime currentTime = DateTime.UtcNow;
                TimeSpan processorTime = m_process.TotalProcessorTime - m_startTime;

                Utilization = (processorTime - m_lastProcessorTime).TotalSeconds / (Environment.ProcessorCount * currentTime.Subtract(m_lastMonitorTime).TotalSeconds);

                m_lastMonitorTime = currentTime;
                m_lastProcessorTime = processorTime;
            }
            catch
            {
                m_updateUtilizationTimer.Enabled = false;
            }
        }

        #endregion
    }
}