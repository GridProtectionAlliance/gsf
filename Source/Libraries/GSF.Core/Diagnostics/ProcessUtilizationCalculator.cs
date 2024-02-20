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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;

namespace GSF.Diagnostics
{
    /// <summary>
    /// Represents a utilization calculator for a related <see cref="Process"/> set.
    /// </summary>
    public sealed class ProcessUtilizationCalculator : IDisposable
    {
        #region [ Members ]

        // Nested Types
        private class ProcessReference
        {
            public WeakReference<Process> Reference { private get; set; }

            public Process Process => Reference.TryGetTarget(out Process process) ? process : null;

            public TimeSpan LastTotalProcessorTime;
            public DateTime LastCalculationTime;
        }

        // Constants

        /// <summary>
        /// Default value for the <see cref="UpdateInterval"/> property.
        /// </summary>
        public const int DefaultUpdateInterval = 5000;

        // Events

        /// <summary>
        /// Provides status messages to consumer.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T}.Argument"/> is new status message.
        /// </remarks>
        public event EventHandler<EventArgs<string>> StatusMessage;

        // Fields
        private readonly Timer m_updateUtilizationTimer;
        private readonly List<ProcessReference> m_processReferences;
        private int m_updateInterval;
        private bool m_disposed;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new <see cref="ProcessUtilizationCalculator"/>.
        /// </summary>
        public ProcessUtilizationCalculator()
        {
            m_processReferences = new List<ProcessReference>();
            m_updateInterval = DefaultUpdateInterval;

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
        /// Gets the current processor utilization, percent between 0.0 and 1.0, of the associated <see cref="Process"/> set.
        /// </summary>
        public double Utilization { get; private set; }

        /// <summary>
        /// Gets or sets the interval, in milliseconds, over which <see cref="Utilization"/> will be calculated.
        /// </summary>
        public int UpdateInterval
        {
            get => m_updateInterval;
            set
            {
                m_updateInterval = value;

                if (m_updateInterval > 0)
                    m_updateUtilizationTimer.Interval = m_updateInterval;
            }
        }

        /// <summary>
        /// Gets associated processes for this <see cref="ProcessUtilizationCalculator"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="ProcessUtilizationCalculator"/> maintains a <see cref="WeakReference{T}"/> to the associated
        /// <see cref="Process"/> so this property can be <c>null</c> if the process is no longer available.
        /// </remarks>
        public Process[] AssociatedProcesses => m_processReferences.Select(reference => reference.Process).ToArray();

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
        /// Starts calculating the total processor utilization of the specified <paramref name="processes"/>.
        /// </summary>
        /// <param name="processes">The <see cref="Process"/> set, e.g., parent and child processes, to monitor for total processor utilization.</param>
        public void Initialize(params Process[] processes)
        {
            Initialize((IEnumerable<Process>)processes);
        }

        /// <summary>
        /// Starts calculating the total processor utilization of the specified <paramref name="processes"/>.
        /// </summary>
        /// <param name="processes">The <see cref="Process"/> set, e.g., parent and child processes, to monitor for total processor utilization.</param>
        public void Initialize(IEnumerable<Process> processes)
        {
            m_processReferences.AddRange(processes
                .Where(process => process is not null)
                .Select(process => new ProcessReference
                {
                    Reference = new WeakReference<Process>(process),
                    LastTotalProcessorTime = process.TotalProcessorTime,
                    LastCalculationTime = DateTime.UtcNow
                })
            );

            if (m_processReferences.Count == 0)
            {
                OnStatusMessage(MessageLevel.Warning, "No processes to monitor, utilization timer canceled");
            }
            else
            {
                if (m_updateInterval > 0)
                    m_updateUtilizationTimer.Start();
                else
                    OnStatusMessage(MessageLevel.Warning, "Update interval is zero, utilization timer disabled. Updates to utilization will occur after refresh call.");
            }
        }

        /// <summary>
        /// Refreshes the processor utilization of the associated <see cref="Process"/> set.
        /// </summary>
        public void Refresh()
        {
            List<int> itemsToRemove = new();
            double totalUtilization = 0.0D;

            for (int i = 0; i < m_processReferences.Count; i++)
            {
                ProcessReference reference = m_processReferences[i];
                Process process = reference.Process;

                try
                {
                    if (process is not null && !process.HasExited)
                    {
                        process.Refresh();

                        DateTime currentTime = DateTime.UtcNow;
                        TimeSpan totalProcessorTime = process.TotalProcessorTime;

                        totalUtilization += (totalProcessorTime - reference.LastTotalProcessorTime).TotalMilliseconds / (Environment.ProcessorCount * currentTime.Subtract(reference.LastCalculationTime).TotalMilliseconds);

                        reference.LastTotalProcessorTime = totalProcessorTime;
                        reference.LastCalculationTime = currentTime;
                    }
                    else
                    {
                        itemsToRemove.Add(i);
                    }
                }
                catch (Exception ex)
                {
                    OnStatusMessage(MessageLevel.Error, $"Failed to refresh process utilization due to an exception, process removed from monitor list: {ex.Message}");
                    itemsToRemove.Add(i);
                }
            }

            Utilization = totalUtilization;

            if (itemsToRemove.Count == 0)
                return;

            // Process items to remove in reverse order to avoid index shifting
            itemsToRemove.Reverse();

            foreach (int index in itemsToRemove)
                m_processReferences.RemoveAt(index);
        }

        private void UpdateUtilizationTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Refresh();

                if (m_processReferences.Count > 0)
                    return;

                OnStatusMessage(MessageLevel.Warning, "No processes to left monitor, utilization timer canceled");
                
                Utilization = 0.0D;
                m_updateUtilizationTimer.Enabled = false;
            }
            catch (Exception ex)
            {
                OnStatusMessage(MessageLevel.Warning, $"Failed to update processor utilization due to an exception, utilization timer canceled: {ex.Message}");
                
                Utilization = 0.0D;
                m_updateUtilizationTimer.Enabled = false;
            }

        }
        private void OnStatusMessage(MessageLevel level, string status)
        {
            try
            {
                using (Logger.SuppressLogMessages())
                    StatusMessage?.Invoke(this, new EventArgs<string>($"{level.ToString().ToUpperInvariant()}: {status}"));
            }
            catch (Exception ex)
            {
                Logger.SwallowException(ex);
            }
        }

        #endregion
    }
}